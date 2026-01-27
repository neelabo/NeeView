using System;
using System.Windows;

namespace NeeView
{
    public interface IViewContentStrategy : IDisposable
    {
        FrameworkElement CreateLoadedContent(object data);
        void OnSourceChanged();
    }
}
