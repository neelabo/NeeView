using System;
using System.Windows;

namespace NeeView.Windows
{
    public class WindowStateChangeEventArgs : EventArgs
    {
        public WindowStateChangeEventArgs(Window window, WindowStateEx oldState, WindowStateEx newState)
        {
            Window = window;
            OldState = oldState;
            NewState = newState;
        }

        public Window Window { get; set; }
        public WindowStateEx OldState { get; set; }
        public WindowStateEx NewState { get; set; }
    }

}
