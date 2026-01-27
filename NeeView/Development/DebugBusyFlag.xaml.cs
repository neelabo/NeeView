using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// DebugBusyFlag.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class DebugBusyFlag : UserControl, INotifyPropertyChanged
    {
        public DebugBusyFlag()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(DebugBusyFlag), new PropertyMetadata("Label"));


        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(DebugBusyFlag), new PropertyMetadata(false));

    }

    public class BooleanToCustomBrushConverter : IValueConverter
    {
        public Brush True { get; set; } = Brushes.Red;
        public Brush False { get; set; } = Brushes.Gainsboro;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFlag)
            {
                return isFlag ? True : False;
            }

            return False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
