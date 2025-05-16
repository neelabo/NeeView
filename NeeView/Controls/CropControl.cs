using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// Viewbox を指定して表示領域を制限したコントロールを表示する
    /// </summary>
    public class CropControl : Control
    {
        static CropControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CropControl), new FrameworkPropertyMetadata(typeof(CropControl)));
        }


        private Viewbox? _viewbox;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _viewbox = this.GetTemplateChild("PART_ViewBox") as Viewbox ?? throw new InvalidOperationException();

            this.SizeChanged += CropControl_SizeChanged;
            this.IsTabStop = false;
            this.Focusable = false;

            Update();
        }


        public FrameworkElement? Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(CropControl), new PropertyMetadata(null, TargetProperty_Changed));

        private static void TargetProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CropControl control)
            {
                control.Update();
            }
        }


        public Rect Viewbox
        {
            get { return (Rect)GetValue(ViewboxProperty); }
            set { SetValue(ViewboxProperty, value); }
        }

        public static readonly DependencyProperty ViewboxProperty =
            DependencyProperty.Register(nameof(Viewbox), typeof(Rect), typeof(CropControl), new PropertyMetadata(new Rect(0, 0, 1, 1), ViewboxProperty_Changed));

        private static void ViewboxProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CropControl control)
            {
                control.Update();
            }
        }


        private void CropControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_viewbox is null) return;

            var width = this.ActualWidth;
            var height = this.ActualHeight;

            var imageWidth = width / Viewbox.Width;
            var imageHeight = height / Viewbox.Height;
            var imageLeft = -Viewbox.Left * imageWidth;
            var imageTop = -Viewbox.Top * imageHeight;

            var element = _viewbox;
            element.Width = imageWidth;
            element.Height = imageHeight;
            Canvas.SetLeft(element, imageLeft);
            Canvas.SetTop(element, imageTop);
        }
    }
}
