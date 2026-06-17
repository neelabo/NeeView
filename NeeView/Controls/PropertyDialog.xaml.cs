using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// QuickAccessPropertyDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyDialog : Window
    {
        public PropertyDialog()
        {
            InitializeComponent();
        }

        public PropertyDocument Document
        {
            get { return (PropertyDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register(nameof(Document), typeof(PropertyDocument), typeof(PropertyDialog), new PropertyMetadata(null));


        public double ColumnRate
        {
            get { return (double)GetValue(ColumnRateProperty); }
            set { SetValue(ColumnRateProperty, value); }
        }

        public static readonly DependencyProperty ColumnRateProperty =
            DependencyProperty.Register(nameof(ColumnRate), typeof(double), typeof(PropertyDialog), new PropertyMetadata(0.75));


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }



}
