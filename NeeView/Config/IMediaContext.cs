using System;
using System.ComponentModel;

namespace NeeView
{
    public interface IMediaContext : INotifyPropertyChanged
    {
        bool IsMuted { get; set; }
        bool IsRepeat { get; set; }
        double MediaStartDelaySeconds { get; set; }
        double Volume { get; set; }

        IDisposable SubscribePropertyChanged(string propertyName, PropertyChangedEventHandler handler);
    }
}
