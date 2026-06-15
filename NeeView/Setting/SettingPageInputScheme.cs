using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView.Setting
{
    public class SettingPageInputScheme : SettingPage
    {
        private readonly CommandResetControl _commandResetControl;
        private readonly SettingItemGroup _group;

        public SettingPageInputScheme(string name) : base(name)
        {
            this.IsResetButtonEnabled = false;

            _commandResetControl = new CommandResetControl(Config.Current.Command.PresetInputScheme);
            PageReadOrder = Config.Current.Command.PresetPageReadOrder;

            var pageReadOrder = new Dictionary<Enum, string>
            {
                [PageReadOrder.LeftToRight] = "▶ " + PageReadOrder.LeftToRight.ToAliasName(),
                [PageReadOrder.RightToLeft] = "◀ " + PageReadOrder.RightToLeft.ToAliasName()
            };
            Debug.Assert(Enum.GetNames(typeof(PageReadOrder)).Length == pageReadOrder.Count);

            this.Items = new List<SettingItem>();
            _group = new SettingItemGroup();
            _group.Children.Add(new SettingItemContent(TextResources.GetString("CommandResetWindow.ResetType.Title"), _commandResetControl) { IsStretch = true });
            _group.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(this, nameof(PageReadOrder), new PropertyMemberElementOptions()
            {
                EnumMap = pageReadOrder,
                Name = TextResources.GetString("CommandConfig.PresetPageReadOrder"),
                Tips = TextResources.GetString("CommandConfig.PresetPageReadOrder.Remarks")
            })));
            this.Items.Add(_group);
        }

        public InputScheme InputScheme => _commandResetControl.InputScheme;

        [PropertyMember]
        public PageReadOrder PageReadOrder { get; set; }

        public void AddSettingItem(SettingItem item)
        {
            _group.Children.Add(item);
        }
    }
}
