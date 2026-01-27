using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ConsoleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        public ConsoleWindow()
        {
            InitializeComponent();

            this.Closed += ConsoleWindow_Closed;

            this.Console.ConsoleHost = new ConsoleHost(this);
        }

        private void ConsoleWindow_Closed(object? sender, System.EventArgs e)
        {
            this.Console.Dispose();
        }
    }
}
