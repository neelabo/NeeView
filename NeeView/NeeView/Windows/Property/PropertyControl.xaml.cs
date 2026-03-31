using NeeView.Setting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// PropertyControl.xaml の相互作用ロジック
    /// </summary>
    [ContentProperty("Value")]
    public partial class PropertyControl : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(PropertyControl), new PropertyMetadata(null));


        public string Tips
        {
            get { return (string)GetValue(TipsProperty); }
            set { SetValue(TipsProperty, value); }
        }

        public static readonly DependencyProperty TipsProperty =
            DependencyProperty.Register("Tips", typeof(string), typeof(PropertyControl), new PropertyMetadata(null));


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyControl), new PropertyMetadata(null, ValueProperty_Changed));

        private static void ValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyControl control)
            {
                control.Update();
            }
        }

        public double ColumnRate
        {
            get { return (double)GetValue(ColumnRateProperty); }
            set { SetValue(ColumnRateProperty, value); }
        }

        public static readonly DependencyProperty ColumnRateProperty =
            DependencyProperty.Register("ColumnRate", typeof(double), typeof(PropertyControl), new PropertyMetadata(0.75, ColumnRateProperty_Changed));

        private static void ColumnRateProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyControl control)
            {
                control.Update();
            }
        }


        public bool IsStretch
        {
            get { return (bool)GetValue(IsStretchProperty); }
            set { SetValue(IsStretchProperty, value); }
        }

        public static readonly DependencyProperty IsStretchProperty =
            DependencyProperty.Register(nameof(IsStretch), typeof(bool), typeof(PropertyControl), new PropertyMetadata(true, IsStretchProperty_Changed));

        private static void IsStretchProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyControl control)
            {
                control.Update();
            }
        }


        public VisibilityPropertyValue? VisibilityValue
        {
            get { return (VisibilityPropertyValue)GetValue(VisibilityValueProperty); }
            set { SetValue(VisibilityValueProperty, value); }
        }

        public static readonly DependencyProperty VisibilityValueProperty =
            DependencyProperty.Register(nameof(VisibilityValue), typeof(VisibilityPropertyValue), typeof(PropertyControl), new PropertyMetadata(null, VisibilityValueProperty_Changed));

        private static void VisibilityValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyControl control)
            {
                var value = (VisibilityPropertyValue?)e.NewValue;
                if (value is not null)
                {
                    value.SetBind(control);
                }
                else
                {
                    BindingOperations.ClearBinding(control, VisibilityProperty);
                }
            }
        }



        public PropertyControl()
        {
            InitializeComponent();
        }


        private void Update()
        {
            this.Root.SizeChanged -= Root_SizeChanged;
            if (Value == null) return;

            var isStretch = IsStretch;
            if (Value is PropertyValue_Boolean booleanValue)
            {
                if (booleanValue.VisualType == PropertyVisualType.ToggleSwitch)
                {
                    isStretch = false;
                }
            }

            if (isStretch)
            {
                this.ValueUI.Width = this.Root.ActualWidth * ColumnRate;
                this.Root.SizeChanged += Root_SizeChanged;
            }
            else
            {
                this.ValueUI.Width = double.NaN;
            }
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                this.ValueUI.Width = e.NewSize.Width * ColumnRate;
            }
        }
    }
}
