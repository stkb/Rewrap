using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace VS.Options
{
    /// <summary>
    /// Interaction logic for OptionsPageControl.xaml
    /// </summary>
    public partial class OptionsPageControl : UserControl
    {
        public OptionsPageControl()
        {
            InitializeComponent();
            this.DataContext = Model;
        }

        // Model for the Options page
        public ViewModel Model { get { return _Model; } }
        private readonly ViewModel _Model = new ViewModel();

        // Load options into this control
        public void SetOptions (GlobalOptionsGroup globalOptions, List<OptionsGroup> optionsGroups)
        {
            Model.GlobalOptions = globalOptions;
            Model.OptionsGroups = new ObservableCollection<OptionsGroup>( optionsGroups );
        }

        // Get options from this control
        public (GlobalOptionsGroup, List<OptionsGroup>) GetOptions()
        {
            return (Model.GlobalOptions, Model.OptionsGroups.ToList());
        }


        // Event handlers

        
        // Button to modify/add a new group. Shows the popup to choose languages
        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            SetLanguagesPicker( button.DataContext as OptionsGroup );

            LanguagesPopup.PlacementTarget = button;
            LanguagesPopup.IsOpen = true;
        }

        // Confirms the new languages selection & closes the popup
        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            LanguagesPopup.IsOpen = false;
            AddOrModifyGroup();
        }

        // Closes the popup and does nothing further
        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LanguagesPopup.IsOpen = false;
        }

        // Removes the group
        void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            LanguagesPopup.IsOpen = false;
            RemoveGroup();
        }

        // Tracks focus of input boxes to show tip text
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            ShowTipText( e.OriginalSource as Control );

            base.OnGotKeyboardFocus( e );
        }

        // Opens the help link
        void OpenLinkInBrowser(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start( e.Uri.AbsoluteUri );
        }


        // Methods


        // Sets the available languages for the popup for the given group
        void SetLanguagesPicker(OptionsGroup group)
        {
            var availableLanguages =
                Rewrap.Core.languages
                    .Where
                        ( lang =>
                            !Model.OptionsGroups
                                .Where( g => g != group )
                                .Any( g => g.Languages.Contains( lang ) )
                        );

            var languagesWithSelectedState =
                availableLanguages
                    .Select
                        ( lang =>
                            new LanguageSelection() {
                                Name = lang,
                                Selected = group != null && group.Languages.Contains( lang )
                            }
                        )
                    .ToList();

            Model.LanguagesPicker = new LanguagesPicker( group, languagesWithSelectedState );
        }

        // Adds or modifies a group from the languages selected in the popup
        void AddOrModifyGroup()
        {
            var group = Model.LanguagesPicker.Group;
            var selectedLanguages =
                Model.LanguagesPicker.Languages
                    .Where( l => l.Selected )
                    .Select( l => l.Name )
                    .ToList();


            // No languages: Remove the group, or if this was a new group,
            // nothing happens.
            if ( selectedLanguages.Count == 0 )
            {
                Model.OptionsGroups.Remove( group );
            }
            // New group: add it
            else if( group == null )
            {
                Model.OptionsGroups.Add( new OptionsGroup( selectedLanguages ) );
            }
            // Else just modify languages
            else
            {
                group.SetLanguages( selectedLanguages );
            }

            Model.LanguagesPicker = null;
        }

        // Removes a group
        void RemoveGroup()
        {
            Model.OptionsGroups.Remove( Model.LanguagesPicker.Group );

            Model.LanguagesPicker = null;
        }

        // Shows the tip text for the given control
        private void ShowTipText(Control inputBox)
        {
            if (inputBox == null) return;

            TipBox.Text = null;

            string tip = null;
            string findLabel(FrameworkElement control)
            {
                tip = tip ?? (string)control.ToolTip;
                switch(control.Parent)
                {
                    case UniformGrid ug:
                        int index = ug.Children.IndexOf( control ) - 1;
                        if (index >=0 && ug.Children[index] is Label l )
                        {
                            return l.Content as String;
                        }
                        else return null;

                    case FrameworkElement fe:
                        return findLabel( fe );

                    default:
                        return null;
                }
            }

            string label = findLabel( inputBox );
            if(label != null && tip != null)
            {
                TipBox.Text = tip;
                TipBox.Inlines.InsertBefore
                    ( TipBox.Inlines.FirstInline
                    , new Bold( new Run( label + " " ) )
                    );
            }
        }
    }

    public class LanguagesHeadingConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var languages = (IEnumerable<string>)value;
            return String.Join( ", ", languages ); // + " ⯆";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class ValueConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch(value)
            {
                case null:
                    return "";
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ( value != null )
            {
                if ( bool.TryParse( value.ToString(), out var boolResult ) )
                {
                    return boolResult;
                }
                else if ( int.TryParse( value.ToString(), out var intResult ) )
                {
                    return intResult;
                }
            }
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
