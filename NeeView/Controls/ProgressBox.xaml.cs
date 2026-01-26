using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ProgressBox.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressBox : UserControl
    {
        public ProgressBox()
        {
            InitializeComponent();

            this.DataContext = new ProgressBoxViewModel();
        }


        public bool HideMenu
        {
            get { return (bool)GetValue(HideMenuProperty); }
            set { SetValue(HideMenuProperty, value); }
        }

        public static readonly DependencyProperty HideMenuProperty =
            DependencyProperty.Register(nameof(HideMenu), typeof(bool), typeof(ProgressBox), new PropertyMetadata(false));



        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4 && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                // allow Alt+F4
            }
            else
            {
                // ignore all keys
                e.Handled = true;
            }
        }
    }



    public class ProgressBoxViewModel : BindableBase
    {
        private ProgressBoxModel _model;

        public ProgressBoxViewModel()
        {
            _model = new ProgressBoxModel();
            _model.SubscribePropertyChanged(nameof(ProgressBoxModel.IsEnabled), (s, e) => RaisePropertyChanged(nameof(IsEnabled)));
            _model.SubscribePropertyChanged(nameof(ProgressBoxModel.Message), (s, e) => RaisePropertyChanged(nameof(Message)));
        }

        public bool IsEnabled => _model.IsEnabled;

        public string Message => _model.Message;
    }



    public class ProgressBoxModel : BindableBase
    {
        private readonly Progress<string> _progress;

        public ProgressBoxModel()
        {
            _progress = new Progress<string>(Progress_Report);
            ProcessJobEngine.Current.Progress = _progress;
            ProcessJobEngine.Current.SubscribePropertyChanged(nameof(ProcessJobEngine.IsBusy), (s, e) => RaisePropertyChanged(nameof(IsEnabled)));
        }

        public bool IsEnabled => ProcessJobEngine.Current.IsBusy;

        public string Message { get; private set; } = "";

        private void Progress_Report(string obj)
        {
            Message = obj;
            RaisePropertyChanged(nameof(Message));
        }
    }
}
