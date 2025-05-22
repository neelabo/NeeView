using System;

namespace NeeView
{
    public class BreadcrumbBarEventArgs : EventArgs
    {
        public BreadcrumbBarEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
