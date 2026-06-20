using System;
using System.Windows;
using System.Windows.Input;


namespace NeeView.Windows
{
    public class DragStartEventArgs : EventArgs
    {
        public DragStartEventArgs(MouseEventArgs mouseEventArgs, object dragItem, DataObject data, DragDropEffects allowedEffects)
        {
            this.MouseEventArgs = mouseEventArgs;
            this.DragItem = dragItem;
            this.Data = data;
            this.AllowedEffects = allowedEffects;
        }

        public MouseEventArgs MouseEventArgs { get; set; }

        public bool Cancel { get; set; }

        public object DragItem { get; set; }

        public DataObject Data { get; set; }

        public DragDropEffects AllowedEffects { get; set; }

        public Action? DragEndAction { get; set; }
        
        public int DragCount { get; set; }


        public Point GetPosition(IInputElement relativeTo)
        {
            return MouseEventArgs.GetPosition(relativeTo);
        }
    }
}
