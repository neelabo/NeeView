using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// RenameControl.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class RenameControl : UserControl, INotifyPropertyChanged
    {
        private readonly RenameManager _manager;
        private readonly RenameContext _context;
        private bool _closed;


        public RenameControl(RenameControlSource source)
        {
            InitializeComponent();

            _manager = RenameManager.GetRenameManager(source.Target ?? source.TargetContainer)
                ?? throw new InvalidOperationException("RenameManager must not be null.");

            this.Target = source.Target;
            this.StoredFocusTarget = source.TargetContainer;
            if (this.Target is not null)
            {
                this.RenameTextBox.FontFamily = this.Target.FontFamily;
                this.RenameTextBox.FontSize = this.Target.FontSize;
            }

            _context = new RenameContext(source.Text);

            this.RenameTextBox.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 終了時イベント
        /// </summary>
        public event EventHandler<RenameClosedEventArgs>? Closed;


        // リネームを行うTextBlock
        public TextBlock? Target { get; private set; }

        public RenameContext Context => _context;

        public bool IsInvalidFileNameChars
        {
            get => _context.IsInvalidFileNameChars;
            set => _context.IsInvalidFileNameChars = value;
        }

        public bool IsInvalidSeparatorChars
        {
            get => _context.IsInvalidSeparatorChars;
            set => _context.IsInvalidSeparatorChars = value;
        }

        public bool IsSelectFileNameBody
        {
            get => _context.IsSelectFileNameBody;
            set => _context.IsSelectFileNameBody = value;
        }

        public bool IsHideExtension
        {
            get => _context.IsHideExtension;
            set => _context.IsHideExtension = value;
        }

        // フォーカスを戻すコントロール
        public UIElement StoredFocusTarget { get; set; }


        public async ValueTask<RenameControlResult> ShowAsync()
        {
            var tcs = new TaskCompletionSource<RenameControlResult>();
            Closed += RenameControl_Closed;
            _manager.Add(this);
            var result = await tcs.Task;
            Closed -= RenameControl_Closed;
            return result;

            void RenameControl_Closed(object? sender, RenameClosedEventArgs e)
            {
                tcs.TrySetResult(new RenameControlResult(e.OldValue, e.NewValue, e.MoveRename, e.IsRestoreFocus));
            }
        }

        public static async ValueTask<RenameControlResult> ShowAsync(RenameControlSource source)
        {
            var renameControl = new RenameControl(source);
            return await renameControl.ShowAsync();
        }

        /// <summary>
        /// Rename開始
        /// </summary>
        public void Open()
        {
            _manager.Add(this);
        }

        /// <summary>
        /// Rename終了
        /// </summary>
        /// <param name="isSuccess">名前変更成功</param>
        /// <param name="isRestoreFocus">元のコントロールにフォーカスを戻す要求</param>
        /// <param name="moveRename">次の項目に名前変更を要求</param>
        public async ValueTask CloseAsync(bool isSuccess, bool isRestoreFocus = true, int moveRename = 0)
        {
            Debug.Assert(-1 <= moveRename && moveRename <= 1);

            if (_closed) return;
            _closed = true;
            this.IsHitTestVisible = false;

            var oldValue = _context.OldText;
            var newValue = isSuccess ? _context.NewText : _context.OldText;
            var restoreFocus = isRestoreFocus && this.RenameTextBox.IsFocused;

            if (oldValue != newValue)
            {
                await OnRenameAsync(oldValue, newValue);

                // NOTE: テキスト切り替えを隠すために閉じるのを遅らせる
                await Task.Delay(100);
            }

            _manager.Remove(this);

            if (restoreFocus && StoredFocusTarget != null)
            {
                FocusTools.FocusIfWindowActive(StoredFocusTarget);
            }

            var args = new RenameClosedEventArgs(oldValue, newValue, moveRename, restoreFocus);
            Closed?.Invoke(this, args);
        }

        protected virtual async ValueTask<bool> OnRenameAsync(string oldValue, string newValue)
        {
            return await Task.FromResult(true);
        }

        private async void RenameTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            await CloseAsync(true);
        }

        private void RenameTextBox_Loaded(object? sender, RoutedEventArgs e)
        {
            // 拡張子以外を選択状態にする
            string name = _context.IsSelectFileNameBody ? LoosePath.GetFileNameWithoutExtension(_context.Text) : _context.Text;
            this.RenameTextBox.Select(0, name.Length);

            // 表示とともにフォーカスする
            this.RenameTextBox.Focus();
        }

        private void RenameTextBox_Unloaded(object? sender, RoutedEventArgs e)
        {
        }

        private void RenameTextBox_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
        }

        private async void RenameTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                await CloseAsync(false);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                await CloseAsync(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                var moveRename = (Keyboard.Modifiers == ModifierKeys.Shift) ? -1 : +1;
                await CloseAsync(true, true, moveRename);
                e.Handled = true;
            }
        }

        private async void RenameTextBox_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            await CloseAsync(true);
            e.Handled = true;
        }

        private void MeasureText_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            this.RenameTextBox.MinWidth = Math.Min(this.MeasureText.ActualWidth + 30, this.MaxWidth);
        }

        // 単キーコマンド無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object? sender, KeyEventArgs e)
        {
            KeyExGesture.AddFilter(KeyExGestureFilter.All);
        }

        /// <summary>
        /// renameコントロールをターゲットの位置に合わせる
        /// </summary>
        public void SyncLayout()
        {
            Point pos;
            if (this.Target is not null)
            {
                pos = this.Target.TranslatePoint(new Point(-3, -2), _manager);
            }
            else
            {
                pos = this.StoredFocusTarget.TranslatePoint(new Point(2, 2), _manager);
            }
            Canvas.SetLeft(this, pos.X);
            Canvas.SetTop(this, pos.Y);

            this.MaxWidth = _manager.ActualWidth - pos.X - 8;
        }

        public void SetTargetVisibility(Visibility visibility)
        {
            if (this.Target is null) return;

            this.Target.Visibility = visibility;
        }
    }
}
