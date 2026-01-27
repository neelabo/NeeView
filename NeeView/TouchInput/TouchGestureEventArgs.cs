using System;

namespace NeeView
{
    public class TouchGestureEventArgs : EventArgs
    {
        public TouchArea Area { get; set; }
        public bool Handled { get; set; }

        public TouchGestureEventArgs()
        {
        }

        public TouchGestureEventArgs(TouchArea area)
        {
            this.Area = area;
        }
    }
}
