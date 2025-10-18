using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using AnimatedImage.Wpf;

namespace NeeView
{

    [NotifyPropertyChanged]
    public partial class AnimatedMediaPlayer : IOpenableMediaPlayer, IDisposable
    {
        private readonly Image _image;
        private ImageAnimationController? _player;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private bool _isEnabled = true;
        private bool _isRepeat = true;
        private bool _isPlaying;



        public AnimatedMediaPlayer()
        {
            _image = new Image()
            {
                Stretch = Stretch.Fill,
            };
            ImageBehavior.SetAutoStart(_image, false);
            ImageBehavior.AddAnimationLoadedHandler(_image, Image_Loaded);
            ImageBehavior.AddAnimationCompletedHandler(_image, Image_Completed);
        }



        public event PropertyChangedEventHandler? PropertyChanged;


        public event EventHandler? MediaEnded;
        public event EventHandler? MediaPlayed;

#pragma warning disable CS0067
        public event EventHandler? MediaOpened;
        public event EventHandler<ExceptionEventArgs>? MediaFailed;
#pragma warning restore CS0067


        public Image Image => _image;

        public bool HasAudio => false;

        public bool HasVideo => true;

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
            get { return false; }
            set { }
        }

        public bool IsMuted
        {
            get { return false; }
            set { }
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
                SetProperty(ref _isPlaying, value);
            }
        }

        public bool ScrubbingEnabled => true;


        public Duration Duration
        {
            get { return GetDuration(); }
        }


        public double Position
        {
            get { return GetPosition(); }
            set
            {
                if (_disposedValue) return;
                SetPosition(value);
            }
        }

        public double Volume
        {
            get { return 0.0; }
            set { }
        }

        public bool RateEnabled => false;

        public double Rate
        {
            get { return 1.0; }
            set { }
        }

        public bool CanControlTracks => false;
        public TrackCollection? AudioTracks => null;
        public TrackCollection? Subtitles => null;

        public int PositionChangeInterval => 200;
        public int ScrubbingInterval => 0;

        public bool IsDisposed => _disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private Duration GetDuration()
        {
            return _player?.Duration ?? Duration.Automatic;
        }

        private double GetPosition()
        {
            if (_player is null) return 0.0;
            return _player.CurrentFrame >= 0
                ? (double)_player.CurrentFrame / (_player.FrameCount - 1)
                : 1.0;
        }

        private void SetPosition(double position)
        {
            if (_player is null) return;
            var frame = (int)(_player.FrameCount * position);
            frame = MathUtility.Clamp(frame, 0, _player.FrameCount - 1);
            _player.GotoFrame(frame);
        }

        public void Open(MediaSource mediaSource, TimeSpan delay)
        {
            Debug.Assert(_player is null);
            if (_disposedValue) return;

            var stream = mediaSource.OpenStreamAsync(CancellationToken.None).GetAwaiter().GetResult(); // TODO: async
            _disposables.Add(stream);

            var bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.StreamSource = stream;
            bitmapSource.EndInit();

            ImageBehavior.SetAnimatedSource(_image, bitmapSource);

            IsPlaying = true;
        }


        public void Close()
        {
            if (_disposedValue) return;
            if (_player is null) return;

            _player.CurrentFrameChanged -= Player_CurrentFrameChanged;
            _player.Dispose();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (_disposedValue) return;

            _player = ImageBehavior.GetAnimationController(_image);
            if (_player is null) return;

            _player.CurrentFrameChanged += Player_CurrentFrameChanged;
            _player.Play();

            UpdatePlayed();
            UpdateRepeat();

            MediaPlayed?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged("");
        }

        private void Image_Completed(object sender, RoutedEventArgs e)
        {
            if (_disposedValue) return;

            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void Player_CurrentFrameChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(Position));
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


        private void UpdatePlayed()
        {
            if (_disposedValue) return;
            if (_player is null) return;

            var isPlay = _isEnabled && _isPlaying;

            if (isPlay)
            {
                _player.Play();
            }
            else
            {
                _player.Pause();
            }
        }

        private void UpdateRepeat()
        {
            if (_disposedValue) return;
            if (_player is null) return;

            var frame = _player.CurrentFrame;
            if (_isRepeat)
            {
                ImageBehavior.SetRepeatBehavior(_image, RepeatBehavior.Forever);
            }
            else
            {
                ImageBehavior.SetRepeatBehavior(_image, new RepeatBehavior(1));
            }
            if (IsValidFrame(frame))
            {
                _player.GotoFrame(frame);
            }
        }

        private bool IsValidFrame(int frame)
        {
            if (_player is null) return false;
            if (frame < 0) return false;
            if (frame >= _player.FrameCount) return false;
            return true;
        }
    }
}
