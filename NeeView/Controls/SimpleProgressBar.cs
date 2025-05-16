using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    public class SimpleProgressBar : Control
    {
        static SimpleProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleProgressBar), new FrameworkPropertyMetadata(typeof(SimpleProgressBar)));
        }

        private Grid? _root;
        private Rectangle? _bar;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _root = GetTemplateChild("PART_Root") as Grid ?? throw new InvalidOperationException();
            _bar = GetTemplateChild("PART_Bar") as Rectangle ?? throw new InvalidOperationException();

            _root.SizeChanged += (s, e) => Update();
            Update();
        }


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SimpleProgressBar), new PropertyMetadata(0.0, ValueProperty_Changed));

        private static void ValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SimpleProgressBar control)
            {
                control.Update();
            }
        }


        private void Update()
        {
            if (_root == null || _bar == null) return;

            _bar.Width = _root.ActualWidth * Value;
        }
    }
}
