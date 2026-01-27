using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    // from https://stackoverflow.com/questions/5047576/wpf-treeview-how-to-style-selected-items-with-rounded-corners-like-in-explorer
    // from  https://stackoverflow.com/questions/664632/highlight-whole-treeviewitem-line-in-wpf

    public class TreeViewItemTools
    {
        private static TreeViewItem? CurrentItem;


        private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent("UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewItemTools));


        public static readonly DependencyProperty IndentLengthProperty =
            DependencyProperty.RegisterAttached("IndentLength", typeof(double), typeof(TreeViewItemTools), new PropertyMetadata(10.0));

        public static readonly DependencyProperty VerticalMarginProperty =
            DependencyProperty.RegisterAttached("VerticalMargin", typeof(double), typeof(TreeViewItemTools), new PropertyMetadata(0.0));

        private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey =
            DependencyProperty.RegisterAttachedReadOnly("IsMouseDirectlyOverItem", typeof(bool), typeof(TreeViewItemTools), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CalculateIsMouseDirectlyOverItem)));

        public static readonly DependencyProperty IsMouseDirectlyOverItemProperty =
            IsMouseDirectlyOverItemKey.DependencyProperty;


        static TreeViewItemTools()
        {
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseEnterEvent, new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseLeaveEvent, new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UpdateOverItemEvent, new RoutedEventHandler(OnUpdateOverItem));
        }


        public static double GetIndentLength(DependencyObject obj)
        {
            return (double)obj.GetValue(IndentLengthProperty);
        }

        public static void SetIndentLength(DependencyObject obj, double value)
        {
            obj.SetValue(IndentLengthProperty, value);
        }

        public static double GetVerticalMargin(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalMarginProperty);
        }

        public static void SetVerticalMargin(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalMarginProperty, value);
        }

        public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
        }

        private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
        {
            return item == CurrentItem;
        }

        private static void OnUpdateOverItem(object sender, RoutedEventArgs e)
        {
            CurrentItem = sender as TreeViewItem;
            if (CurrentItem != null)
            {
                CurrentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
                e.Handled = true;
            }
        }

        private static void OnMouseTransition(object sender, MouseEventArgs e)
        {
            lock (IsMouseDirectlyOverItemProperty)
            {
                if (CurrentItem != null)
                {
                    DependencyObject oldItem = CurrentItem;
                    CurrentItem = null;
                    oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
                }

                Mouse.DirectlyOver?.RaiseEvent(new RoutedEventArgs(UpdateOverItemEvent));
            }
        }
    }


    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            TreeViewItem? parent;
            while ((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private static TreeViewItem? GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }


    public class TreeViewIndentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is TreeViewItem item && values[1] is double length)
            {
                return new GridLength(length * item.GetDepth());
            }

            return new GridLength(0.0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class TreeViewVerticalMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is TreeViewItem item && item.GetDepth() == 0 && values[1] is double length)
            {
                return new Thickness(0, length, 0, length);
            }

            return default(Thickness);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
