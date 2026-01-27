using System;
using System.Windows;

namespace NeeView.Windows
{
    public class DropInfo<T>
        where T : class
    {
        public DropInfo(T data, T dropTarget, double position)
        {
            Data = data;
            DropTarget = dropTarget;
            Position = position;
        }

        public DropInfo(DragEventArgs e, string format, T dropTarget, FrameworkElement dropTargetVisual)
        {
            var data = e.Data.GetData(format) as T ?? throw new ArgumentException($"dropData must be type {typeof(T).Name}", nameof(e));

            Data = data;
            DropTarget = dropTarget;

            var pos = e.GetPosition(dropTargetVisual);
            Position = pos.Y / dropTargetVisual.ActualHeight;
        }

        public T Data { get; set; }
        public T DropTarget { get; set; }
        public double Position { get; set; }

        public bool IsValid()
        {
            return Data != null && DropTarget != null;
        }
    }

}
