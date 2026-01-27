using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// DragActionParameterControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DragActionParameterControl : UserControl
    {
        #region DependencyProperties

        public bool IsAny
        {
            get { return (bool)GetValue(IsAnyProperty); }
            private set { SetValue(IsAnyProperty, value); }
        }

        public static readonly DependencyProperty IsAnyProperty =
            DependencyProperty.Register("IsAny", typeof(bool), typeof(DragActionParameterControl), new PropertyMetadata(false));

        #endregion

        private DragActionParameterViewModel? _vm;

        public DragActionParameterControl()
        {
            InitializeComponent();
        }

        public void Initialize(DragActionCollection commandMap, string key)
        {
            InitializeComponent();

            _vm = new DragActionParameterViewModel(commandMap, key);
            this.DataContext = _vm;

            this.IsAny = _vm.PropertyDocument != null;

            if (this.IsAny)
            {
                this.EmptyText.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.MainPanel.Visibility = Visibility.Collapsed;
            }
        }


        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            _vm?.Reset();
            this.Inspector.Refresh(); // TODO: MVVM的に更新されるようにする
        }
    }
}
