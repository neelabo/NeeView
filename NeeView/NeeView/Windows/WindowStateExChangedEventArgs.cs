using System;

namespace NeeView.Windows
{
    public class WindowStateExChangedEventArgs : EventArgs
    {
        public WindowStateExChangedEventArgs(WindowStateEx oldState, WindowStateEx newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public WindowStateEx OldState { get; set; }
        public WindowStateEx NewState { get; set; }
    }
}
