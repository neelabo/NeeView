// from https://github.com/takanemu/WPFDragAndDropSample

using Microsoft.Xaml.Behaviors;
using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace NeeView.Windows
{
    /// <summary>
    /// ドラッグ対象オブジェクト用ビヘイビア
    /// <para>
    /// ListBoxの項目などのドラッグには <see cref="ContainerDragStartBehavior{TItem}">ContainerDragStartBehavior</see> を使用する
    /// </para>
    /// </summary>
    public class DragStartBehavior : Behavior<FrameworkElement>
    {
        private Point _origin;
        private bool _isButtonDown;
        private IInputElement? _dragItem;
        private Point _dragStartPos;
        private DragAdorner? _dragGhost;


        /// <summary>
        /// ドラッグ開始イベント
        /// </summary>
        public event EventHandler<DragStartEventArgs>? DragBegin;

        /// <summary>
        /// ドラッグ終了イベント
        /// </summary>
        public event EventHandler? DragEnd;


        /// <summary>
        /// DoDragDropのフック
        /// </summary>
        /// <remarks>
        /// staticなオブジェクトになることがあるので標準のプロパティにしている
        /// </remarks>
        public IDragDropHook? DragDropHook { get; set; }


        /// <summary>
        /// ドラッグアンドドロップ操作の効果
        /// </summary>
        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects), typeof(DragStartBehavior), new UIPropertyMetadata(DragDropEffects.All));

        /// <summary>
        /// ドラッグされるデータ
        /// </summary>
        public object DragDropData
        {
            get { return GetValue(DragDropDataProperty); }
            set { SetValue(DragDropDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.Register("DragDropData", typeof(object), typeof(DragStartBehavior), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグされるデータを識別する文字列(任意)
        /// </summary>
        public string DragDropFormat
        {
            get { return (string)GetValue(DragDropFormatProperty); }
            set { SetValue(DragDropFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDropFormatProperty =
            DependencyProperty.Register("DragDropFormat", typeof(string), typeof(DragStartBehavior), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグ先.
        /// 範囲外にドラッグされたときにターゲットをスクロールさせる
        /// </summary>
        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(DragStartBehavior), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグ有効
        /// </summary>
        public bool IsDragEnable
        {
            get { return (bool)GetValue(IsDragEnableProperty); }
            set { SetValue(IsDragEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDragEnableProperty =
            DependencyProperty.Register("IsDragEnable", typeof(bool), typeof(DragStartBehavior), new UIPropertyMetadata(true));


        /// <summary>
        /// 初期化
        /// </summary>
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove += PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp += PreviewMouseUpHandler;
            this.AssociatedObject.QueryContinueDrag += QueryContinueDragHandler;
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
            base.OnDetaching();
        }

        /// <summary>
        /// マウスボタン押下処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            _origin = e.GetPosition(this.AssociatedObject);
            _isButtonDown = true;

            if (sender is IInputElement)
            {
                // マウスダウンされたアイテムを記憶
                _dragItem = sender as IInputElement;
                // マウスダウン時の座標を取得
                _dragStartPos = e.GetPosition(_dragItem);
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            if (e.LeftButton != MouseButtonState.Pressed || !_isButtonDown)
            {
                return;
            }
            if (_dragItem == null)
            {
                return;
            }

            var point = e.GetPosition(this.AssociatedObject);

            if (CheckDistance(point, _origin) && _dragGhost == null)
            {
                // アクティブWindowの直下のContentに対して、Adornerを付加する
                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

                var dataObject = new DataObject();
                if (this.DragDropData != null)
                {
                    if (this.DragDropFormat != null)
                    {
                        dataObject.SetData(this.DragDropFormat, this.DragDropData);
                    }
                    else
                    {
                        dataObject.SetData(this.DragDropData);
                    }
                }

                var args = new DragStartEventArgs(e, _dragItem, dataObject, this.AllowedEffects);

                DragBegin?.Invoke(sender, args);
                if (!args.Cancel)
                {
                    if (window != null)
                    {
                        var root = window.Content as UIElement;
                        var layer = AdornerLayer.GetAdornerLayer(root);

                        var ghost = (FrameworkElement)sender;
                        var dragStartPos = _dragStartPos;
                        if (this.DragDropData is IHasDragGhost hasDragGhost && hasDragGhost.GetDragGhost() != null)
                        {
                            ghost = hasDragGhost.GetDragGhost();
                            /*
                            var size = new Size(ghost.Width, ghost.Height);
                            ghost.Measure(size);
                            ghost.Arrange(new Rect(size));
                            ghost.UpdateLayout();
                            */
                            var bounds = VisualTreeHelper.GetDescendantBounds(ghost);
                            dragStartPos = new Point(bounds.Width * 0.5, bounds.Height * 0.5);
                        }

                        if (root != null && ghost != null)
                        {
                            _dragGhost = new DragAdorner(root, ghost, 0.5, 0, dragStartPos);
                            layer.Add(_dragGhost);
                        }

                        DragDropHook?.BeginDragDrop(sender, this.AssociatedObject, args.Data, args.AllowedEffects);
                        DragDrop.DoDragDrop(this.AssociatedObject, args.Data, args.AllowedEffects);
                        DragDropHook?.EndDragDrop(sender, this.AssociatedObject, args.Data, args.AllowedEffects);

                        if (_dragGhost != null)
                        {
                            layer.Remove(_dragGhost);
                        }
                    }
                    else
                    {
                        DragDropHook?.BeginDragDrop(sender, this.AssociatedObject, args.Data, args.AllowedEffects);
                        DragDrop.DoDragDrop(this.AssociatedObject, args.Data, args.AllowedEffects);
                        DragDropHook?.EndDragDrop(sender, this.AssociatedObject, args.Data, args.AllowedEffects);
                    }
                }

                _isButtonDown = false;
                e.Handled = true;
                _dragGhost = null;
                _dragItem = null;

                DragEnd?.Invoke(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// マウスボタンリリース処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        /// <summary>
        /// 座標検査
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        /// ゴーストの移動処理
        /// Window全体に、ゴーストが移動するタイプのドラッグを想定している
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryContinueDragHandler(object sender, QueryContinueDragEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            if (_dragItem == null)
            {
                return;
            }

            try
            {
                if (_dragGhost != null)
                {
                    var point = CursorInfo.GetNowPosition((Visual)_dragItem);
                    if (double.IsNaN(point.X))
                    {
                        e.Action = System.Windows.DragAction.Cancel;
                        e.Handled = true;
                        return;
                    }
                    _dragGhost.LeftOffset = point.X;
                    _dragGhost.TopOffset = point.Y;
                }

                if (this.Target != null)
                {
                    AutoScroll(this.Target, e);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// ドラッグがターゲットの外にある時に自動スクロールさせる
        /// </summary>
        /// <param name="container"></param>
        /// <param name="e"></param>
        private void AutoScroll(FrameworkElement container, QueryContinueDragEventArgs _)
        {
            ScrollViewer? scrollViewer = VisualTreeUtility.FindVisualChild<ScrollViewer>(container);
            if (scrollViewer == null)
            {
                return;
            }

            var root = (FrameworkElement)Window.GetWindow(container).Content;
            if (root == null)
            {
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
}
