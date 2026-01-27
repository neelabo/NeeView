using System;
using System.Windows;

namespace NeeView.ComponentModel
{
    public class RectChangeEventArgs : EventArgs
    {
        public RectChangeEventArgs(Rect newRect, Rect previousRect)
        {
            NewRect = newRect;
            PreviousRect = previousRect;
        }

        public Rect NewRect { get; }
        public Rect PreviousRect { get; }

        public bool XChanged => NewRect.X != PreviousRect.X;
        public bool YChanged => NewRect.Y != PreviousRect.Y;
        public bool WidthChanged => NewRect.Width != PreviousRect.Width;
        public bool HeightChanged => NewRect.Height != PreviousRect.Height;
        public bool PointChanged => XChanged || YChanged;
        public bool SizeChanged => WidthChanged || HeightChanged;
    }

}
