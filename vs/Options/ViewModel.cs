using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VS.Options
{
    public class ViewModel: Base
    {
        public GlobalOptionsGroup GlobalOptions
        {
            get { return _GlobalOptions; }
            set { SetField( ref _GlobalOptions, value ); }
        }
        private GlobalOptionsGroup _GlobalOptions;

        public ObservableCollection<OptionsGroup> OptionsGroups
        {
            get { return _OptionsGroups; }
            set { SetField( ref _OptionsGroups, value ); }
        }
        private ObservableCollection<OptionsGroup> _OptionsGroups;

        public LanguagesPicker LanguagesPicker
        {
            get { return _LanguagesPicker; }
            set { SetField( ref _LanguagesPicker, value ); }
        }
        private LanguagesPicker _LanguagesPicker;
    }

    public class LanguagesPicker: Base
    {
        public LanguagesPicker(OptionsGroup group, List<LanguageSelection> languages)
        {
            Group = group;
            Languages = languages;
        }

        public OptionsGroup Group { get; private set; }

        // Needs to be an old-style tuple for wpf databinding
        public List<LanguageSelection> Languages { get; private set; }
    }

    public class LanguageSelection
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    public class OptionsGroup: Base
    {
        public OptionsGroup(IEnumerable<String> languages)
        {
            SetLanguages( languages );
        }

        public ReadOnlyCollection<String> Languages
        {
            get { return _Languages; }
            private set { SetField( ref _Languages, value ); }
        }
        private ReadOnlyCollection<String> _Languages;

        public void SetLanguages(IEnumerable<String> languages)
        {
            Languages = new ReadOnlyCollection<string>( languages.ToList() );
        }

        public int? WrappingColumn { get; set; }

        public bool? WholeComment { get; set; }

        public bool? DoubleSentenceSpacing { get; set; }

        public bool? Reformat { get; set; }
    }

    public class GlobalOptionsGroup
    {
        public int? WrappingColumn { get; set; } = 80;

        public bool WholeComment { get; set; } = true;

        public bool DoubleSentenceSpacing { get; set; } = false;

        public bool Reformat { get; set; } = false;
    }

    public class Base : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) ) return false;

            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }
    }

    /// <summary>
    /// Design data
    /// </summary>
    public class DesignViewModel: ViewModel
    {
        public DesignViewModel(): base()
        {
            GlobalOptions = new GlobalOptionsGroup();

            OptionsGroups = new ObservableCollection<OptionsGroup>()
            {
                new OptionsGroup(new String[] { "C#", "JavaScript", "C#", "JavaScript", "C#", "JavaScript", "C#", "JavaScript", "C#", "JavaScript", "C#", "JavaScript", } ) {
                    WrappingColumn = null,
                    WholeComment = null,
                    DoubleSentenceSpacing = true,
                    Reformat = false,
                },
            };

            LanguagesPicker =
                new LanguagesPicker(
                    null,
                    new List<LanguageSelection>() {
                        new LanguageSelection() { Name = "AutoHotkey", Selected = false },
                        new LanguageSelection() { Name = "Basic", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = true },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                        new LanguageSelection() { Name = "C/C++", Selected = false },
                    }
                );
        }
    }
}
