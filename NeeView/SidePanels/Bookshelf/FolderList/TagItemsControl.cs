using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public class TagItemsControl : ItemsControl
    {
        public double ItemOpacity
        {
            get { return (double)GetValue(ItemOpacityProperty); }
            set { SetValue(ItemOpacityProperty, value); }
        }

        public static readonly DependencyProperty ItemOpacityProperty =
            DependencyProperty.Register(nameof(ItemOpacity), typeof(double), typeof(TagItemsControl), new PropertyMetadata(1.0));


        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(TagItemsControl), new PropertyMetadata(Orientation.Horizontal));


        public TagItemsControl()
        {
            FrameworkElementFactory stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            
            Binding orientationBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath(TagItemsControl.OrientationProperty)
            };
            stackPanelFactory.SetBinding(StackPanel.OrientationProperty, orientationBinding);

            ItemsPanelTemplate template = new ItemsPanelTemplate(stackPanelFactory);
            this.ItemsPanel = template;
        }

    }

}
