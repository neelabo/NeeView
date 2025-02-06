namespace NeeView
{
    public record class MediaPlayerAccessor
    {
        private readonly IMediaPlayer _player;

        public MediaPlayerAccessor(IMediaPlayer player)
        {
            _player = player;
        }

        [WordNodeMember]
        public bool IsDisposed => _player.IsDisposed;

        [WordNodeMember]
        public double Duration
        {
            get => AppDispatcher.Invoke(() => _player.Duration.HasTimeSpan ? _player.Duration.TimeSpan.TotalSeconds : 0.0);
        }

        [WordNodeMember]
        public TrackCollectionAccessor? AudioTrack
        {
            get => _player.AudioTracks is not null ? new TrackCollectionAccessor(_player.AudioTracks) : null;
        }

        [WordNodeMember]
        public TrackCollectionAccessor? Subtitle
        {
            get => _player.Subtitles is not null ? new TrackCollectionAccessor(_player.Subtitles) : null;
        }

        [WordNodeMember]
        public double Volume
        {
            get => Config.Current.Archive.Media.Volume;
            set => AppDispatcher.Invoke(() => Config.Current.Archive.Media.Volume = value);
        }

        [WordNodeMember]
        public bool IsMuted
        {
            get => Config.Current.Archive.Media.IsMuted;
            set => AppDispatcher.Invoke(() => Config.Current.Archive.Media.IsMuted = value);
        }

        [WordNodeMember]
        public bool IsRepeat
        {
            get => _player.IsRepeat;
            set => AppDispatcher.Invoke(() => _player.IsRepeat = value);
        }

        [WordNodeMember]
        public double Position
        {
            get => AppDispatcher.Invoke(() => _player.Position);
            set => AppDispatcher.Invoke(() => _player.Position = value);
        }

        [WordNodeMember]
        public bool IsPlaying
        {
            get => _player.IsPlaying;
            set => AppDispatcher.Invoke(() =>
            {
                if (value)
                {
                    _player.Play();
                }
                else
                {
                    _player.Pause();
                }
            });
        }

        [WordNodeMember]
        public double Rate
        {
            get => _player.Rate;
            set => AppDispatcher.Invoke(() => _player.Rate = value);
        }
    }
}
