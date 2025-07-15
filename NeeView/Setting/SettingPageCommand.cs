using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingPage: Command
    /// </summary>
    class SettingPageCommand : SettingPage
    {
        public SettingPageCommand() : base(TextResources.GetString("SettingPage.Command"))
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageCommandList(),
                new SettingPageContextMenu(),
                new SettingPageScript(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Command.GeneralAdvance"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsAccessKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMove))));
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMoveWheel)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Command, nameof(CommandConfig.IsReversePageMove)),
            });
            section.Children.Add(new SettingItemSubProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsReversePageMoveHorizontalWheel)))
            {
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Command, nameof(CommandConfig.IsReversePageMove)),
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Command, nameof(CommandConfig.IsHorizontalWheelLimitedOnce))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// SettingPage: Script
    /// </summary>
    class SettingPageScript : SettingPage
    {
        public SettingPageScript() : base(TextResources.GetString("SettingPage.Script"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Script"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.IsScriptFolderEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.ScriptFolder), new PropertyMemberElementOptions() { EmptyValue = SaveDataProfile.DefaultScriptsFolder }))
            {
                IsStretch = true,
                SubContent = UIElementTools.CreateHyperlink(TextResources.GetString("SettingPage.Script.OpenScriptFolder"), new RelayCommand(ScriptManager.Current.OpenScriptsFolder)),
            });

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.ErrorLevel))) { SubContent = CreateScriptErrorLevelRemarks() });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.IsSQLiteEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Script, nameof(ScriptConfig.OnBookLoadedWhenRenamed))));

            this.Items.Add(section);
        }


        private static TextBlock CreateScriptErrorLevelRemarks()
        {
            var binding = new Binding(nameof(ScriptConfig.ErrorLevel))
            {
                Source = Config.Current.Script,
                Converter = new ScriptErrorLevelToRemarksConverter(),
            };
            var textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.SetBinding(TextBlock.TextProperty, binding);
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, "Control.GrayText");
            return textBlock;
        }

        private class ScriptErrorLevelToRemarksConverter : IValueConverter
        {
            public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is ScriptErrorLevel errorLevel)
                {
                    return errorLevel.GetRemarks();
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    }

    /// <summary>
    /// SettingPage: CommandList
    /// </summary>
    class SettingPageCommandList : SettingPage
    {
        public SettingPageCommandList() : base(TextResources.GetString("SettingPage.Command.Main"))
        {
            var linkCommand = new RelayCommand(() => this.IsSelected = true);

            this.IsScrollEnabled = false;
            this.IsResetButtonConfirm = false;
            this.ResetButtonMargin = new Thickness(0, 0, 10, 15);

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Command.Main"));
            section.Children.Add(new SettingItemCommand() { SearchResultItem = new SettingItemLink(TextResources.GetString("SettingPage.Command.Main"), linkCommand) { IsContentOnly = true } });
            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// SettingPage: ContextMenu
    /// </summary>
    class SettingPageContextMenu : SettingPage
    {
        public SettingPageContextMenu() : base(TextResources.GetString("SettingPage.ContextMenu"))
        {
            var linkCommand = new RelayCommand(() => this.IsSelected = true);

            this.IsScrollEnabled = false;
            this.ResetButtonMargin = new Thickness(0, 0, 10, 15);

            var section = new SettingItemSection(TextResources.GetString("SettingPage.ContextMenu.Edit"));
            section.Children.Add(new SettingItemContextMenu() { SearchResultItem = new SettingItemLink(TextResources.GetString("SettingPage.ContextMenu.Edit"), linkCommand) { IsContentOnly = true } });
            this.Items = new List<SettingItem>() { section };
        }
    }
}
