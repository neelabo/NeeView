using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView.Setting
{
    /// <summary>
    /// InputTouchSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class InputTouchSettingControl : UserControl
    {
        private InputTouchSettingViewModel? _vm;

        public InputTouchSettingControl()
        {
            InitializeComponent();
        }

        public void Initialize(IReadOnlyDictionary<string, CommandElement> commandMap, string key)
        {
            InitializeComponent();

            this.GestureBox.PreviewMouseLeftButtonUp += GestureBox_PreviewMouseLeftButtonUp;

            _vm = new InputTouchSettingViewModel(commandMap, key, this.GestureBox);
            DataContext = _vm;
        }

        public void Flush()
        {
            _vm?.Flush();
        }

        private void GestureBox_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            var pos = e.GetPosition(this.GestureBox);

            _vm.SetTouchGesture(pos, this.GestureBox.ActualWidth, this.GestureBox.ActualHeight);
        }
    }



    /// <summary>
    /// タッチエリアを背景色に変換
    /// </summary>
    public class TouchAreaToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var map = (TouchAreaMap)value;
            var area = (TouchArea)parameter;

            return map[area] ? App.Current.Resources["Control.Accent"] : App.Current.Resources["Window.Background"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
