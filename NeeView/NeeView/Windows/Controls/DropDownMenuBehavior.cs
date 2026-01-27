// from http://tnakamura.hatenablog.com/entry/20101001/dropdownmenu_attached_behavior

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public static class DropDownMenuBehavior
    {
        public static readonly DependencyProperty DropDownMenuProperty = DependencyProperty.RegisterAttached(
            "DropDownMenu",
            typeof(ContextMenu),
            typeof(DropDownMenuBehavior),
            new PropertyMetadata(null, OnDropDownMenuChanged));

        private static void OnDropDownMenuChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ToggleButton button)
            {
                return;
            }

            if (e.NewValue is not ContextMenu dropDownMenu)
            {
                return;
            }

            dropDownMenu.Placement = PlacementMode.Bottom;
            dropDownMenu.PlacementTarget = button;
            dropDownMenu.VerticalOffset = -5;
            dropDownMenu.SetBinding(ContextMenu.IsOpenProperty, new Binding(nameof(button.IsChecked))
            {
                Source = button,
                Delay = 200, // ボタンでも閉じれるようにコンテキストメニュからの更新を遅延
            });
        }

        public static void SetDropDownMenu(ToggleButton button, ContextMenu dropDownMenu)
        {
            button.SetValue(DropDownMenuProperty, dropDownMenu);
        }

        public static ContextMenu? GetDropDownMenu(ToggleButton button)
        {
            return button.GetValue(DropDownMenuProperty) as ContextMenu;
        }
    }
}
