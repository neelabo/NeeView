using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// DragActionGestureControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DragActionGestureControl : UserControl
    {
        private DragActionGestureControlViewModel? _vm;


        public DragActionGestureControl()
        {
            InitializeComponent();
        }


        internal void Initialize(DragActionCollection memento, string key)
        {
            _vm = new DragActionGestureControlViewModel(memento, key, this.GestureBox);
            DataContext = _vm;
        }

        internal void Decide()
        {
            _vm?.Decide();
        }
    }
}
