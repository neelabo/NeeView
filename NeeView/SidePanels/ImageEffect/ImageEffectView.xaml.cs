using NeeView.Effects;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public partial class ImageEffectView : UserControl
    {
        private readonly ImageEffectViewModel _vm;
        private bool _isFocusRequest;


        public ImageEffectView(ImageEffect model)
        {
            InitializeComponent();

            _vm = new ImageEffectViewModel(model);
            this.DataContext = _vm;

            this.IsVisibleChanged += ImageEffectView_IsVisibleChanged;

            Debug.WriteLine($"> Create: {nameof(ImageEffectView)}");
        }


        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AddFilter(KeyExGestureFilter.All);
        }

        private void ImageEffectView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isFocusRequest && this.IsVisible)
            {
                this.Focus();
                _isFocusRequest = false;
            }
        }

        // フィルターパラメータリセット
        private void Reset(object sender, RoutedEventArgs e)
        {
            _vm.ResetValue();

            this.inspectorF.Refresh();
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
}
