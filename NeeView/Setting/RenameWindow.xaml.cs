using NeeLaboratory.Generators;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NeeView.Setting
{
    public class RenameWindowParam
    {
        public RenameWindowParam(string text, string defaultText)
        {
            Text = text;
            DefaultText = defaultText;
        }

        public string Text { get; set; }
        public string DefaultText { get; set; }
    }


    /// <summary>
    /// RenameWindow.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class RenameWindow : Window, INotifyPropertyChanged
    {
        private readonly RenameWindowParam _param;
        private string _text;


        public RenameWindow(RenameWindowParam param)
        {
            _param = param;
            _text = _param.Text;

            InitializeComponent();
            this.DataContext = this;

            this.Loaded += RenameWindow_Loaded;
            this.KeyDown += RenameWindow_KeyDown;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public string Text
        {
            get { return _text; }
            set { _text = value; RaisePropertyChanged(); }
        }


        private void RenameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.NameTextBox.SelectAll();
            this.NameTextBox.Focus();
        }

        private void RenameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Text = _param.DefaultText;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _param.Text = Text;
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
