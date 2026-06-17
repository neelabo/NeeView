using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows.Property
{
    class PropertyValueTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (container is not FrameworkElement element) return null;

            if (item is PropertyValue_Boolean booleanValue)
            {
                if (booleanValue.VisualType == PropertyVisualType.ToggleSwitch)
                {
                    return element.FindResource("PropertyValue_Boolean_ToggleSwitch") as DataTemplate;
                }
            }

            return null;
        }
    }
}
