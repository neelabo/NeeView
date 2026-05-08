using NeeView.Properties;
using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHidePanelCommand : CommandElement
    {
        public ToggleHidePanelCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            var binding = new MultiBinding() { Converter = new MultiBooleanOrConverter() };
            binding.Bindings.Add(new Binding(nameof(PanelsConfig.IsHideLeftPanel)) { Source = Config.Current.Panels });
            binding.Bindings.Add(new Binding(nameof(PanelsConfig.IsHideRightPanel)) { Source = Config.Current.Panels });
            return binding;
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, IsHidePanel());
            return GetStateExecuteMessage(state);
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var isHidePanel = IsHidePanel();
            Config.Current.Panels.IsHideLeftPanel = !isHidePanel;
            Config.Current.Panels.IsHideRightPanel = !isHidePanel;
        }

        private bool IsHidePanel()
        {
            return Config.Current.Panels.IsHideLeftPanel || Config.Current.Panels.IsHideRightPanel;
        }
    }
}
