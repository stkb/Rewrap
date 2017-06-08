using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Editor;
using System.Linq;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Rewrap;

namespace VS
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class TextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import(typeof(IVsEditorAdaptersFactoryService))]
        internal IVsEditorAdaptersFactoryService editorFactory = null;

        public void TextViewCreated(IWpfTextView textView)
        {
            var vsTextView = editorFactory.GetViewAdapter(textView);

            CommandFilter.AddToTextView(textView, vsTextView);
        }
    }


    internal class CommandFilter : IOleCommandTarget
    {
        private CommandFilter(IWpfTextView textView, IVsTextView vsTextView)
        {
            this.textView = textView;

            vsTextView.AddCommandFilter(this, out this.next);
        }

        private readonly IWpfTextView textView;
        private readonly IOleCommandTarget next;

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if(pguidCmdGroup == StdCommand.CommandSet && prgCmds[0].cmdID == StdCommand.CommandId)
            {
                prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                return VSConstants.S_OK;
            }
            else
            {
                return next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {             
            if (pguidCmdGroup == StdCommand.CommandSet && nCmdID == StdCommand.CommandId)
            {
                ExecRewrap();
                return VSConstants.S_OK;
            }
            else
            {
                return next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
        }

        public static void AddToTextView(IWpfTextView textView, IVsTextView vsTextView)
        {
            new CommandFilter(textView, vsTextView);
        }

        private void ExecRewrap()
        {
            var snapshot = this.textView.TextBuffer.CurrentSnapshot;

            var edit =
                Core.rewrap(
                    language: GetLanguage(snapshot.TextBuffer),
                    filePath: GetFilePath(snapshot.TextBuffer),
                    selections: GetSelections(snapshot),
                    settings: Environment.GetSettings(this.textView),
                    lines: snapshot.Lines.Select(l => l.GetText())
                );

            if(edit.lines.Length > 0)
            {
                using (var editor = snapshot.TextBuffer.CreateEdit(EditOptions.DefaultMinimalChange, null, null))
                {
                    var eol = snapshot.Lines.First().GetLineBreakText();
                    if (String.IsNullOrEmpty(eol)) eol = textView.Options.GetNewLineCharacter();

                    var startPos =
                        snapshot.GetLineFromLineNumber(edit.startLine).Start.Position;
                    var endPos =
                        snapshot.GetLineFromLineNumber(edit.endLine).End.Position;
                    var wrappedText = 
                        String.Join(eol, edit.lines);

                    editor.Replace(startPos, endPos - startPos, wrappedText);
                    editor.Apply();
                }
            }
        }

        private string GetLanguage(ITextBuffer textBuffer)
        {
            string language = 
                textBuffer.ContentType.TypeName.Split('.').Reverse().First().ToLower();

            // This happens with HTML files, and possibly some others.
            if(language == "projection" &&
                textBuffer.Properties.TryGetProperty("IdentityMapping", out textBuffer)
            )
            {
                return GetLanguage(textBuffer);
            }

            return language;
        }

        private string GetFilePath(ITextBuffer textBuffer)
        {
            if(textBuffer.Properties
                .TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
            {
                return doc.FilePath;
            }

            // HTML files (and possibly some others) have an extra level of indirection.
            // Hopefully there isn't a case where this causes an infinite loop.
            if (textBuffer.Properties
                .TryGetProperty("IdentityMapping", out textBuffer))
            {
                return GetFilePath(textBuffer);
            }

            return "";
        }

        private Selection[] GetSelections(ITextSnapshot snapshot)
        {
            return new Selection[]
            {
                new Selection(
                    GetPosition(snapshot, this.textView.Selection.ActivePoint),
                    GetPosition(snapshot, this.textView.Selection.AnchorPoint)
                )
            };
        }

        private Position GetPosition(ITextSnapshot snapshot, VirtualSnapshotPoint point)
        {
            var offset = point.Position.Position;
            var line = snapshot.GetLineFromPosition(offset);
            var column = offset - line.Start.Position;

            return new Position(line.LineNumber, column);
        }
    }

    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class StdCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("03647b07-e8ca-4d6e-9aae-5b89bbd3276d");

        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="StdCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private StdCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(null, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }


        public static StdCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private System.IServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new StdCommand(package);
        }

    }
}
