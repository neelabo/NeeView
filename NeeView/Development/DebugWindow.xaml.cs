using System.Windows;

namespace NeeView
{
    /// <summary>
    /// DebugWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        public DebugWindow(MainWindowViewModel vm) : this()
        {
            this.DataContext = vm;
        }
    }

}
