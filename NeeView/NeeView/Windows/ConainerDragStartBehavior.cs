#nullable enable
// from https://github.com/takanemu/WPFDragAndDropSample

using Microsoft.Xaml.Behaviors;
using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace NeeView.Windows
{

    /// <summary>
    /// ドラッグ開始処理。
    /// データ作成に時間がかかる場合があるため非同期化した。
    /// </summary>
    public delegate ValueTask DragBeginAsync(object sender, DragStartEventArgs args, CancellationToken token);

    /// <summary>
    /// TreeViewやListBoxに特化した<see cref="DragStartBehavior"/>
    /// </summary>
    public class ContainerDragStartBehavior<TItem> : Behavior<FrameworkElement>
        where TItem : UIElement
    {
        private Point _origin;
        private bool _isButtonDown;
        private TItem? _dragItem;
        private UIElement? _adornerVisual;
        private Point _dragStartPos;
        private DragAdorner? _dragGhost;
        private CancellationTokenSource? _cancellationTokenSource;



        /// <summary>
        /// ドラッグ中アイテム
        /// </summary>
        public TItem? DragItem => _dragItem;

        /// <summary>
        /// ドラッグしたか
        /// </summary>
        public bool Dragged { get; private set; }

        /// <summary>
        /// DoDragDropのフック
        /// </summary>
        /// <remarks>
        /// staticなオブジェクトになることがあるので標準のプロパティにしている
        /// </remarks>
        public IDragDropHook? DragDropHook { get; set; }


        /// <summary>
        /// ドラッグ開始フック
        /// </summary>
        public DragBeginAsync DragBeginAsync
        {
            get { return (DragBeginAsync)GetValue(DragBeginAsyncProperty); }
            set { SetValue(DragBeginAsyncProperty, value); }
        }

        public static readonly DependencyProperty DragBeginAsyncProperty =
            DependencyProperty.Register("DragBeginAsync", typeof(DragBeginAsync), typeof(ContainerDragStartBehavior<TItem>), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグアンドドロップ操作の効果
        /// </summary>
        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects), typeof(ContainerDragStartBehavior<TItem>), new UIPropertyMetadata(DragDropEffects.All));


        /// <summary>
        /// 右ボタンドラッグを許可
        /// </summary>
        public bool AllowRightButtonDrag
        {
            get { return (bool)GetValue(AllowRightButtonDragProperty); }
            set { SetValue(AllowRightButtonDragProperty, value); }
        }

        public static readonly DependencyProperty AllowRightButtonDragProperty =
            DependencyProperty.Register("AllowRightButtonDrag", typeof(bool), typeof(ContainerDragStartBehavior<TItem>), new PropertyMetadata(false));


        /// <summary>
        /// ドラッグされるデータを識別する文字列。設定されている場合のみ既定でドラッグコントロールをDataObjectに追加する。
        /// </summary>
        public string DragDropFormat
        {
            get { return (string)GetValue(DragDropFormatProperty); }
            set { SetValue(DragDropFormatProperty, value); }
        }

        public static readonly DependencyProperty DragDropFormatProperty =
            DependencyProperty.Register("DragDropFormat", typeof(string), typeof(ContainerDragStartBehavior<TItem>), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグ有効
        /// </summary>
        public bool IsDragEnable
        {
            get { return (bool)GetValue(IsDragEnableProperty); }
            set { SetValue(IsDragEnableProperty, value); }
        }

        public static readonly DependencyProperty IsDragEnableProperty =
            DependencyProperty.Register("IsDragEnable", typeof(bool), typeof(ContainerDragStartBehavior<TItem>), new UIPropertyMetadata(true));


        /// <summary>
        /// 範囲外カーソルでの自動スクロール
        /// </summary>
        public bool IsAutoScroll
        {
            get { return (bool)GetValue(IsAutoScrollProperty); }
            set { SetValue(IsAutoScrollProperty, value); }
        }

        public static readonly DependencyProperty IsAutoScrollProperty =
            DependencyProperty.Register("IsAutoScroll", typeof(bool), typeof(ContainerDragStartBehavior<TItem>), new PropertyMetadata(false));



        /// <summary>
        /// 初期化
        /// </summary>
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove += PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp += PreviewMouseUpHandler;
            this.AssociatedObject.QueryContinueDrag += QueryContinueDragHandler;
            this.AssociatedObject.IsMouseCapturedChanged += IsMouseCapturedChangedHandler;
            base.OnAttached();
        }

        /// <summary>
        /// 後始末
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDown -= PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove -= PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp -= PreviewMouseUpHandler;
            this.AssociatedObject.QueryContinueDrag -= QueryContinueDragHandler;
            this.AssociatedObject.IsMouseCapturedChanged -= IsMouseCapturedChangedHandler;
            base.OnDetaching();
        }

        /// <summary>
        /// マウスボタン押したままの移動で選択項目が変更される機能を抑制
        /// </summary>
        private void IsMouseCapturedChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.IsMouseCaptured)
            {
                this.AssociatedObject.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// マウスボタン押下処理
        /// </summary>
        protected virtual void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }

            _origin = e.GetPosition(this.AssociatedObject);
            _isButtonDown = true;
            this.Dragged = false;

            if (sender is UIElement element)
            {
                if (element.InputHitTest(e.GetPosition(element)) is DependencyObject hitObject)
                {
                    _dragItem = hitObject is TItem item ? item : VisualTreeUtility.GetParentElement<TItem>(hitObject);

                    if (_dragItem != null)
                    {
                        _adornerVisual = GetAdornerVisual(_dragItem) ?? _dragItem;
                        _dragStartPos = e.GetPosition(_adornerVisual);
                    }
                }
            }
            else
            {
                _dragItem = null;
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }

            bool released = AllowRightButtonDrag ? (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released) : e.LeftButton == MouseButtonState.Released;
            if (released || !_isButtonDown || _dragItem == null)
            {
                return;
            }

            var point = e.GetPosition(this.AssociatedObject);

            if (ContainerDragStartBehavior<TItem>.CheckDistance(point, _origin) && _cancellationTokenSource == null)
            {
                this.Dragged = true;
                _cancellationTokenSource = new CancellationTokenSource();
                _ = BeginDragAsync(sender, e, _cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// ドラッグ開始処理
        /// </summary>
        private async ValueTask BeginDragAsync(object sender, MouseEventArgs e, CancellationToken token)
        {
            if (_dragItem is null)
            {
                EndDrag();
                return;
            }

            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            var dataObject = new DataObject();
            if (this.DragDropFormat != null)
            {
                dataObject.SetData(this.DragDropFormat, _dragItem);
            }

            var args = new DragStartEventArgs(e, _dragItem, dataObject, this.AllowedEffects);

            if (DragBeginAsync != null)
            {
                var control = (FrameworkElement)sender;
                try
                {
                    control.Cursor = Cursors.Wait;
                    control.CaptureMouse();
                    await DragBeginAsync(sender, args, token);
                }
                catch (OperationCanceledException)
                {
                    args.Cancel = true;
                }
                finally
                {
                    control.Cursor = null;
                    control.ReleaseMouseCapture();
                }
            }

            if (!args.Cancel)
            {
                AdornerLayer? layer = null;

                if (window != null)
                {
                    var dragCount = GetDragCount();
                    if (window.Content is UIElement root && _adornerVisual != null)
                    {
                        layer = AdornerLayer.GetAdornerLayer(root);
                        var offset = (Vector)root.TranslatePoint(new Point(0, 0), window);
                        _dragGhost = new DragAdorner(root, _adornerVisual, 0.5, dragCount, _dragStartPos + offset);
                        layer.Add(_dragGhost);
                    }
                }

                DragDropHook?.BeginDragDrop(sender, this.AssociatedObject, args.Data, args.AllowedEffects);

                try
                {
                    DragDrop.DoDragDrop(this.AssociatedObject, args.Data, args.AllowedEffects);
                }
                catch (Exception ex)
                {
                    // ドラッグ先のアプリで発生した例外が戻されることがあるので、ここで握りつぶす。...いいのか？
                    Debug.WriteLine(ex.Message);
                }

                args.DragEndAction?.Invoke();
                DragDropHook?.EndDragDrop(sender, this.AssociatedObject, args.Data, args.AllowedEffects);

                layer?.Remove(_dragGhost);
                _dragGhost = null;
            }

            EndDrag();
        }

        private void EndDrag()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _isButtonDown = false;
            _dragItem = null;
        }


        /// <summary>
        /// マウスボタンリリース処理
        /// </summary>
        protected virtual void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// ドラッグ項目数を取得
        /// </summary>
        protected virtual int GetDragCount()
        {
            return 1;
        }

        public virtual UIElement? GetAdornerVisual(TItem dragItem)
        {
            return dragItem;
        }


        /// <summary>
        /// 座標検査
        /// </summary>
        private static bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        /// ゴーストの移動処理
        /// Window全体に、ゴーストが移動するタイプのドラッグを想定している
        /// </summary>
        private void QueryContinueDragHandler(object sender, QueryContinueDragEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }

            if (sender is not Visual visual)
            {
                return;
            }

            try
            {
                if (_dragGhost != null)
                {
                    var point = CursorInfo.GetNowPosition(visual);
                    if (double.IsNaN(point.X))
                    {
                        Debug.WriteLine("_dragItem does not exist in virtual tree.");
                        e.Action = System.Windows.DragAction.Cancel;
                        e.Handled = true;
                        return;
                    }
                    _dragGhost.LeftOffset = point.X;
                    _dragGhost.TopOffset = point.Y;
                }

                if (IsAutoScroll)
                {
                    AutoScroll(sender, e);
                }

                //e.Handled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// ドラッグがターゲットの外にある時に自動スクロールさせる.
        /// </summary>
        private void AutoScroll(object sender, QueryContinueDragEventArgs e)
        {
            if (sender is not FrameworkElement container)
            {
                return;
            }

            ScrollViewer? scrollViewer = VisualTreeUtility.FindVisualChild<ScrollViewer>(container);
            if (scrollViewer == null)
            {
                return;
            }

            if (Window.GetWindow(container)?.Content is not FrameworkElement root)
            {
                // container does not exist in virtual tree.
                return;
            }
            var cursor = CursorInfo.GetNowPosition(root);
            if (double.IsNaN(cursor.X))
            {
                return;
            }

            var point = root.TranslatePoint(cursor, container);
            double offset = VirtualizingPanel.GetScrollUnit(container) == ScrollUnit.Pixel ? _dragGhost != null ? _dragGhost.ActualHeight * 0.5 : 20.0 : 1.0;

            if (point.Y < 0.0)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset);
            }
            else if (point.Y > container.ActualHeight)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
            }
        }
    }


    /// <summary>
    /// TreeView DragDropStartBehavior
    /// </summary>
    public class TreeViewDragDropStartBehavior : ContainerDragStartBehavior<TreeViewItem>
    {
        public override UIElement? GetAdornerVisual(TreeViewItem dragItem)
        {
            return VisualTreeUtility.FindVisualChild<ContentPresenter>(dragItem);
        }
    }

    /// <summary>
    /// ListBox DragDropStartBehavior
    /// </summary>
    public class ListBoxDragDropStartBehavior : ContainerDragStartBehavior<ListBoxItem>
    {
        public override UIElement? GetAdornerVisual(ListBoxItem dragItem)
        {
            return VisualTreeUtility.FindVisualChild<ContentPresenter>(dragItem);
        }
    }

    /// <summary>
    /// ListBoxExtended DragDropStartBehavior
    /// Ctrlキーでの選択解除前にドラッグできるようにする処理
    /// </summary>
    public class ListBoxExtendedDragDropStartBehavior : ListBoxDragDropStartBehavior
    {
        private List<object>? _selectedItems;

        protected override void OnAttached()
        {
            base.OnAttached();
            Debug.Assert(this.AssociatedObject is ListBoxExtended);
        }

        protected override void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            base.PreviewMouseDownHandler(sender, e);

            var listBox = (ListBoxExtended)this.AssociatedObject;

            _selectedItems = null;

            if (listBox.SelectedItems.Count > 1 && this.DragItem != null && listBox.SelectedItems.Contains(this.DragItem.DataContext))
            {
                switch (Keyboard.Modifiers)
                {
                    case ModifierKeys.None:
                        _selectedItems = new List<object>() { this.DragItem.DataContext };
                        e.Handled = true;
                        break;

                    case ModifierKeys.Control:
                        _selectedItems = listBox.SelectedItems.Cast<object>().Where(x => x != this.DragItem.DataContext).ToList();
                        e.Handled = true;
                        break;
                }
            }
        }

        protected override void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            base.PreviewMouseUpHandler(sender, e);

            var listBox = (ListBoxExtended)this.AssociatedObject;

            if (e.ChangedButton == MouseButton.Right)
            {
                return;
            }

            if (_selectedItems != null && !this.Dragged)
            {
                listBox.SetSelectedItems(_selectedItems);
                listBox.SetAnchorItem(null);
                listBox.RaisePreviewMouseUpWithSelectionChanged(sender, e);
            }

            _selectedItems = null;
        }


        protected override int GetDragCount()
        {
            var listBox = (ListBoxExtended)this.AssociatedObject;

            return listBox.SelectedItems.Count;
        }
    }
}
