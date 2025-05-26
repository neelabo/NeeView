using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;

namespace NeeView
{
    /// <summary>
    /// ItemsControl の挿入ドロップ補助
    /// </summary>
    /// <remarks>
    /// TODO: ListBoxDragReceiver と統合する
    /// </remarks>
    public class ItemsControlDropAssist
    {
        protected readonly ItemsControl _itemsControl;
        private RectangleAdorner? _adorner;


        public ItemsControlDropAssist(ItemsControl itemsControl)
        {
            _itemsControl = itemsControl;
        }


        protected RectangleAdorner Adorner => EnsureAdorner();
        public Brush ShapeBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));
        public Brush SplitBrush { get; set; } = new SolidColorBrush(Colors.Black);
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        public bool AllowInsert { get; set; } = true;
        public ListBoxDragReceiveItemType ReceiveItemType { get; set; } = ListBoxDragReceiveItemType.All;



        private void InitializeAdornerParameter()
        {
            SplitBrush = App.Current.Resources["Control.Foreground"] as Brush ?? Brushes.Black;
        }

        private RectangleAdorner EnsureAdorner()
        {
            if (_adorner is null)
            {
                _adorner = new RectangleAdorner(_itemsControl) { IsClipEnabled = true };
            }
            return _adorner;
        }


        public void OnDragEnter(object? sender, DragEventArgs e)
        {
            InitializeAdornerParameter();
            Adorner.Attach();
        }

        public void OnDragLeave(object? sender, DragEventArgs e)
        {
            Adorner.Detach();
        }

        public void OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Handled)
            {
                Adorner.Visibility = Visibility.Collapsed;
                return;
            }

            var panel = e.Data.GetData<LayoutPanel>();
            if (panel is null)
            {
                Adorner.Visibility = Visibility.Collapsed;
                return;
            }

            var target = PointToViewItemDelta(_itemsControl, e, Orientation);

            if (e.Effects == DragDropEffects.None || target.Item?.Content == panel)
            {
                Adorner.Visibility = Visibility.Collapsed;
            }
            else if (target.Item is not null)
            {
                var isVisible = SetAdornerShape(target.Item, target.Delta, Orientation);
                Adorner.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                SetAdornerDefaultShape(_itemsControl, Orientation);
                Adorner.Visibility = Visibility.Visible;
            }
        }

        public ItemsControlDropTarget OnDrop(object? sender, DragEventArgs e)
        {
            Adorner.Detach();
            return PointToViewItemDelta(_itemsControl, e, Orientation);
        }

        private ItemsControlDropTarget PointToViewItemDelta(ItemsControl listBox, DragEventArgs e, Orientation orientation)
        {
            var (item, rate) = ItemsControlTools.PointToViewItemRate(_itemsControl, e, Orientation);
            var delta = item is not null && AllowInsert ? GetInsertOffset(rate, IsFolder(item)) : 0;
            return new ItemsControlDropTarget(item, delta);
        }

        protected virtual bool IsFolder(ContentPresenter? item)
        {
            return false;
        }

        protected virtual bool Accept(ContentPresenter? item, int delta, DragEventArgs e)
        {
            return true;
        }

        private static int GetInsertOffset(double rate, bool isFolder)
        {
            if (isFolder)
            {
                return rate switch
                {
                    < 0.25 => -1,
                    > 0.75 => +1,
                    _ => 0
                };
            }
            else
            {
                return rate switch
                {
                    < 0.5 => -1,
                    _ => +1
                };
            }
        }

        private void SetAdornerDefaultShape(ItemsControl itemsControl, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                SetAdornerHorizontalSplit(itemsControl, null, HorizontalAlignment.Left);
            }
            else
            {
                SetAdornerVerticalSplit(itemsControl, null, VerticalAlignment.Top);
            }
        }

        private bool SetAdornerShape(ContentPresenter item, int delta, Orientation orientation)
        {
            switch (delta)
            {
                case -1:
                    if (AllowInsert)
                    {
                        if (orientation == Orientation.Horizontal)
                        {
                            SetAdornerHorizontalSplit(item, HorizontalAlignment.Left);
                        }
                        else
                        {
                            SetAdornerVerticalSplit(item, VerticalAlignment.Top);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 0:
                    if (AllowReceiveItem(item))
                    {
                        SetAdornerShape(item);
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case +1:
                    if (AllowInsert)
                    {
                        if (orientation == Orientation.Horizontal)
                        {
                            SetAdornerHorizontalSplit(item, HorizontalAlignment.Right);
                        }
                        else
                        {
                            SetAdornerVerticalSplit(item, VerticalAlignment.Bottom);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        private bool AllowReceiveItem(ContentPresenter item)
        {
            if (ReceiveItemType == ListBoxDragReceiveItemType.All)
            {
                return true;
            }
            if (IsFolder(item))
            {
                return ReceiveItemType.HasFlag(ListBoxDragReceiveItemType.Folder);
            }
            else
            {
                return ReceiveItemType.HasFlag(ListBoxDragReceiveItemType.Item);
            }
        }

        private void SetAdornerShape(FrameworkElement element)
        {
            (var p0, var p1) = GetElementRect(element);
            Adorner.Start = p0;
            Adorner.End = p1;
            Adorner.Brush = ShapeBrush;
        }

        private void SetAdornerVerticalSplit(ContentPresenter item, VerticalAlignment vertical)
        {
            var next = GetVerticalItem(item, vertical);
            SetAdornerVerticalSplit(item, next, vertical);
        }

        private void SetAdornerVerticalSplit(FrameworkElement item, FrameworkElement? next, VerticalAlignment vertical)
        {
            if (next is not null)
            {
                var p0 = GetElementCenter(item);
                var p1 = GetElementCenter(next);
                var x0 = p0.X - item.ActualWidth * 0.5;
                var x1 = p0.X + item.ActualWidth * 0.5;
                var y = (p0.Y + p1.Y) * 0.5;
                Adorner.Start = new Point(x0, y - 1.0);
                Adorner.End = new Point(x1, y + 1.0);
            }
            else
            {
                var direction = vertical.Direction();
                var p0 = GetElementCenter(item);
                var x0 = p0.X - item.ActualWidth * 0.5;
                var x1 = p0.X + item.ActualWidth * 0.5;
                var y = p0.Y + item.ActualHeight * 0.5 * direction;
                Adorner.Start = new Point(x0, y - 1.0 - direction);
                Adorner.End = new Point(x1, y + 1.0 - direction);
            }
            Adorner.Brush = SplitBrush;
        }

        private void SetAdornerHorizontalSplit(ContentPresenter item, HorizontalAlignment horizontal)
        {
            var next = GetHorizontalItem(item, horizontal);
            SetAdornerHorizontalSplit(item, next, horizontal);
        }

        private void SetAdornerHorizontalSplit(FrameworkElement item, FrameworkElement? next, HorizontalAlignment horizontal)
        {
            if (next is not null)
            {
                var p0 = GetElementCenter(item);
                var p1 = GetElementCenter(next);
                var x = (p0.X + p1.X) * 0.5;
                var y0 = p0.Y - item.ActualHeight * 0.5;
                var y1 = p0.Y + item.ActualHeight * 0.5;
                Adorner.Start = new Point(x - 1, y0);
                Adorner.End = new Point(x + 1, y1);
            }
            else
            {
                var direction = horizontal.Direction();
                var p0 = GetElementCenter(item);
                var x = p0.X + item.ActualWidth * 0.5 * direction;
                var y0 = p0.Y - item.ActualHeight * 0.5;
                var y1 = p0.Y + item.ActualHeight * 0.5;
                Adorner.Start = new Point(x - 1.0 - direction, y0);
                Adorner.End = new Point(x + 1.0 - direction, y1);
            }
            Adorner.Brush = SplitBrush;
        }

        private ContentPresenter? GetVerticalItem(ContentPresenter item, VerticalAlignment vertical)
        {
            var point = GetElementCenter(item);
            point.Y += item.ActualHeight * vertical.Direction() * 1.0;
            return ItemsControlTools.ItemHitTest(_itemsControl, point);
        }

        private ContentPresenter? GetHorizontalItem(ContentPresenter item, HorizontalAlignment horizontal)
        {
            var point = GetElementCenter(item);
            point.X += item.ActualWidth * horizontal.Direction() * 1.0;
            return ItemsControlTools.ItemHitTest(_itemsControl, point);
        }

        private (Point p0, Point p1) GetElementRect(FrameworkElement element)
        {
            var p0 = element.TranslatePoint(new Point(0, 0), _itemsControl);
            var p1 = p0 + new Vector(element.ActualWidth, element.ActualHeight);
            return (p0, p1);
        }

        private Point GetElementCenter(FrameworkElement element)
        {
            return element.TranslatePoint(new Point(element.ActualWidth * 0.5, element.ActualHeight * 0.5), _itemsControl);
        }
    }


    public record ItemsControlDropTarget(ContentPresenter? Item, int Delta);
}
