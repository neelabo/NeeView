using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(PropertyDocument), typeof(PropertyInspector), new PropertyMetadata(null));


        public bool IsHsvMode
        {
            get { return (bool)GetValue(IsHsvModeProperty); }
            set { SetValue(IsHsvModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHsvMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHsvModeProperty =
            DependencyProperty.Register("IsHsvMode", typeof(bool), typeof(PropertyInspector), new PropertyMetadata(false));



        public bool IsResetButtonVisible
        {
            get { return (bool)GetValue(IsResetButtonVisibleProperty); }
            set { SetValue(IsResetButtonVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsResetButtonVisible.  This enables animation, styling, binding, etc...
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

        // Using a DependencyProperty as the backing store for ColumnRate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnRateProperty =
            DependencyProperty.Register("ColumnRate", typeof(double), typeof(PropertyInspector), new PropertyMetadata(0.75));

        public bool IsSwitchMode
        {
            get { return (bool)GetValue(IsSwitchModeProperty); }
            set { SetValue(IsSwitchModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSwitchMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSwitchModeProperty =
            DependencyProperty.Register("IsSwitchMode", typeof(bool), typeof(PropertyInspector), new PropertyMetadata(false));




        /// <summary>
        /// 
        /// </summary>
        public PropertyInspector()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }


        //
        private void Reset(object? sender, RoutedEventArgs e)
        {
            foreach (var item in Document.Elements.OfType<PropertyMemberElement>())
            {
                item.ResetValue();
            }

            this.properties.Items.Refresh();
        }

        //
        public void Refresh()
        {
            this.properties.Items.Refresh();
        }
    }
}
