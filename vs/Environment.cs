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
                page.Reformat,
                page.WholeComment
            );
        }

        private static SettingsPage Page;


        public class SettingsPage : DialogPage
        {
            [Category("Rewrap options")]
            [DisplayName("Double sentence spacing")]
            [Description("When wrapping lines that end in a period, adds two spaces after that sentence in the wrapped text.")]
            public bool DoubleSentenceSpacing { get; set; } = false;


            [Category("Rewrap options")]
            [DisplayName("Reformat")]
            [Description("(EXPERIMEMTAL) When wrapping lines, reformat paragraph indents.")]
            public bool Reformat { get; set; } = false;


            [Category("Rewrap options")]
            [DisplayName("Wrapping column")]
            [Description("Column to wrap at.")]
            public int WrappingColumn { get; set; } = 80;


            [Category("Rewrap options")]
            [DisplayName("Wrap whole comments")]
            [Description("With the cursor inside a comment block, wrap the whole comment block instead of just a single paragraph.")]
            public bool WholeComment { get; set; } = false;
        }
    }
}
