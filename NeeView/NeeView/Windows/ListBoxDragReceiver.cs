using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows
{
    //public enum ListBoxDragReceiverMode
    //{
    //    Insert,
    //    Folder,
    //}

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



        protected RectangleAdorner Adorner => _adorner ??= new RectangleAdorner(_listBox);
        public Brush ShapeBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80));
        public Brush SplitBrush { get; set; } = new SolidColorBrush(Colors.Black);
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        //public ListBoxDragReceiverMode Mode { get; set; } = ListBoxDragReceiverMode.Insert;

        public bool AllowInsert { get; set; } = true;
        public ListBoxDragReceiveItemType ReceiveItemType { get; set; } = ListBoxDragReceiveItemType.All;


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

        private void SetAdornerVerticalSplit(FrameworkElement element, VerticalAlignment vertical)
        {
            (var p0, var p1) = GetElementRect(element);
            if (vertical == VerticalAlignment.Top)
            {
                Adorner.Start = new Point(p0.X, p0.Y - 1);
                Adorner.End = new Point(p1.X, p0.Y + 1);
            }
            else
            {
                Adorner.Start = new Point(p0.X, p1.Y - 1);
                Adorner.End = new Point(p1.X, p1.Y + 1);
            }
            Adorner.Brush = SplitBrush;
        }

        private void SetAdornerHorizontalSplit(FrameworkElement element, HorizontalAlignment horizontal)
        {
            (var p0, var p1) = GetElementRect(element);
            if (horizontal == HorizontalAlignment.Left)
            {
                Adorner.Start = new Point(p0.X - 1, p0.Y);
                Adorner.End = new Point(p0.X + 1, p1.Y);
            }
            else
            {
                Adorner.Start = new Point(p1.X - 1, p0.Y);
                Adorner.End = new Point(p1.X + 1, p1.Y);
            }
            Adorner.Brush = SplitBrush;
        }

        private (Point p0, Point p1) GetElementRect(FrameworkElement element)
        {
            var p0 = element.TranslatePoint(new Point(0, 0), _listBox);
            var p1 = p0 + new Vector(element.ActualWidth, element.ActualHeight);
            return (p0, p1);
        }
    }
}