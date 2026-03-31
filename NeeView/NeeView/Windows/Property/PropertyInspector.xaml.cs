using NeeView.Setting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// Inspector.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyInspector : UserControl
    {
        public PropertyDocument Document
        {
            get { return (PropertyDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(PropertyDocument), typeof(PropertyInspector), new PropertyMetadata(null));


        public bool IsHsvMode
        {
            get { return (bool)GetValue(IsHsvModeProperty); }
            set { SetValue(IsHsvModeProperty, value); }
        }

        public static readonly DependencyProperty IsHsvModeProperty =
            DependencyProperty.Register("IsHsvMode", typeof(bool), typeof(PropertyInspector), new PropertyMetadata(false));



        public bool IsResetButtonVisible
        {
            get { return (bool)GetValue(IsResetButtonVisibleProperty); }
            set { SetValue(IsResetButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsResetButtonVisibleProperty =
            DependencyProperty.Register("IsResetButtonVisible", typeof(bool), typeof(PropertyInspector), new PropertyMetadata(true, IsResetButtonVisibleProperty_Changed));

        private static void IsResetButtonVisibleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyInspector control)
            {
                control.ResetButton.Visibility = control.IsResetButtonVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        public double ColumnRate
        {
            get { return (double)GetValue(ColumnRateProperty); }
            set { SetValue(ColumnRateProperty, value); }
        }

        public static readonly DependencyProperty ColumnRateProperty =
            DependencyProperty.Register("ColumnRate", typeof(double), typeof(PropertyInspector), new PropertyMetadata(0.75));

        public bool IsSwitchMode
        {
            get { return (bool)GetValue(IsSwitchModeProperty); }
            set { SetValue(IsSwitchModeProperty, value); }
        }

        public static readonly DependencyProperty IsSwitchModeProperty =
            DependencyProperty.Register("IsSwitchMode", typeof(bool), typeof(PropertyInspector), new PropertyMetadata(false));


        public bool IsStretch
        {
            get { return (bool)GetValue(IsStretchProperty); }
            set { SetValue(IsStretchProperty, value); }
        }

        public static readonly DependencyProperty IsStretchProperty =
            DependencyProperty.Register(nameof(IsStretch), typeof(bool), typeof(PropertyInspector), new PropertyMetadata(true));


        public VisibilityPropertyValue? VisibilityValue
        {
            get { return (VisibilityPropertyValue)GetValue(VisibilityValueProperty); }
            set { SetValue(VisibilityValueProperty, value); }
        }

        public static readonly DependencyProperty VisibilityValueProperty =
            DependencyProperty.Register(nameof(VisibilityValue), typeof(VisibilityPropertyValue), typeof(PropertyInspector), new PropertyMetadata(null, VisibilityValueProperty_Changed));

        private static void VisibilityValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyInspector control)
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


        public PropertyInspector()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }


        private void Reset(object? sender, RoutedEventArgs e)
        {
            foreach (var item in Document.Elements.OfType<PropertyMemberElement>())
            {
                item.ResetValue();
            }

            this.properties.Items.Refresh();
        }

        public void Refresh()
        {
            this.properties.Items.Refresh();
        }
    }
}
