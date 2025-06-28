using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView.Setting
{
    public class ObjectCompareConverter : IValueConverter
    {
        public object? Target { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return object.Equals(value, Target);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectCompareToVisibilityConverter : IValueConverter
    {
        public object? Target { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return object.Equals(value, Target) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Setting: Window
    /// </summary>
    public class SettingPageWindow : SettingPage
    {
        public SettingPageWindow() : base(Properties.TextResources.GetString("SettingPage.Window"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageFonts(),
                new SettingPageWindowTitle(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Window.Theme"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(ThemeManager.Current, nameof(ThemeManager.SelectedItem), new PropertyMemberElementOptions()
            {
                GetStringMapFunc = ThemeManager.CreateItemsMap
            })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.CustomThemeFolder)))
            {
                IsStretch = true,
                SubContent = UIElementTools.CreateHyperlink(Properties.TextResources.GetString("SettingPage.Window.Theme.OpenCustomThemeFolder"), OpenCustomThemeFolder),
            });
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Window.Background"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.CustomBackground)),
                new BackgroundSettingControl(Config.Current.Background.CustomBackground)));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Window.Advance"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsHamburgerMenu))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.IsCaptionEmulateInFullScreen))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Window, nameof(WindowConfig.MouseActivateAndEat))));
            this.Items.Add(section);
        }

        #region Commands
        private RelayCommand? _openCustomThemeFolder;
        public RelayCommand OpenCustomThemeFolder
        {
            get { return _openCustomThemeFolder = _openCustomThemeFolder ?? new RelayCommand(OpenCustomThemeFolder_Execute); }
        }

        private void OpenCustomThemeFolder_Execute()
        {
            ThemeManager.OpenCustomThemeFolder();
        }
        #endregion

    }



    /// <summary>
    /// SettingPage: Fonts
    /// </summary>
    public class SettingPageFonts : SettingPage
    {
        public SettingPageFonts() : base(Properties.TextResources.GetString("SettingPage.Fonts"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.Fonts"));
            section.Children.Add(new SettingItemPropertyFont(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.FontName))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.FontScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.MenuFontScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.FolderTreeFontScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.PanelFontScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Fonts, nameof(FontsConfig.IsClearTypeEnabled))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// Setting: WindowTitle
    /// </summary>
    public class SettingPageWindowTitle : SettingPage
    {
        public SettingPageWindowTitle() : base(Properties.TextResources.GetString("SettingPage.WindowTitle"))
        {
            var section = new SettingItemSection(Properties.TextResources.GetString("SettingPage.WindowTitle"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormat1))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormat2))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormatMedia))) { IsStretch = true });
            section.Children.Add(new SettingItemNote(Properties.TextResources.GetString("SettingPage.WindowTitle.Note"), Properties.TextResources.GetString("SettingPage.WindowTitle.Note.Title")));

            this.Items = new List<SettingItem>() { section };
        }
    }

}
