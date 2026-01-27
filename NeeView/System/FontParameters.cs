using NeeLaboratory.ComponentModel;
using System;
using System.Windows.Media;

namespace NeeView
{
    public class FontParameters : BindableBase
    {
        static FontParameters() => Current = new FontParameters();
        public static FontParameters Current { get; }


        private double _systemFontSize = 12.0;
        private string _defaultFontName;
        private double _defaultFontSize;
        private double _menuFontSize;
        private double _folderTreeFontSize;
        private double _panelFontSize;
        private double _fontIconSize;
        private ClearTypeHint _clearTypeHint = ClearTypeHint.Enabled;


        private FontParameters()
        {
            Config.Current.Fonts.PropertyChanged +=
                (s, e) => UpdateFonts();

            SystemVisualParameters.Current.PropertyChanged += Current_PropertyChanged;

            _defaultFontName = Config.Current.Fonts.FontName ?? "";
            InitializeDefaultFont();

            UpdateFonts();
        }

        private void Current_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SystemVisualParameters.MessageFontName):
                case nameof(SystemVisualParameters.MessageFontSize):
                case nameof(SystemVisualParameters.MenuFontSize):
                    UpdateFonts();
                    break;
            }
        }


        public string DefaultFontName
        {
            get { return _defaultFontName; }
            set
            {
                if (SetProperty(ref _defaultFontName, value))
                {
                    InitializeDefaultFont();
                }
            }
        }


        public double SystemFontSize
        {
            get { return _systemFontSize; }
            set
            {
                if (SetProperty(ref _systemFontSize, value))
                {
                    var limitSize = Math.Max(_systemFontSize, 24.0);
                    App.Current.Resources["SystemFontSize"] = _systemFontSize;
                    App.Current.Resources["SystemFontSizeNormal"] = Math.Min(_systemFontSize * 1.25, limitSize);
                    App.Current.Resources["SystemFontSizeLarge"] = Math.Min(_systemFontSize * 1.5, limitSize);
                    App.Current.Resources["SystemFontSizeHuge"] = Math.Min(_systemFontSize * 2.0, limitSize);
                }
            }
        }

        public double DefaultFontSize
        {
            get { return _defaultFontSize; }
            set
            {
                if (SetProperty(ref _defaultFontSize, value))
                {
                    App.Current.Resources["DefaultFontSize"] = _defaultFontSize;
                }
            }
        }

        public double MenuFontSize
        {
            get { return _menuFontSize; }
            set
            {
                if (SetProperty(ref _menuFontSize, value))
                {
                    App.Current.Resources["MenuFontSize"] = _menuFontSize;
                }
            }
        }

        public double FolderTreeFontSize
        {
            get { return _folderTreeFontSize; }
            set
            {
                if (SetProperty(ref _folderTreeFontSize, value))
                {
                    App.Current.Resources["FolderTreeFontSize"] = _folderTreeFontSize;
                }
            }
        }

        public double PaneFontSize
        {
            get { return _panelFontSize; }
            set
            {
                if (SetProperty(ref _panelFontSize, value))
                {
                    App.Current.Resources["PanelFontSize"] = _panelFontSize;
                }
            }
        }

        public double FontIconSize
        {
            get { return _fontIconSize; }
            set
            {
                if (SetProperty(ref _fontIconSize, value))
                {
                    App.Current.Resources["FontIconSize"] = _fontIconSize;
                }
            }
        }

        public ClearTypeHint ClearTypeHint
        {
            get { return _clearTypeHint; }
            set
            {
                if (SetProperty(ref _clearTypeHint, value))
                {
                    App.Current.Resources["Window.ClearTypeHint"] = _clearTypeHint;
                }
            }
        }


        private void InitializeDefaultFont()
        {
            App.Current.Resources["DefaultFontFamily"] = new FontFamily(_defaultFontName);

            var arrowFontFamily = new FontFamily();
            arrowFontFamily.FamilyMaps.Add(new FontFamilyMap() { Unicode = "2190-2193", Target = "Calibri" }); // 矢印フォントだけ變更
            if (!string.IsNullOrWhiteSpace(_defaultFontName))
            {
                arrowFontFamily.FamilyMaps.Add(new FontFamilyMap() { Unicode = "0000-10ffff", Target = _defaultFontName });
            }
            App.Current.Resources["ArrowFontFamily"] = arrowFontFamily;
        }

        private void UpdateFonts()
        {
            SystemFontSize = SystemVisualParameters.Current.MessageFontSize;
            DefaultFontSize = SystemFontSize * Config.Current.Fonts.FontScale;
            PaneFontSize = SystemFontSize * Config.Current.Fonts.PanelFontScale;
            FolderTreeFontSize = SystemFontSize * Config.Current.Fonts.FolderTreeFontScale;
            FontIconSize = Math.Max(DefaultFontSize + 15, 28.0);

            MenuFontSize = SystemVisualParameters.Current.MenuFontSize * Config.Current.Fonts.MenuFontScale;

            DefaultFontName = Config.Current.Fonts.FontName;

            ClearTypeHint = Config.Current.Fonts.IsClearTypeEnabled ? ClearTypeHint.Enabled : ClearTypeHint.Auto;
        }
    }
}
