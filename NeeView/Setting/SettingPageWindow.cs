using NeeLaboratory.Windows.Input;
using NeeView.Properties;
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
        public SettingPageWindow() : base(TextResources.GetString("SettingPage.Window"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageFonts(),
                new SettingPageWindowTitle(),
                new SettingPageMenuBar(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Window.Theme"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.ThemeString), new PropertyMemberElementOptions()
            {
                GetStringMapFunc = ThemeManager.CreateItemsMap
            })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Theme, nameof(ThemeConfig.CustomThemeFolder)))
            {
                IsStretch = true,
                SubContent = UIElementTools.CreateHyperlink(TextResources.GetString("SettingPage.Window.Theme.OpenCustomThemeFolder"), OpenCustomThemeFolder),
            });
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Window.Background"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.CustomBackground)),
                new BackgroundSettingControl(Config.Current.Background.CustomBackground)));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Window.Advance"));
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
        public SettingPageFonts() : base(TextResources.GetString("SettingPage.Fonts"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Fonts"));
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
        public SettingPageWindowTitle() : base(TextResources.GetString("SettingPage.WindowTitle"))
        {
            var section = new SettingItemSection(TextResources.GetString("SettingPage.WindowTitle"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormat1))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormat2))) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.WindowTitle, nameof(WindowTitleConfig.WindowTitleFormatMedia))) { IsStretch = true });
            section.Children.Add(new SettingItemNote(TextResources.GetString("SettingPage.WindowTitle.Note"), TextResources.GetString("SettingPage.WindowTitle.Note.Title")));

            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// Setting: MenuBar
    /// </summary>
    public class SettingPageMenuBar : SettingPage
    {
        public SettingPageMenuBar() : base(TextResources.GetString("SettingPage.MenuBar"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.MenuBar"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsHamburgerMenu))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.AddressBar"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsAddressBarEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsSettingsButtonEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.MenuBar, nameof(MenuBarConfig.IsBookmarkDialogEnabled))));
            this.Items.Add(section);
        }
    }

}
