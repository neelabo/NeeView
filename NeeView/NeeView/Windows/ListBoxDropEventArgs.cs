using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    public class ListBoxDropEventArgs : EventArgs
    {
        public ListBoxDropEventArgs(DragEventArgs dragEventArgs, ListBoxItem? item, int delta)
        {
            DragEventArgs = dragEventArgs;
            Item = item;
            Delta = delta;
        }

        public DragEventArgs DragEventArgs { get; }

        public ListBoxItem? Item { get; }

        public int Delta { get; }
    }
}