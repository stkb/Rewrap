using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Rewrap;
using System;
using System.ComponentModel;

namespace VS
{
    class Environment
    {
        public static Settings GetSettings(IWpfTextView editor)
        {
            if (Page != null)
                return GetSettings(editor, Page);

            var shell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
            if (shell == null)
                throw new Exception("Rewrap: Couldn't get IVsShell instance");

            IVsPackage vsPackage = null;
            if(shell.IsPackageLoaded(RewrapPackage.Guid, out vsPackage) != VSConstants.S_OK)
            {
                shell.LoadPackage(RewrapPackage.Guid, out vsPackage);
            }
            Package package = vsPackage as Package;
            if (package == null)
                throw new Exception("Rewrap: Couldn't get package instance");

            Page = (SettingsPage)package.GetDialogPage(typeof(SettingsPage));
            if (Page == null)
                throw new Exception("Rewrap: Couldn't get SettingsPage instance");

            return GetSettings(editor, Page);         
        }

        private static Settings GetSettings(IWpfTextView editor, SettingsPage page)
        {
            return new Settings(
                page.WrappingColumn,
                editor.Options.GetTabSize(),
                page.DoubleSentenceSpacing,
                page.Reformat,
                page.WholeComment
            );
        }

        private static SettingsPage Page;


        public class SettingsPage : DialogPage
        {
            [Category("Global options")]
            [DisplayName("\t\t\tWrapping column")]
            [Description("Column to wrap at. Eg: 80 will wrap after 80 characters.")]
            public int WrappingColumn { get; set; }

            [Category( "Global options" )]
            [DisplayName("\t\tWrap whole comments")]
            [Description("(Default: true) With the cursor inside a comment block, wrap the whole\r\ncomment block instead of just a single paragraph.")]
            public bool WholeComment { get; set; }

            [Category( "Global options" )]
            [DisplayName("\tDouble sentence spacing")]
            [Description("(Default: false) When wrapping lines that end in a period, adds two spaces after\r\nthat sentence in the wrapped text.")]
            public bool DoubleSentenceSpacing { get; set; }

            [Category( "Global options" )]
            [DisplayName("Reformat (experimental)")]
            [Description("(Default: false) When wrapping lines, fix and reformat paragraph indents.")]
            public bool Reformat { get; set; }


            public override void SaveSettingsToStorage()
            {
                this.Store.CreateCollection( "Rewrap\\*" );

                this.Store.SetInt32( "Rewrap\\*", "wrappingColumn", this.WrappingColumn );
                this.Store.SetBoolean( "Rewrap\\*", "wholeComment", this.WholeComment );
                this.Store.SetBoolean( "Rewrap\\*", "doubleSentenceSpacing", this.DoubleSentenceSpacing );
                this.Store.SetBoolean( "Rewrap\\*", "reformat", this.Reformat );
            }

            public override void LoadSettingsFromStorage()
            {
                this.Store.CreateCollection( "Rewrap\\*" );

                WrappingColumn = this.Store.GetInt32( "Rewrap\\*", "wrappingColumn", 60 );
                WholeComment = this.Store.GetBoolean( "Rewrap\\*", "wholeComment", true );
                DoubleSentenceSpacing = this.Store.GetBoolean( "Rewrap\\*", "doubleSentenceSpacing", false );
                Reformat = this.Store.GetBoolean( "Rewrap\\*", "reformat", false );
            }

            private WritableSettingsStore Store
            {
                get
                {
                    if ( this.store == null )
                    {
                        var cModel = (IComponentModel)( Site.GetService( typeof( SComponentModel ) ) );
                        var sp = cModel.GetService<SVsServiceProvider>();
                        var manager = new ShellSettingsManager( sp );
                        this.store = manager.GetWritableSettingsStore( SettingsScope.UserSettings );
                    }
                    return this.store;
                }
            }

            private WritableSettingsStore store;
        }
    }
}
