using System;
using System.Collections.Specialized;

namespace NeeLaboratory.ComponentModel
{
    public static class NotifyCollectionChangedExtensions
    {
        public static IDisposable SubscribeCollectionChanged(this INotifyCollectionChanged obj, NotifyCollectionChangedEventHandler handler)
        {
            obj.CollectionChanged += handler;
            return new AnonymousDisposable(() => obj.CollectionChanged -= handler);
        }

    }
}
