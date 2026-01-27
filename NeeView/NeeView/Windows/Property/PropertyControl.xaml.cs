using System.Windows;
using System.Windows.Controls;
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

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(PropertyControl), new PropertyMetadata(null));


        public string Tips
        {
            get { return (string)GetValue(TipsProperty); }
            set { SetValue(TipsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tips.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TipsProperty =
            DependencyProperty.Register("Tips", typeof(string), typeof(PropertyControl), new PropertyMetadata(null));


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyControl), new PropertyMetadata(null, Value_Changed));

        private static void Value_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

        // Using a DependencyProperty as the backing store for ColumnRate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnRateProperty =
            DependencyProperty.Register("ColumnRate", typeof(double), typeof(PropertyControl), new PropertyMetadata(0.75, ColumnRateProperty_Changed));

        private static void ColumnRateProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyControl control)
            {
                control.Update();
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

            var isStretch = true;
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
