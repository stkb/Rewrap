using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace VS.Options
{
    [System.ComponentModel.DesignerCategory( "Code" )]
    public class OptionsPage : UIElementDialogPage
    {
        GlobalOptionsGroup GlobalOptions;

        List<OptionsGroup> OptionsGroups;


        public Options GetOptions(string language, string filePath)
        {
            var knownLanguage = Rewrap.Core.findLanguage( language, filePath );
            var languageOptions = OptionsGroups.Find( og => og.Languages.Contains( knownLanguage ) );

            return new Options()
            {
                WrappingColumn = languageOptions?.WrappingColumn ?? GlobalOptions.WrappingColumn,
                WholeComment = languageOptions?.WholeComment ?? GlobalOptions.WholeComment,
                DoubleSentenceSpacing = languageOptions?.DoubleSentenceSpacing ?? GlobalOptions.DoubleSentenceSpacing,
                Reformat = languageOptions?.Reformat ?? GlobalOptions.Reformat,
            };

        }

        public override void SaveSettingsToStorage()
        {
            if(ChildControl != null)
            {
                (GlobalOptions, OptionsGroups) = ChildControl.GetOptions();
            }

            // Create methods that write the settings only if they're not null
            Action<string, string, T?> writeProp<T>(Action<string, string, T> writeFn) where T : struct
            {
                return (string collection, string prop, T? value) =>
                {
                    if ( value.HasValue )
                        writeFn( collection, prop, value.Value );
                };
            }
            var writeInt = writeProp<int>( Store.SetInt32 );
            var writeBool = writeProp<bool>( Store.SetBoolean );

            Store.DeleteCollection( "Rewrap" );

            // Save global options
            var globalCollection = "Rewrap\\*";
            Store.CreateCollection( globalCollection );
            writeInt( globalCollection, "wrappingColumn", GlobalOptions.WrappingColumn );
            Store.SetBoolean( globalCollection, "wholeComment", GlobalOptions.WholeComment );
            Store.SetBoolean( globalCollection, "doubleSentenceSpacing", GlobalOptions.DoubleSentenceSpacing );
            Store.SetBoolean( globalCollection, "reformat", GlobalOptions.Reformat );

            // Save language-specific options
            for ( var i = 0; i < OptionsGroups.Count; i++ )
            {
                var group = OptionsGroups[i];
                var collection = "Rewrap\\" + String.Join( ",", group.Languages );
                Store.CreateCollection( collection );
                Store.SetInt32( collection, "index", i );
                writeInt( collection, "wrappingColumn", group.WrappingColumn );
                writeBool( collection, "wholeComment", group.WholeComment );
                writeBool( collection, "doubleSentenceSpacing", group.DoubleSentenceSpacing );
                writeBool( collection, "reformat", group.Reformat );
            }
        }

        public override void LoadSettingsFromStorage()
        {
            Store.CreateCollection( "Rewrap\\*" );
            GlobalOptions = new GlobalOptionsGroup() {
                WrappingColumn = Store.GetInt32( "Rewrap\\*", "wrappingColumn", 80 ),
                WholeComment = Store.GetBoolean( "Rewrap\\*", "wholeComment", true ),
                DoubleSentenceSpacing = Store.GetBoolean( "Rewrap\\*", "doubleSentenceSpacing", false ),
                Reformat = Store.GetBoolean( "Rewrap\\*", "reformat", false ),
            };

            // Read setting but set to null if it doesn't exist
            Func<string, string, T?> readProp<T>(Func<string, string, T> readFn) where T : struct
            {
                return (string collection, string prop) =>
                {
                    try
                    {
                        return readFn( "Rewrap\\" + collection, prop );
                    }
                    catch
                    {
                        return null;
                    }
                };
            }
            var readInt = readProp( Store.GetInt32 );
            var readBool = readProp( Store.GetBoolean );

            (int?, OptionsGroup) readGroup(string name)
            {
                return (
                    readInt( name, "index" ),
                    new OptionsGroup( name.Split( ',' ).Select( s => s.Trim() ) ) {
                        WrappingColumn = readInt( name, "wrappingColumn" ),
                        WholeComment = readBool( name, "wholeComment" ),
                        DoubleSentenceSpacing = readBool( name, "doubleSentenceSpacing" ),
                        Reformat = readBool( name, "reformat" ),
                    }
                 );
            }

            OptionsGroups =
                Store.GetSubCollectionNames( "Rewrap" )
                    .Where( name => name != "*" )
                    .Select( readGroup )
                    .OrderBy( indexAndGroup => indexAndGroup.Item1 )
                    .Select( indexAndGroup => indexAndGroup.Item2 )
                    .ToList();


            if ( ChildControl != null )
            {
                ChildControl.SetOptions( GlobalOptions, OptionsGroups );
            }
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

        OptionsPageControl ChildControl;

        protected override UIElement Child
        {
            get
            {
                if ( ChildControl != null ) return ChildControl;

                ChildControl = new OptionsPageControl();
                ChildControl.SetOptions( GlobalOptions, OptionsGroups );

                return ChildControl;
            }
        }

        private WritableSettingsStore store;
    }


    public class Options
    {
        public int? WrappingColumn { get; set; }

        public bool WholeComment { get; set; }

        public bool DoubleSentenceSpacing { get; set; }

        public bool Reformat { get; set; }
    }

}

