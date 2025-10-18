// 開発用：VLCイベント出力
//#define VLC_DUMP_EVEMT

// プレイヤー単位のオーディオOFFをトラックで管理する
//#define VLC_AUDIOENABLE_TRACK

// プレイヤー単位のオーディオOFFをミュートで管理する
#define VLC_AUDIOENABLE_MUTE

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Core.Interops.Signatures;
using Vlc.DotNet.Wpf;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class VlcMediaPlayer : IOpenableMediaPlayer, IDisposable
    {
        private readonly VlcVideoSourceProvider _source;
        private readonly Vlc.DotNet.Core.VlcMediaPlayer _player;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private bool _isEnabled = true;
        private bool _isAudioEnabled = true;
        private bool _isMuted;
        private bool _isRepeat;
        private bool _isPlaying;
        private bool _isOpened;
        private bool _hasAudio;
        private bool _hasVideo;
        private bool _scrubbingEnabled;
        private Duration _duration;
        private VlcTrackCollectionSource? _audioTracks;
        private VlcTrackCollectionSource? _subtitles;
        private Uri? _uri;
        private AudioInfo? _audioInfo;
        private Locker.Key? _activeLockerKey;

        /// <summary>
        /// 再生位置要求
        /// </summary>
        /// <remarks>
        /// 再生終了後の位置要求を処理するためのもの。リピートの切り替え処理が存在しないため、対応するために複雑な処理になっている。
        /// 再生停止後の再再生は位置が０になってしまい、設定タイミングによってはそれも無効化されてしまうため、この変数で位置を要求する。
        /// - NegativeInfinity で要求なし
        /// - Playing イベントで要求の位置に設定
        /// - 外部からの Position 設定で要求解除
        /// - PositionChanged イベントで要求位置より進んでいれば要求解除
        /// - 外部への Position はこの要求位置を加味した位置を渡す
        /// </remarks>
        private float _requestPosition = float.NegativeInfinity;


        public VlcMediaPlayer()
        {
            var libDirectory = new DirectoryInfo(Config.Current.Archive.Media.LibVlcPath);
            if (!libDirectory.Exists) throw new DirectoryNotFoundException($"The directory containing libvlc.dll does not exist: {libDirectory.FullName}");
            _source = new VlcVideoSourceProvider(Application.Current.Dispatcher);

            // dll version check
            var versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(libDirectory.FullName, "libvlc.dll"));
            if (versionInfo.FileMajorPart != 3) throw new NotSupportedException($"Not yet compatible with libvlc.dll version {versionInfo.FileVersion}. Only valid for version 3.x.");

            var options = new List<string>();
#if VLC_AUDIOENABLE_MUTE
            // hotfix for libvlc 3.x issue
            // https://code.videolan.org/videolan/vlc/-/issues/28194
            options.Add("--aout=directsound");
#endif
            _source.CreatePlayer(libDirectory, options.ToArray());
            _player = _source.MediaPlayer;

            AttachPlayer();


#if VLC_DUMP_EVEMT
            _player.MediaChanged += (s, e) => Trace($"MediaChanged: {e.NewMedia}");
            _player.Opening += (s, e) => Trace($"Opening: ");
            _player.Buffering += (s, e) => Trace($"Buffering: {e.NewCache}");
            _player.Playing += (s, e) => Trace($"Playing: ");
            _player.Paused += (s, e) => Trace($"Paused: ");
            _player.EndReached += (s, e) => Trace($"EndReached: ");
            _player.EncounteredError += (s, e) => Trace($"EncounteredError: ");
            _player.Corked += (s, e) => Trace($"Corked: ");
            _player.AudioDevice += (s, e) => Trace($"AudioDevice: {e.Device}");
            _player.Muted += (s, e) => Trace($"Muted: ");
            _player.AudioVolume += (s, e) => Trace($"AudioVolume: ");
            _player.ChapterChanged += (s, e) => Trace($"ChapterChanged: ");
            _player.Forward += (s, e) => Trace($"Forward: ");
            _player.Backward += (s, e) => Trace($"Backward: ");
            _player.EsAdded += (s, e) => Trace($"EsAdded: {e.Id}");
            _player.EsDeleted += (s, e) => Trace($"EsDeleted: {e.Id}");
            _player.EsSelected += (s, e) => Trace($"EsSelected: {e.Id}");
            _player.LengthChanged += (s, e) => Trace($"LengthChanged: {e.NewLength}");
            _player.TimeChanged += (s, e) => Trace($"TimeChanged: {e.NewTime}");
            _player.TitleChanged += (s, e) => Trace($"TitleChanged: {e.NewTitle}");
            _player.PausableChanged += (s, e) => Trace($"PausableChanged: {e.IsPaused}");
            _player.PositionChanged += (s, e) => Trace($"PositionChanged: {e.NewPosition}");
            _player.ScrambledChanged += (s, e) => Trace($"ScrambledChanged: {e.NewScrambled}");
            _player.SeekableChanged += (s, e) => Trace($"SeekableChanged: {e.NewSeekable}");
            _player.SnapshotTaken += (s, e) => Trace($"SnapshotTaken: {e.FileName}");
            _player.Stopped += (s, e) => Trace($"Stopped:");
            _player.Uncorked += (s, e) => Trace($"Uncorked:");
            _player.Unmuted += (s, e) => Trace($"Unmuted:");
            _player.VideoOutChanged += (s, e) => Trace($"VideoOutChanged: {e.NewCount}");
            //_player.Log += (s, e) => Trace($"Log: {e.Message} ");
#endif
        }



        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CS0067
        public event EventHandler? MediaOpened;
