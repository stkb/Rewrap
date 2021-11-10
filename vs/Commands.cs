using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;

namespace VS
{
    /// Standard Rewrap Command
    class RewrapCommand
    {
        public const int ID = 0x0100;

        // Initializes the command for the menu item
        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            if (package == null) throw new ArgumentNullException( "package" );

            if (await package.GetServiceAsync( typeof( IMenuCommandService ) )
                    is OleMenuCommandService commandService
               )
            {
                commandService.AddCommand
                    ( new MenuCommand( null, new CommandID( RewrapPackage.CmdSetGuid, ID ) )
                    );
            }
        }

        /// Called when a new text view is created
        public static void AttachToTextView(IWpfTextView textView, IVsTextView vsTextView)
        {
            new CommandFilter( textView, vsTextView );
        }

        /// A command filter receives status and exec queries from VS for all commands
        /// executed by the user within the context of a text view. We handle this command
        /// this way so that we have a handle to the affected text view (and text therein)
        /// when it is executed.
        ///
        /// An alternative would be handling commmand execution the normal way (via
        /// MenuCommand or OleMenuCommand) and the retrieving the active text view
        /// ourselves from DTE.
        class CommandFilter : IOleCommandTarget
        {
            public CommandFilter(IWpfTextView textView, IVsTextView vsTextView)
            {
                this.textView = textView;
                vsTextView.AddCommandFilter( this, out this.next );
            }

            /// Queries the status of the command(s). Returns success if it's The rewrap
            /// command, else passes the query to the next handler.
            public int QueryStatus(ref Guid CmdSetID, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (CmdSetID == RewrapPackage.CmdSetGuid && prgCmds[0].cmdID == RewrapCommand.ID)
                {
                    prgCmds[0].cmdf =
                        (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                    return VSConstants.S_OK;
                }
                else return next.QueryStatus( ref CmdSetID, cCmds, prgCmds, pCmdText );
            }

            /// Executes the command, if it's the rewrap command.
            public int Exec(ref Guid CmdSetID, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (CmdSetID == RewrapPackage.CmdSetGuid && nCmdID == RewrapCommand.ID)
                {
                    Editor.StandardWrap( this.textView );
                    return VSConstants.S_OK;
                }
                else return next.Exec( ref CmdSetID, nCmdID, nCmdexecopt, pvaIn, pvaOut );
            }

            private readonly IWpfTextView textView;
            private readonly IOleCommandTarget next;
        }

    }


    /// Auto-Wrap Command
    sealed class AutoWrapCommand
    {
        const int ID = 0x0200;

        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (package == null) throw new ArgumentNullException("package");

            var commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService
                    ?? throw new Exception("Rewrap: Couldn't get OleMenuCommandService");
            commandService.AddCommand(MenuCommand);

            StatusBar = await package.GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;

            // Restore if enabled in settings
            var mgr = await package.GetServiceAsync(typeof(SVsSettingsManager)) as IVsSettingsManager
                ?? throw new Exception("Rewrap: Couldn't get IVsSettingsManager");
            mgr.GetWritableSettingsStore((uint)SettingsScope.UserSettings, out SettingsStore);
            if (SettingsStore.GetBool("Rewrap\\*", "autoWrap", out var value) == VSConstants.S_OK && value == 1)
                ToggleEnabled(null, null);
        }

        // MenuCommand vs OleMenuCommand: OleMenuCommand, which has a query method, could
        // be used to query an enabled state held elsewhere. But MenuCommand is enough for
        // us here. It has its own checked state, which we use as the source of the enable
        // state.
        static readonly MenuCommand MenuCommand =
            new MenuCommand(ToggleEnabled, new CommandID(RewrapPackage.CmdSetGuid, ID));

        // Gets if auto-wrap is enabled
        static bool Enabled { get { return MenuCommand.Checked; } }

        static void ToggleEnabled(object _, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            MenuCommand.Checked = !MenuCommand.Checked;
            SettingsStore.SetBool("Rewrap\\*", "autoWrap", Enabled ? 1 : 0);

            // Show a brief message in status bar when toggling on/off. Would like
            // something permanent that shows the status but it's not possible without
            // dirty hacks https://stackoverflow.com/q/30096546
            if (StatusBar != null)
            {
                var msg = Enabled ? "Auto-wrap: On" : "Auto-wrap: Off";
                StatusBar.SetText(msg);
            }
        }

        static IVsWritableSettingsStore SettingsStore;

        static IVsStatusbar StatusBar;

        /// Attaches a change listener to a TextView
        public static void AttachToTextView(IWpfTextView textView)
        {
            textView.TextBuffer.Changed += TextBuffer_Changed;

            void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (!AutoWrapCommand.Enabled) return;

                // Change count can also be 0
                if (e.Changes.Count != 1) return;
                var change = e.Changes[0];
                // Make sure we're in the active document
                if (!textView.HasAggregateFocus) return;

                Editor.AutoWrap(textView, change.NewPosition, change.NewText);
            }
        }
    }


    /// Watches for newly-opened text views and attaches commands to them.
    [Export( typeof( IWpfTextViewCreationListener ) )]
    [ContentType( "text" )]
    [TextViewRole( PredefinedTextViewRoles.Editable )]
    internal sealed class TextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import( typeof( IVsEditorAdaptersFactoryService ) )]
        internal IVsEditorAdaptersFactoryService editorFactory = null;

        public void TextViewCreated(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Some IWpfTextViews that get created aren't code windows (eg tooltips in the
            // "Find all references" pane). In these cases the retrieved IVsTextView will
            // be null, and we ignore it.
            var vsTextView = editorFactory.GetViewAdapter( textView );
            if (vsTextView != null)
            {
                RewrapCommand.AttachToTextView(textView, vsTextView);
                AutoWrapCommand.AttachToTextView(textView);
            }
        }
    }
}
