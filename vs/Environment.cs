using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Rewrap;
using System;
using System.ComponentModel;
using System.Linq;

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
                page.TidyUpIndents,
                page.WholeComment
            );
        }

        private static SettingsPage Page;


        public class SettingsPage : DialogPage
        {
            [Category("Rewrap options")]
            [DisplayName("Double sentence spacing")]
            [Description("")]
            public bool DoubleSentenceSpacing { get; private set; } = false;


            [Category("Rewrap options")]
            [DisplayName("Tidy up indents")]
            [Description("")]
            public bool TidyUpIndents { get; private set; } = false;


            [Category("Rewrap options")]
            [DisplayName("Wrapping column")]
            [Description("Column to wrap at.")]
            public int WrappingColumn { get; set; } = 80;


            [Category("Rewrap options")]
            [DisplayName("Wrap whole comment")]
            [Description("")]
            public bool WholeComment { get; private set; } = false;
        }
    }
}
