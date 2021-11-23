using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Rewrap;
using System;
using System.Linq;
using VS.Options;

namespace VS
{
    /// Gets the needed information from the text editor to perform the re-wrap, and
    /// applies the returned edit.
    static class Editor
    {
        /// Does a standard wrap for the given text view.
        public static void StandardWrap(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var textBuffer = textView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            var file = new File(GetLanguage(textBuffer), GetFilePath(textBuffer), () => Core.noCustomMarkers);
            DocState getDocState(ITextSnapshot ss) =>
                new DocState(file.path, ss.Version.VersionNumber, GetSelections(textView, ss));

            var settings = GetSettings
                (textView, rs => Core.maybeChangeWrappingColumn(getDocState(snapshot), rs));

            var edit = Core.rewrap
                (file, settings, GetSelections(textView, snapshot), DocLine(snapshot));
            if (ApplyEdit(textView, snapshot, edit))
                Core.saveDocState(getDocState(textBuffer.CurrentSnapshot));
        }

        /// If conditions are met, does an auto-wrap for the given text view.
        public static void AutoWrap(IWpfTextView textView, int pos, string newText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var textBuffer = textView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            var file = new File(GetLanguage(textBuffer), GetFilePath(textBuffer), () => Core.noCustomMarkers);

            var settings = GetSettings
                (textView, rs => Core.getWrappingColumn(file.path, rs));

            var ssPos = GetPosition(snapshot, pos);
            var edit = Core.maybeAutoWrap(file, settings, newText, ssPos, DocLine(snapshot));
            ApplyEdit(textView, snapshot, edit);
        }

        /// Gets settings for the given text view
        private static Settings GetSettings
            (IWpfTextView textView, Func<int[], int> getColumn)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var textBuffer = textView.TextBuffer;
            var file = new File(GetLanguage(textBuffer), GetFilePath(textBuffer), () => Core.noCustomMarkers);
            var options = OptionsPage.GetOptions(file);

            int[] ifNotEmpty (int[] x) { return x.Length > 0 ? x : null; }
            var rulers =
                (options.WrappingColumn.HasValue ? new[] { options.WrappingColumn.Value } : null)
                ?? ifNotEmpty (GetRulersFromEditor(textView))
                ?? ifNotEmpty (GetRulersFromRegistry())
                ?? new[] { 80 };

            return new Settings
                ( getColumn(rulers)
                , textView.Options.GetTabSize()
                , options.DoubleSentenceSpacing
                , options.Reformat
                , options.WholeComment
                );
        }

        /// Applies the given Edit to the given text view snapshot.
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

        /// Gets the document type (language) for the given text buffer.
        static string GetLanguage(ITextBuffer textBuffer)
        {
            string language =
                textBuffer.ContentType.TypeName.Split( '.' ).Reverse().First().ToLower();

            // This happens with HTML files, and possibly some others.
            if ( language.EndsWith("projection") &&
                textBuffer.Properties.TryGetProperty( "IdentityMapping", out textBuffer )
            )
            {
                return GetLanguage( textBuffer );
            }

            return language;
        }

        /// Gets the file path of the document in the given text buffer.
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

        /// Gets a Rewrap Position object from a snapshot point.
        static Position GetPosition(ITextSnapshot snapshot, VirtualSnapshotPoint point)
        {
            return GetPosition(snapshot, point.Position.Position);
        }
        static Position GetPosition(ITextSnapshot snapshot, int offset)
        {
            var line = snapshot.GetLineFromPosition(offset);
            var column = offset - line.Start.Position;
            return new Position(line.LineNumber, column);
        }


        /// Given a snapshot, returns a function that gets a text line for the given line
        /// index.
        private static Func<int,string> DocLine(ITextSnapshot snapshot)
        {
            var lineCount = snapshot.LineCount;
            return i => i < lineCount ?
                    snapshot.GetLineFromLineNumber(i).GetText() : null;
        }

        /// Gets the OptionsPage from the package.
        static OptionsPage OptionsPage
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

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

        /// Gets rulers added as adornments in the editor by the Editor
        /// Guidelines extension. These are added by that extension if they are
        /// set in .editorconfig, and in that case are not added to the registry
        static int[] GetRulersFromEditor(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                return textView.GetAdornmentLayer("ColumnGuide").Elements
                    .Select(x => {
                        var data = (x.Adornment as System.Windows.Shapes.Line).DataContext;
                        return (int)data.GetType().GetProperty("Column").GetValue(data);
                    })
                    .ToArray();
            }
            catch { return Array.Empty<int>(); }
        }

        /// Gets editor rulers (guides) from the registry. Returns an empty array if none
        /// are set.
        static int[] GetRulersFromRegistry()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var shell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell))
                ?? throw new Exception("Rewrap: Couldn't get IVsShell instance");
            shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out object value);
            if(value is string rawVersion)
            {
                // Before v17, VS would always give eg "16.0" as the version number even
                // if it was actually 16.1, 16.2 etc. Now v17.1 returns "17.1". However
                // the registry path to the guides is still at ...\17.0\.... So we always
                // try <major>.0 first, and then <major>.<minor> in case things change in
                // the future.
                var parts = rawVersion.Split('.');
                string major = parts[0], minor = parts[1];
                string tryVersion (string v) {
                    var path = $@"HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\{v}\Text Editor";
                    // GetValue only returns the given default value if the reg value
                    // doesn't exist. If the whole key doesn't exist, it returns null.
                    return (string)Microsoft.Win32.Registry.GetValue(path, "Guides", null);
                }

                var guidesStr = tryVersion($"{major}.0") ?? tryVersion($"{major}.{minor}") ?? ")";
                var rulers =
                    guidesStr
                        .Split(')')[1]
                        .Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                        .Select( s => Int32.Parse( s.Trim() ) ).ToArray();

                return rulers;
            }
            else return Array.Empty<int>();

        }
    }
}
