using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows
{
    /// <summary>
    /// リストコントロールへの挿入ドロップ補助
    /// </summary>
    public class InsertDropAssist
    {
        protected readonly FrameworkElement _itemsControl;
        private readonly DropAssistProfile _profile;
        private RectangleAdorner? _adorner;


        public InsertDropAssist(FrameworkElement itemsControl, DropAssistProfile profile)
        {
            _itemsControl = itemsControl;
            _profile = profile;
        }


        protected RectangleAdorner Adorner => EnsureAdorner();
        public Brush ShapeBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));
        public Brush SplitBrush { get; set; } = Brushes.Black;
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        public bool AllowInsert { get; set; } = true;
        public InsertDropItemType ReceiveItemType { get; set; } = InsertDropItemType.All;


        private void InitializeAdornerParameter()
        {
            SplitBrush = App.Current.Resources["Control.Foreground"] as Brush ?? Brushes.Black;
        }

        private RectangleAdorner EnsureAdorner()
        {
            if (_adorner is null)
            {
                _adorner = new RectangleAdorner(_profile.GetAdornerTarget(_itemsControl)) { IsClipEnabled = true };
            }
            return _adorner;
        }

        public virtual void OnDragEnter(object? sender, DragEventArgs e)
        {
            InitializeAdornerParameter();
            Adorner.Attach();
        }

        public virtual void OnDragLeave(object? sender, DragEventArgs e)
        {
            Adorner.Detach();
        }

        public virtual DropTargetItem OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Handled)
            {
                Adorner.Visibility = Visibility.Collapsed;
                return new DropTargetItem(null, 0);
            }

            var target = _profile.PointToDropTargetItem(e, _itemsControl, AllowInsert, Orientation);

            if (e.Effects == DragDropEffects.None)
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

            return target;
        }

        public virtual DropTargetItem OnDrop(object? sender, DragEventArgs e)
        {
            Adorner.Detach();
            return _profile.PointToDropTargetItem(e, _itemsControl, AllowInsert, Orientation);
        }

        public void HideAdorner()
        {
            Adorner.Visibility = Visibility.Collapsed;
        }

        private void SetAdornerDefaultShape(FrameworkElement itemsControl, Orientation orientation)
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

        private bool SetAdornerShape(FrameworkElement item, int delta, Orientation orientation)
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

        private bool AllowReceiveItem(FrameworkElement item)
        {
            if (ReceiveItemType == InsertDropItemType.All)
            {
                return true;
            }
            if (_profile.IsFolder(item))
            {
                return ReceiveItemType.HasFlag(InsertDropItemType.Folder);
            }
            else
            {
                return ReceiveItemType.HasFlag(InsertDropItemType.Item);
            }
        }

        private void SetAdornerShape(FrameworkElement element)
        {
            (var p0, var p1) = GetElementRect(element);
            Adorner.Start = p0;
            Adorner.End = p1;
            Adorner.Brush = ShapeBrush;
        }

        private void SetAdornerVerticalSplit(FrameworkElement item, VerticalAlignment vertical)
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

        private void SetAdornerHorizontalSplit(FrameworkElement item, HorizontalAlignment horizontal)
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

        private FrameworkElement? GetVerticalItem(FrameworkElement item, VerticalAlignment vertical)
        {
            var point = GetElementCenter(item);
            point.Y += item.ActualHeight * vertical.Direction() * 1.0;
            return _profile.ItemHitTest(_itemsControl, point);
        }

        private FrameworkElement? GetHorizontalItem(FrameworkElement item, HorizontalAlignment horizontal)
        {
            var point = GetElementCenter(item);
            point.X += item.ActualWidth * horizontal.Direction() * 1.0;
            return _profile.ItemHitTest(_itemsControl, point);
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


    /// <summary>
    /// ドロップターゲット項目情報
    /// </summary>
    /// <param name="Item">ターゲット項目</param>
    /// <param name="Delta">ターゲット項目の挿入位置。-1 or 0 or 1</param>
    public record DropTargetItem(FrameworkElement? Item, int Delta);
}
