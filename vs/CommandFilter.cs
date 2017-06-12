using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace VS
{
    class CommandFilter : IOleCommandTarget
    {
        /// <summary>
        /// Adds a new CommandFilter object to the given text view
        /// </summary>
        public static void AddToTextView(IWpfTextView textView, IVsTextView vsTextView)
        {
            new CommandFilter(textView, vsTextView);
        }


        private CommandFilter(IWpfTextView textView, IVsTextView vsTextView)
        {
            this.textView = textView;

            vsTextView.AddCommandFilter(this, out this.next);
        }


        private readonly IWpfTextView textView;
        private readonly IOleCommandTarget next;


        /// <summary>
        /// Queries the status of the command(s). Returns success if it's one of
        /// our commands, else passes the query to the next handler.
        /// </summary>
        /// <param name="CmdSetID"></param>
        /// <param name="cCmds"></param>
        /// <param name="prgCmds"></param>
        /// <param name="pCmdText"></param>
        public int QueryStatus(ref Guid CmdSetID, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (CmdSetID == StdCommand.SetGuid && prgCmds[0].cmdID == StdCommand.ID)
            {
                prgCmds[0].cmdf = 
                    (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED );
                return VSConstants.S_OK;
            }
            else
            {
                return next.QueryStatus(ref CmdSetID, cCmds, prgCmds, pCmdText);
            }
        }


        public int Exec(ref Guid CmdSetID, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (CmdSetID == StdCommand.SetGuid && nCmdID == StdCommand.ID)
            {
                Editor.ExecRewrap(this.textView);
                return VSConstants.S_OK;
            }
            else
            {
                return next.Exec(ref CmdSetID, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
        }
    }


    /// <summary>
    /// Watches for newly-opened text views and attaches CommandFilters to them.
    /// </summary>
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
}
