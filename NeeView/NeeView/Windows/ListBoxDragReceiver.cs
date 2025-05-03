using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows
{
    public enum ListBoxDragReceiveItemType
    {
        None = 0,
        Folder = (1 << 0),
        Item = (1 << 1),
        All = Folder | Item
    }

    public class ListBoxDragReceiver
    {
        protected readonly ListBox _listBox;
        private RectangleAdorner? _adorner;


        public ListBoxDragReceiver(ListBox listBox)
        {
            _listBox = listBox;

            _listBox.PreviewDragEnter += OnPreviewDragEnter;
            _listBox.PreviewDragLeave += OnPreviewDragLeave;
            _listBox.PreviewDragOver += OnPreviewDragOver;
            _listBox.Drop += OnDrop;
        }


        public event DragEventHandler? PreviewDragOver;
        public event EventHandler<ListBoxDropEventArgs>? DragOver;
        public event EventHandler<ListBoxDropEventArgs>? Drop;


        protected RectangleAdorner Adorner => EnsureAdorner();
        public Brush ShapeBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));
        public Brush SplitBrush { get; set; } = new SolidColorBrush(Colors.Black);
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        public bool AllowInsert { get; set; } = true;
        public ListBoxDragReceiveItemType ReceiveItemType { get; set; } = ListBoxDragReceiveItemType.All;


        private RectangleAdorner EnsureAdorner()
        {
            if (_adorner is null)
            {
                // NOTE: ClipToBounds 効果のあるコントロールに関連付ける
                var presenter = VisualTreeUtility.GetChildElement<ScrollContentPresenter>(_listBox);
                Debug.Assert(presenter != null);
                _adorner = new RectangleAdorner(presenter) { IsClipEnabled = true };
            }
            return _adorner;
        }

        protected virtual void OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            OnPreviewDragOver(sender, e);
            Adorner.Attach();
        }

        private void OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            Adorner.Detach();
        }

        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            PreviewDragOver?.Invoke(sender, e);
            if (e.Handled)
            {
                Adorner.Visibility = Visibility.Collapsed;
                return;
            }

            var (item, delta) = PointToViewItemDelta(_listBox, e, Orientation);

            DragOver?.Invoke(sender, new ListBoxDropEventArgs(e, item, delta));

            if (item is not null && e.Effects != DragDropEffects.None)
            {
                var isVisible = SetAdornerShape(item, delta, Orientation);
                Adorner.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                Adorner.Visibility = Visibility.Collapsed;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            Adorner.Detach();

            var (item, delta) = PointToViewItemDelta(_listBox, e, Orientation);
            Drop?.Invoke(sender, new ListBoxDropEventArgs(e, item, delta));
        }

        (ListBoxItem? item, int delta) PointToViewItemDelta(ListBox listBox, DragEventArgs e, Orientation orientation)
        {
            var (item, rate) = ListBoxTools.PointToViewItemRate(_listBox, e, Orientation);
            var delta = item is not null && AllowInsert ? GetInsertOffset(rate, IsFolder(item)) : 0;
            return (item, delta);
        }

        protected virtual bool IsFolder(ListBoxItem? item)
        {
            return false;
        }

        protected virtual bool Accept(ListBoxItem? item, int delta, DragEventArgs e)
        {
            return true;
        }

        private int GetInsertOffset(double rate, bool isFolder)
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

        private bool SetAdornerShape(ListBoxItem item, int delta, Orientation orientation)
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

        private bool AllowReceiveItem(ListBoxItem item)
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

        private void SetAdornerVerticalSplit(ListBoxItem item, VerticalAlignment vertical)
        {
            var next = GetVerticalItem(item, vertical);
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

        private void SetAdornerHorizontalSplit(ListBoxItem item, HorizontalAlignment horizontal)
        {
            var next = GetHorizontalItem(item, horizontal);
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

        private ListBoxItem? GetVerticalItem(ListBoxItem item, VerticalAlignment vertical)
        {
            var point = GetElementCenter(item);
            point.Y += item.ActualHeight * vertical.Direction() * 1.0;
            return VisualTreeUtility.HitTest<ListBoxItem>(_listBox, point);
        }

        private ListBoxItem? GetHorizontalItem(ListBoxItem item, HorizontalAlignment horizontal)
        {
            var point = GetElementCenter(item);
            point.X += item.ActualWidth * horizontal.Direction() * 1.0;
            return VisualTreeUtility.HitTest<ListBoxItem>(_listBox, point);
        }

        private (Point p0, Point p1) GetElementRect(FrameworkElement element)
        {
            var p0 = element.TranslatePoint(new Point(0, 0), _listBox);
            var p1 = p0 + new Vector(element.ActualWidth, element.ActualHeight);
            return (p0, p1);
        }

        private Point GetElementCenter(FrameworkElement element)
        {
            return element.TranslatePoint(new Point(element.ActualWidth * 0.5, element.ActualHeight * 0.5), _listBox);
        }
    }


    public static class VerticalAlignmentExtensions
    {
        public static double Direction(this VerticalAlignment vertical)
        {
            return vertical switch
            {
                VerticalAlignment.Top => -1.0,
                VerticalAlignment.Bottom => 1.0,
                _ => 0.0
            };
        }
    }


    public static class HorizontalAlignmentExtensions
    {
        public static double Direction(this HorizontalAlignment horizontal)
        {
            return horizontal switch
            {
                HorizontalAlignment.Left => -1.0,
                HorizontalAlignment.Right => 1.0,
                _ => 0.0
            };
        }
    }

}