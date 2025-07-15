using NeeLaboratory.ComponentModel;
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

namespace NeeView.Setting
{
    /// <summary>
    /// BackgroundSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class BackgroundSettingControl : UserControl, IValueInitializable
    {
        private readonly BrushSource? _source;

        public BackgroundSettingControl()
        {
            InitializeComponent();
        }

        public BackgroundSettingControl(BrushSource source)
        {
            InitializeComponent();

            _source = source;
            this.DataContext = _source;
        }

        public void InitializeValue()
        {
            _source?.Reset();
        }
    }
}
