using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public partial class ImageEffectView : UserControl
    {
        private readonly ImageEffectViewModel _vm;
        private bool _isFocusRequest;


        public ImageEffectView()
        {
            InitializeComponent();

            _vm = new ImageEffectViewModel();
            _vm.RenameProfileRequested += ViewModel_RenameRequest;
            _vm.DeleteConfirmRequested += ViewModel_DeleteConfirmRequested;

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

        private async void ViewModel_RenameRequest(object? sender, EventArgs e)
        {
            var comboBox = this.EffectProfileComboBox;
            comboBox.UpdateLayout();

            var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(comboBox, "FileNameTextBlock");
            if (textBlock is null) return;

            var rename = new EffectProfileRenameControl(new RenameControlSource(comboBox, textBlock), Rename);
            await rename.ShowAsync();

            bool Rename(string name)
            {
                return _vm.RenameProfile(name);
            }
        }

        private void ViewModel_DeleteConfirmRequested(object? sender, DeleteConfirmEventArgs e)
        {
            var dialog = new MessageDialog(e.Caption, e.Message);
            dialog.Owner = Window.GetWindow(this);
            dialog.Commands.Add(UICommands.Delete);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog();
            e.DialogResult = result.IsPossible;
        }
    }


    public class EffectProfileRenameControl : RenameControl
    {
        private readonly Func<string, bool> _renameFunc;

        public EffectProfileRenameControl(RenameControlSource source, Func<string, bool> renameFunc) : base(source)
        {
            _renameFunc = renameFunc;
            this.IsInvalidSeparatorChars = true;
            this.IsInvalidFileNameChars = true;
        }

        protected override async Task<bool> OnRenameAsync(string oldValue, string newValue)
        {
            if (oldValue == newValue) return true;

            var result = _renameFunc(newValue);
            return result;
        }
    }

}
