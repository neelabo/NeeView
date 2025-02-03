using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    public interface IMediaPlayer : INotifyPropertyChanged, IDisposable
    {
        bool HasAudio { get; }
        bool HasVideo { get; }
        bool IsEnabled { get; set; }
        bool IsAudioEnabled { get; set; }
        bool IsMuted { get; set; }
        bool IsRepeat { get; set; }
        bool IsPlaying { get; }
        Duration Duration { get; }
        double Position { get; set; }
        bool ScrubbingEnabled { get; }
        double Volume { get; set; }
        bool RateEnabled { get; }
        double Rate { get; set; }
        TrackCollection? AudioTracks { get; }
        TrackCollection? Subtitles { get; }
        bool CanControlTracks { get; }

        event EventHandler? MediaEnded;
        event EventHandler<ExceptionEventArgs>? MediaFailed;
        event EventHandler? MediaOpened;
        event EventHandler? MediaPlayed;

        void Play();
        void Pause();

        IDisposable SubscribePropertyChanged(string propertyName, PropertyChangedEventHandler handler);
    }
}
