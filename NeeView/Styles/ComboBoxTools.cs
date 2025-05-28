using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class ComboBoxTools
    {
        public static Brush GetMouseOverBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(MouseOverBackgroundProperty);
        }

        public static void SetMouseOverBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(MouseOverBackgroundProperty, value);
        }

        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.RegisterAttached("MouseOverBackground", typeof(Brush), typeof(ComboBoxTools), new PropertyMetadata(Brushes.Transparent));


        public static Brush GetMouseOverBorderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(MouseOverBorderBrushProperty);
        }

        public static void SetMouseOverBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(MouseOverBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MouseOverBorderBrushProperty =
            DependencyProperty.RegisterAttached("MouseOverBorderBrush", typeof(Brush), typeof(ComboBoxTools), new PropertyMetadata(Brushes.White));


        public static ControlTemplate? GetToggleButtonTemplate(DependencyObject obj)
        {
            return (ControlTemplate)obj.GetValue(ToggleButtonTemplateProperty);
        }

        public static void SetToggleButtonTemplate(DependencyObject obj, Brush value)
        {
            obj.SetValue(ToggleButtonTemplateProperty, value);
        }

        public static readonly DependencyProperty ToggleButtonTemplateProperty =
            DependencyProperty.RegisterAttached("ToggleButtonTemplate", typeof(ControlTemplate), typeof(ComboBoxTools), new PropertyMetadata(null));
    }

}