#pragma warning restore CS0067
        public event EventHandler? MediaEnded;
        public event EventHandler? MediaPlayed;
        public event EventHandler<ExceptionEventArgs>? MediaFailed;


        public VlcVideoSourceProvider SourceProvider => _source;

        public bool HasAudio
        {
            get { return _hasAudio; }
            private set
            {
                if (_disposedValue) return;
                SetProperty(ref _hasAudio, value);
            }
        }

        public bool HasVideo
        {
            get { return _hasVideo; }
            private set
            {
                if (_disposedValue) return;
                SetProperty(ref _hasVideo, value);
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_disposedValue) return;
                if (SetProperty(ref _isEnabled, value))
                {
                    UpdatePlayed();
                }
            }
        }

        public bool IsAudioEnabled
        {
            get { return _isAudioEnabled; }
            set
            {
                if (_disposedValue) return;
                if (SetProperty(ref _isAudioEnabled, value))
                {
                    UpdateAudioEnable();
                }
            }
        }

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (_disposedValue) return;
                if (SetProperty(ref _isMuted, value))
                {
                    UpdateMuted();
                }
            }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set
            {
                if (_disposedValue) return;
                if (SetProperty(ref _isRepeat, value))
                {
                    UpdateRepeat();
                }
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set
            {
                if (_disposedValue) return;
                if (SetProperty(ref _isPlaying, value))
                {
                    if (_isPlaying)
                    {
                        _activeLockerKey = _activeLockerKey ?? MainViewComponent.Current.LockActiveMarker();
                    }
                    else
                    {
                        _activeLockerKey?.Dispose();
                        _activeLockerKey = null;
                    }
                }
            }
        }

        public bool ScrubbingEnabled
        {
            get { return _scrubbingEnabled; }
            private set
            {
                if (_disposedValue) return;
                SetProperty(ref _scrubbingEnabled, value);
            }
        }

        public Duration Duration
        {
            get { return _duration; }
            set
            {
                if (_disposedValue) return;
                SetProperty(ref _duration, value);
            }
        }

        public double Position
        {
            get
            {
                if (_disposedValue) return 0.0;
                return _player.State == MediaStates.Ended ? 1.0 : Math.Max(_player.Position, _requestPosition);
            }
            set
            {
                if (_disposedValue) return;
                var newPosition = (float)value;
                if (_player.Position != newPosition)
                {
                    Trace($"Position = {newPosition}");
                    _requestPosition = float.NegativeInfinity;
                    Task.Run(() =>
                    {
                        if (_disposedValue) return;
                        _player.Position = newPosition;
                        if (_player.State == MediaStates.Ended)
                        {
                            PlayStart(newPosition);
                        }
                    });
                }
            }
        }

        public double Volume
        {
            get
            {
                if (_disposedValue) return 0.0;
                return _player.Audio.Volume / 100.0;
            }
            set
            {
                if (_disposedValue) return;
                var newVolume = (int)(value * 100.0);
                Task.Run(() =>
                {
                    if (_player.Audio.Volume != newVolume)
                    {
                        _player.Audio.Volume = newVolume;
                    }
                });
            }
        }

        public bool RateEnabled => true;

        public double Rate
        {
            get
            {
                if (_disposedValue) return 1.0;
                return (double)_player.Rate;
            }
            set
            {
                if (_disposedValue) return;
                var newRate = (float)value;
                if (0.01f < Math.Abs(_player.Rate - newRate))
                {
                    Task.Run(() =>
                    {
                        _player.Rate = newRate;
                        RaisePropertyChanged();
                    });
                }
            }
        }

        /// <summary>
        /// オーディオトラック、字幕の選択有効
        /// </summary>
        public bool CanControlTracks => true;

        /// <summary>
        /// オーディオトラックの選択管理
        /// </summary>
        public TrackCollection? AudioTracks
        {
            get
            {
                if (_disposedValue) return null;
                return _audioTracks?.Collection;
            }
        }

        /// <summary>
        /// 字幕の選択管理
        /// </summary>
        public TrackCollection? Subtitles
        {
            get
            {
                if (_disposedValue) return null;
                return _subtitles?.Collection;
            }
        }

        public int PositionChangeInterval => 200;
        public int ScrubbingInterval => 0;

        public bool IsDisposed => _disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DetachPlayer();
                    _audioTracks?.Dispose();
                    _subtitles?.Dispose();
                    _disposables.Dispose();
                    _activeLockerKey?.Dispose();
                    Task.Run(() =>
                    {
                        try
                        {
                            //Trace($"VlcMediaPlayer.Dispose: {System.Environment.TickCount} {_uri}");
                            _source.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            Debugger.Break();
                        }
                    });
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private VlcTrackCollectionSource? CreateAudioTracks()
        {
            var tracks = _player.Audio.Tracks;
            if (tracks is null || tracks.Count <= 0) return null;
            return new VlcTrackCollectionSource(tracks);
        }

        private VlcTrackCollectionSource? CreateSubtitles()
        {
            var tracks = _player.SubTitles;
            if (tracks is null || tracks.Count <= 0) return null;

            if (Config.Current.Archive.Media.DefaultSubtitle == DefaultSubtitle.Disable)
            {
                // 字幕を解除
                tracks.Current = tracks.All.FirstOrDefault();
            }

            return new VlcTrackCollectionSource(tracks);
        }

        private void AttachPlayer()
        {
            _player.Playing += Player_Playing;
            _player.EndReached += Player_EndReached;
            _player.EncounteredError += Player_EncounteredError;
            _player.PositionChanged += Player_PositionChanged;
            _player.AudioVolume += Player_AudioVolume;
            _player.SeekableChanged += Player_SeekableChanged;
        }

        private void DetachPlayer()
        {
            _player.Playing -= Player_Playing;
            _player.EndReached -= Player_EndReached;
            _player.EncounteredError -= Player_EncounteredError;
            _player.PositionChanged -= Player_PositionChanged;
            _player.AudioVolume -= Player_AudioVolume;
            _player.SeekableChanged -= Player_SeekableChanged;
        }


        public void Open(MediaSource mediaSource, TimeSpan _)
        {
            if (_disposedValue) return;

            if (mediaSource.Path is null) throw new ArgumentException("VlcMediaPlayer requests a Path from mediaSource.");
            _uri = new Uri(mediaSource.Path);
            _audioInfo = mediaSource.AudioInfo;

            _player.Playing += Player_FirstPlaying;

            //Trace($"VlcMediaPlayer.Open: {System.Environment.TickCount} {_uri}");
            PlayStart();

            IsPlaying = true;

            void Player_FirstPlaying(object? sender, VlcMediaPlayerPlayingEventArgs e)
            {
                _player.Playing -= Player_FirstPlaying;
                OnStarted();
            }
        }

        private void Player_Playing(object? sender, VlcMediaPlayerPlayingEventArgs e)
        {
            if (_disposedValue) return;

            Task.Run(() =>
            {
                Trace($"Playing: {_player.Position} => {_requestPosition}");
                UpdatePlayed();

                if (0.0 <= _requestPosition)
                {
                    _player.Position = _requestPosition;
                }
            });
        }

        private void Player_EndReached(object? sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(Position));
            AppDispatcher.BeginInvoke(() => MediaEnded?.Invoke(this, e));
        }

        private void Player_EncounteredError(object? sender, VlcMediaPlayerEncounteredErrorEventArgs e)
        {
            if (_disposedValue) return;

            AppDispatcher.BeginInvoke(() => MediaFailed?.Invoke(this, new ExceptionEventArgs(new ApplicationException("libVLC Failed"))));
        }

        private void Player_PositionChanged(object? sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (0.0 <= _requestPosition && _requestPosition <= e.NewPosition)
            {
                //Trace($"RequestPosition.Reset");
                _requestPosition = float.NegativeInfinity;
            }
        }

        private void Player_AudioVolume(object? sender, VlcMediaPlayerAudioVolumeEventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(Volume));
        }

        private void Player_SeekableChanged(object? sender, VlcMediaPlayerSeekableChangedEventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(ScrubbingEnabled));
        }


        private void OnStarted()
        {
            if (_disposedValue) return;

            _isOpened = true;

            _audioTracks?.Dispose();
            _audioTracks = CreateAudioTracks();

            _subtitles?.Dispose();
            _subtitles = CreateSubtitles();

            UpdatePlayed();
            UpdateMuted();
            UpdateTrackInfo();
            UpdateAudioEnable();

            ScrubbingEnabled = _player.IsSeekable;

            AppDispatcher.BeginInvoke(() => MediaPlayed?.Invoke(this, EventArgs.Empty));
        }

        public void Play()
        {
            if (_disposedValue) return;

            IsPlaying = true;
            UpdatePlayed();
        }

        public void Pause()
        {
            if (_disposedValue) return;

            IsPlaying = false;
            UpdatePlayed();
        }

        private void UpdateTrackInfo()
        {
            if (_disposedValue) return;

            var media = _player.GetMedia();
            HasAudio = media.Tracks.Any(e => e.Type == MediaTrackTypes.Audio) || _audioInfo is not null;
            HasVideo = media.Tracks.Any(e => e.Type == MediaTrackTypes.Video);
            Duration = new Duration(TimeSpan.FromMilliseconds(_player.Length));
        }

        private void UpdatePlayed()
        {
            if (_disposedValue) return;

            if (ShouldPlay())
            {
                if (_player.State == MediaStates.Ended)
                {
                    PlayStart(true);
                }
                else
                {
                    Task.Run(() => _player.Play());
                }
            }
            else
            {
                Task.Run(() => _player.SetPause(true));
            }
        }


        private void UpdateAudioEnable()
        {
            if (_disposedValue) return;

#if VLC_AUDIOENABLE_TRACK
            if (_audioTracks is not null)
            {
                _audioTracks.IsEnabled = _isAudioEnabled;
            }
#endif
#if VLC_AUDIOENABLE_MUTE
            UpdateMuted();
#endif
        }

        private void UpdateMuted()
        {
            if (_disposedValue) return;

            Task.Run(() =>
            {
                var isMute = _isMuted;
#if VLC_AUDIOENABLE_MUTE
                isMute = isMute || !_isAudioEnabled;
#endif
                _player.Audio.IsMute = isMute;
            });
        }

        private void UpdateRepeat()
        {
            if (_disposedValue) return;

            if (!_isOpened) return;

            var keepPosition = _player.State != MediaStates.Ended;
            PlayStart(keepPosition);
        }

        private bool ShouldPlay()
        {
            return _isEnabled && _isPlaying;
        }

        private void PlayStart(bool keepPosition)
        {
            if (_disposedValue) return;

            if (keepPosition)
            {
                _requestPosition = _player.Position;
            }
            PlayStartContinue();
        }

        private void PlayStart(double position)
        {
            if (_disposedValue) return;

            _requestPosition = (float)position;
            PlayStartContinue();
        }

        private void PlayStartContinue()
        {
            if (_disposedValue) return;

            _player.Playing += Player_SecondPlaying;

            PlayStart();

            void Player_SecondPlaying(object? sender, VlcMediaPlayerPlayingEventArgs e)
            {
                _player.Playing -= Player_SecondPlaying;
                if (_disposedValue) return;
                _audioTracks?.UpdateCurrent();
                _subtitles?.UpdateCurrent();
            }
        }

        private void PlayStart()
        {
            if (_disposedValue) return;

            var options = new List<string>();
            if (_isRepeat)
            {
                options.Add("input-repeat=65535");
            }

            if (_uri is null) return;
            Task.Run(() => _player.Play(_uri, options.ToArray()));
        }

        [Conditional("DEBUG")]
        private void Trace(string message)
        {
            Debug.WriteLine($"VLC: {_player.State}: {message}");
        }
    }
}
