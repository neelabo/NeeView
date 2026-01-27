//#define LOCAL_DEBUG

using System;

namespace NeeView
{
    public class PlaylistSavedEventArgs : EventArgs
    {
        public PlaylistSavedEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    }
}
