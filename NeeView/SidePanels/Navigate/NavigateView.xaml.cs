using NeeView.Effects;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
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
    public partial class NavigateView : UserControl
    {
        private readonly NavigateViewModel _vm;
        private bool _isFocusRequest;


        public NavigateView(NavigateModel model)
        {
            InitializeComponent();

            _vm = new NavigateViewModel(model);
            this.DataContext = _vm;

            this.MediaControlView.Source = model.MediaControl;

            this.IsVisibleChanged += NavigateView_IsVisibleChanged;

            Debug.WriteLine($"> Create: {nameof(NavigateView)}");
        }


        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object? sender, KeyEventArgs e)
        {
            KeyExGesture.AddFilter(KeyExGestureFilter.All);
        }

        private void NavigateView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isFocusRequest && this.IsVisible)
            {
                this.Focus();
                _isFocusRequest = false;
            }
        }

        private void BaseScale_ValueDelta(object? sender, ValueDeltaEventArgs e)
        {
            _vm.AddBaseScaleTick(e.Delta);
        }

        private void Scale_ValueDelta(object? sender, ValueDeltaEventArgs e)
        {
            _vm.AddScaleTick(e.Delta);
        }

        private void Angle_ValueDelta(object? sender, ValueDeltaEventArgs e)
        {
            _vm.AddAngleTick(e.Delta);
        }


        public void FocusAtOnce()
        {
            var focused = this.Focus();
            if (!focused)
            {
                _isFocusRequest = true;
            }
        }
    }



    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            var gridLength = new GridLength(val);

            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }
    }
}
