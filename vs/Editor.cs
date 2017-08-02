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
        /// <summary>
        /// Executes the Rewrap command for the given text view.
        /// </summary>
        public static void ExecRewrap(IWpfTextView textView)
        {
            var snapshot = textView.TextSnapshot;
            var language = GetLanguage( snapshot.TextBuffer );
            var filePath = GetFilePath( snapshot.TextBuffer );

            var edit =
                Core.rewrap
                    ( language: language
                    , filePath: filePath
                    , selections: GetSelections( textView, snapshot )
                    , settings: GetSettings( textView, language, filePath )
                    , lines: snapshot.Lines.Select( l => l.GetText() )
                    );

            ApplyEdit( textView, snapshot, edit );

            var afterSnapshot = textView.TextSnapshot;
            LastWrap = 
                ( filePath
                , afterSnapshot.Version.VersionNumber
                , GetSelections( textView, afterSnapshot )
                );
        }


        /// <summary>
        /// Applies the given Edit to the given text view snapshot.
        /// </summary>
        static void ApplyEdit(IWpfTextView textView, ITextSnapshot snapshot, Edit edit)
        {
            if ( edit.lines.Length > 0 )
            {
                using
                    ( var textEdit =
                        snapshot.TextBuffer.CreateEdit
                            ( EditOptions.DefaultMinimalChange, null, null
                            )
                    )
                {
                    var eol = snapshot.Lines.First().GetLineBreakText();
                    if ( String.IsNullOrEmpty( eol ) )
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
            }
        }


        static Dictionary<string, int> DocWrappingColumns = new Dictionary<string, int>();

        static ValueTuple<string, int, Selection[]> LastWrap = ("", 0, new Selection[] { });

        static int? GetRuler(IWpfTextView editor, string filePath)
        {
            int[] rulers = GetRulers();
            if ( rulers.Length == 0 ) return null;
            if ( rulers.Length == 1 ) return rulers[0];

            var (lastPath, lastVersion, lastSelections) = LastWrap;
            var snapshot = editor.TextBuffer.CurrentSnapshot;

            if (
                lastPath == filePath
                    && lastVersion == snapshot.Version.VersionNumber
                    && Enumerable.SequenceEqual
                        ( lastSelections
                        , GetSelections( editor, snapshot ) 
                        )
            )
            {
                int nextRulerIndex =
                    ( Array.IndexOf<int>( rulers, DocWrappingColumns[filePath] ) + 1 ) 
                        % rulers.Length;
                DocWrappingColumns[filePath] = rulers[nextRulerIndex];
            }
            else if(!DocWrappingColumns.ContainsKey(filePath)) {
                DocWrappingColumns[filePath] = rulers[0];
            }

            return DocWrappingColumns[filePath];
        }


        static Settings GetSettings(IWpfTextView editor, string language, string filePath)
        {
            var options = OptionsPage.GetOptions( language, filePath );

            return new Settings
                ( options.WrappingColumn ?? GetRuler(editor, filePath) ?? 80 
                , editor.Options.GetTabSize()
                , options.DoubleSentenceSpacing
                , options.Reformat
                , options.WholeComment
                );
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
                    ( GetPosition(snapshot, textView.Selection.ActivePoint)
                    , GetPosition(snapshot, textView.Selection.AnchorPoint)
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
                    var shell = (IVsShell)ServiceProvider.GlobalProvider.GetService( typeof( IVsShell ) );
                    if ( shell == null )
                        throw new Exception( "Rewrap: Couldn't get IVsShell instance" );

                    IVsPackage vsPackage = null;
                    if ( shell.IsPackageLoaded( RewrapPackage.Guid, out vsPackage ) != VSConstants.S_OK )
                    {
                        shell.LoadPackage( RewrapPackage.Guid, out vsPackage );
                    }
                    Package package = vsPackage as Package;
                    if ( package == null )
                        throw new Exception( "Rewrap: Couldn't get package instance" );

                    var page = package.GetDialogPage( typeof( OptionsPage ) ) as OptionsPage;
                    if ( page == null )
                        throw new Exception( "Rewrap: Couldn't get OptionsPage instance" );

                    _OptionsPage = page;
                }
                return _OptionsPage;
            }
        }

        static OptionsPage _OptionsPage;

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