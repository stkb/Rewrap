using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Rewrap;
using System;
using System.Collections.Generic;
using System.Linq;
using VS.Options;

namespace VS
{
    /// <summary>
    /// Gets the needed information from the text editor to perform the re-wrap,
    /// and applies the returned edit.
    /// </summary>
    static class Editor
    {
        /// Does a standard wrap for the given text view.
        public static void StandardWrap(IWpfTextView textView)
        {
            var docState = ExecRewrap( Core.rewrap, textView );
            // Save new doc state (selections will have changed)
            if(docState != null) Core.saveDocState( docState );
        }

        /// Does an auto-wrap for the given text view.
        public static void AutoWrap(IWpfTextView textView)
        {
            var tabSize = textView.Options.GetTabSize();
            var pos = GetSelections( textView, textView.TextBuffer.CurrentSnapshot )[0].active;
            if
                (Core.cursorBeforeWrappingColumn
                    ( GetFilePath( textView.TextBuffer )
                    , tabSize
                    , textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(pos.line).GetText()
                    , pos.character
                    , () => GetRulers()[0]
                    )
                )
            {
                return;
            }

            ExecRewrap( Core.autoWrap, textView );
        }

        static DocState ExecRewrap
            ( Func<DocState, Settings, IEnumerable<String>, Edit> wrapFn
            , IWpfTextView textView
            )
        {
            // Prerequisites
            var textBuffer = textView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            var filePath = GetFilePath(textBuffer);
            var language = GetLanguage(textBuffer);
            var options = OptionsPage.GetOptions( language, filePath );


            // Info for wrap
            var settings = GetSettings();
            var lineCount = snapshot.LineCount;
            var docState = GetDocState();       
            var lines = snapshot.Lines.Select( l => l.GetText() );

            // Create edit
            var edit = wrapFn( docState, settings, lines );

            // Apply edit
            if (ApplyEdit( textView, snapshot, edit ))
            {
                snapshot = textBuffer.CurrentSnapshot;
                return GetDocState();
            }
            else return null;



            DocState GetDocState()
            {
                return new DocState
                    ( filePath
                    , language
                    , snapshot.Version.VersionNumber
                    , GetSelections( textView, snapshot )
                    );
            }

            Settings GetSettings()
            {
                int[] wrappingColumns =
                    options.WrappingColumn.HasValue
                        ? new[] { options.WrappingColumn.Value }
                        : GetRulers();

                return new Settings
                    ( 0 // WrappingColumn will later be removed
                    , wrappingColumns
                    , textView.Options.GetTabSize()
                    , options.DoubleSentenceSpacing
                    , options.Reformat
                    , options.WholeComment
                    );
            }
        }


        /// <summary>
        /// Applies the given Edit to the given text view snapshot.
        /// </summary>
        static bool ApplyEdit(IWpfTextView textView, ITextSnapshot snapshot, Edit edit)
        {
            if (edit.lines.Length == 0) return false;

            using
                (var textEdit =
                    snapshot.TextBuffer.CreateEdit
                        ( EditOptions.DefaultMinimalChange, null, null
                        )
                )
            {
                var eol = snapshot.Lines.First().GetLineBreakText();
                if (String.IsNullOrEmpty( eol ))
                    eol = textView.Options.GetNewLineCharacter();

                var startPos =
                    snapshot.GetLineFromLineNumber( edit.startLine ).Start.Position;
                var endPos =
                    snapshot.GetLineFromLineNumber( edit.endLine ).End.Position;
                var wrappedText =
                    String.Join( eol, edit.lines );

                textEdit.Replace( startPos, endPos - startPos, wrappedText );
                textEdit.Apply();
            }

            return true;
        }


        /// <summary>
        /// Gets the document type (language) for the given text buffer.
        /// </summary>
        static string GetLanguage(ITextBuffer textBuffer)
        {
            string language =
                textBuffer.ContentType.TypeName.Split( '.' ).Reverse().First().ToLower();

            // This happens with HTML files, and possibly some others.
            if ( language == "projection" &&
                textBuffer.Properties.TryGetProperty( "IdentityMapping", out textBuffer )
            )
            {
                return GetLanguage( textBuffer );
            }

            return language;
        }


        /// <summary>
        /// Gets the file path of the document in the given text buffer.
        /// </summary>
        static string GetFilePath(ITextBuffer textBuffer)
        {
            if ( textBuffer.Properties
                .TryGetProperty( typeof( ITextDocument ), out ITextDocument doc ) )
            {
                return doc.FilePath;
            }

            // HTML files (and possibly some others) have an extra level of indirection.
            // Hopefully there isn't a case where this causes an infinite loop.
            if ( textBuffer.Properties
                .TryGetProperty( "IdentityMapping", out textBuffer ) )
            {
                return GetFilePath( textBuffer );
            }

            return "";
        }


        /// <summary>
        /// Gets the selection positions in the given text snapshot.
        /// </summary>
        static Selection[] GetSelections(IWpfTextView textView, ITextSnapshot snapshot)
        {
            return new Selection[]
                { new Selection
                    ( GetPosition(snapshot, textView.Selection.AnchorPoint)
                    , GetPosition(snapshot, textView.Selection.ActivePoint)
                    )
                };
        }


        /// <summary>
        /// Gets a Rewrap Position object from a snapshot point.
        /// </summary>
        static Position GetPosition(ITextSnapshot snapshot, VirtualSnapshotPoint point)
        {
            var offset = point.Position.Position;
            var line = snapshot.GetLineFromPosition( offset );
            var column = offset - line.Start.Position;

            return new Position( line.LineNumber, column );
        }


        /// <summary>
        /// Gets the OptionsPage from the package.
        /// </summary>
        static OptionsPage OptionsPage
        {
            get
            {
                if(_OptionsPage == null)
                {
                    var shell = (IVsShell)ServiceProvider.GlobalProvider.GetService( typeof( IVsShell ) )
                        ?? throw new Exception( "Rewrap: Couldn't get IVsShell instance" );

                    var packageGuid = new Guid( RewrapPackage.GuidString );
                    if ( shell.IsPackageLoaded( packageGuid, out IVsPackage vsPackage ) != VSConstants.S_OK )
                    {
                        shell.LoadPackage( packageGuid, out vsPackage );
                    }
                    Package package = vsPackage as Package 
                        ?? throw new Exception( "Rewrap: Couldn't get package instance" );

                    _OptionsPage = package.GetDialogPage( typeof( OptionsPage ) ) as OptionsPage
                        ?? throw new Exception( "Rewrap: Couldn't get OptionsPage instance" );
                }
                return _OptionsPage;
            }
        }

        static OptionsPage _OptionsPage;


        /// Gets editor rulers (guides) from the registry
        static int[] GetRulers()
        {
            var dte = (EnvDTE.DTE)Package.GetGlobalService( typeof( EnvDTE.DTE ) );
            string path =
                @"HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\" + dte.Version + @"\Text Editor";

            var guidesStr =
                (string)Microsoft.Win32.Registry.GetValue( path, "Guides", ")" );
            var rulers =
                guidesStr
                    .Split(')')[1]
                    .Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( s => Int32.Parse( s.Trim() ) ).ToArray();

            return rulers;
        }
    }
}