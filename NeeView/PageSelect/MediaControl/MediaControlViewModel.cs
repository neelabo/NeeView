using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class MediaControlViewModel : BindableBase
    {
        private readonly MediaControl _model;
        private readonly MouseWheelDelta _mouseWheelDelta = new();
        private MediaPlayerOperator? _operator;
        private DisposableCollection _operatorEventDisposables = new();
        private bool _isMoreMenuEnabled;


        public MediaControlViewModel(MediaControl model)
        {
            _model = model;
            _model.Changed += Model_Changed;
            UpdateOperator(_model.LastChangedArgs);

            PlayCommand = new RelayCommand(PlayCommand_Executed);
            RepeatCommand = new RelayCommand(RepeatCommand_Executed);
            MuteCommand = new RelayCommand(MuteCommand_Executed);

            MoreMenuDescription = new MediaPlayerMoreMenuDescription(this);
        }


        public MediaPlayerOperator? Operator
        {
            get { return _operator; }
            set
            {
                if (_operator != value)
                {
                    AttachOperator(value);
                    RaisePropertyChanged();
                    IsMoreMenuEnabled = _operator?.CanControlTracks == true || _operator?.RateEnabled == true;
                }
            }
        }

        public bool IsMoreMenuEnabled
        {
            get { return _isMoreMenuEnabled; }
            set { SetProperty(ref _isMoreMenuEnabled, value); }
        }

        public bool IsPlaying => _operator?.IsPlaying ?? false;


        public RelayCommand PlayCommand { get; }
        public RelayCommand RepeatCommand { get; }
        public RelayCommand MuteCommand { get; }


        private void AttachOperator(MediaPlayerOperator? op)
        {
            DetachOperator();
            if (op is null) return;

            _operator = op;

            _operatorEventDisposables = new();

            _operatorEventDisposables.Add(_operator.SubscribePropertyChanged(nameof(_operator.IsPlaying),
                (s, e) => RaisePropertyChanged(nameof(IsPlaying))));
        }

        private void DetachOperator()
        {
            if (_operator is null) return;

            _operatorEventDisposables.Dispose();
            _operatorEventDisposables.Clear();
            _operator = null;
        }

        private void PlayCommand_Executed()
        {
            if (Operator is null) return;
            Operator.TogglePlay();
        }

        private void RepeatCommand_Executed()
        {
            if (Operator is null) return;
            Operator.IsRepeat = !Operator.IsRepeat;
        }

        private void MuteCommand_Executed()
        {
            if (Operator is null) return;
            Operator.IsMuted = !Operator.IsMuted;
        }

        private void Model_Changed(object? sender, MediaPlayerChanged e)
        {
            UpdateOperator(e);
        }

        private void UpdateOperator(MediaPlayerChanged e)
        {
            if (e.IsValid)
            {
                var mediaPlayer = e.MediaPlayer ?? throw new InvalidOperationException();

                if (Operator?.Player != mediaPlayer)
                {
                    Operator?.Dispose();
                    Operator = new MediaPlayerOperator(mediaPlayer);
                    Operator.MediaEnded += Operator_MediaEnded;
                    Operator.Attach();
                }
            }
            else
            {
                Operator?.Dispose();
                Operator = null;
            }

            RaisePropertyChanged("");

            // TODO: 特殊な処理になっているので整備が必要
            if (e.IsMainMediaPlayer)
            {
                MediaPlayerOperator.BookMediaOperator = Operator;
            }
            else
            {
                MediaPlayerOperator.PageMediaOperator = Operator;
            }
        }

        private void Operator_MediaEnded(object? sender, System.EventArgs e)
        {
            PageFrameBoxPresenter.Current.View?.RaisePageTerminatedEvent(this, 1, true);
        }

        public void SetScrubbing(bool isScrubbing)
        {
            if (_operator == null)
            {
                return;
            }

            _operator.IsScrubbing = isScrubbing;
        }

        public void ToggleTimeFormat()
        {
            if (_operator == null)
            {
                return;
            }

            _operator.IsTimeLeftDisplay = !_operator.IsTimeLeftDisplay;
        }

        public void MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            if (turn == 0) return;

            for (int i = 0; i < Math.Abs(turn); ++i)
            {
                if (turn < 0)
                {
                    BookOperation.Current.Control.MoveNext(this);
                }
                else
                {
                    BookOperation.Current.Control.MovePrev(this);
                }
            }
        }

        internal void MouseWheelVolume(object? sender, MouseWheelEventArgs e)
        {
            if (Operator is null) return;

            var delta = (double)e.Delta / 6000.0;
            Operator.AddVolume(delta);
        }

        internal bool KeyVolume(Key key)
        {
            if (Operator is null) return false;

            switch (key)
            {
                case Key.Up:
                case Key.Right:
                    Operator.AddVolume(+0.01);
                    return true;

                case Key.Down:
                case Key.Left:
                    Operator.AddVolume(-0.01);
                    return true;

                default:
                    return false;
            }
        }


        #region MoreMenu

        public MediaPlayerMoreMenuDescription MoreMenuDescription { get; }

        public class MediaPlayerMoreMenuDescription : MoreMenuDescription
        {
            private readonly MediaControlViewModel _vm;
            private readonly ContextMenu _menu = new();
            private readonly MatchingToBooleanConverter<TrackItem> _matchingTrackItemConverter = new();
            private readonly MatchingToBooleanConverter<double> _matchingDoubleConverter = new();

            public MediaPlayerMoreMenuDescription(MediaControlViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Update(ContextMenu menu)
            {
                return Create();
            }

            public override ContextMenu Create()
            {
                var menu = _menu;
                _menu.Items.Clear();

                if (_vm.Operator is null) return menu;

                if (_vm.Operator.CanControlTracks)
                {
                    // audio tracks
                    var audios = _vm.Operator.AudioTracks;
                    if (audios is not null)
                    {
                        foreach (var track in audios.Tracks)
                        {
                            menu.Items.Add(CreateTrackMenuItem(track, audios));
                        }
                    }
                    else
                    {
                        menu.Items.Add(new MenuItem() { Header = Properties.TextResources.GetString("MediaControl.MoreMenu.NoAudio"), IsEnabled = false });
                    }

                    menu.Items.Add(new Separator());

                    // subtitle tracks
                    var subtitles = _vm.Operator.SubtitleTracks;
                    if (subtitles is not null)
                    {
                        foreach (var track in subtitles.Tracks)
                        {
                            menu.Items.Add(CreateTrackMenuItem(track, subtitles));
                        }
                    }
                    else
                    {
                        menu.Items.Add(new MenuItem() { Header = Properties.TextResources.GetString("MediaControl.MoreMenu.NoSubtitle"), IsEnabled = false });
                    }

                    menu.Items.Add(new Separator());
                }

                // speed rates
                if (_vm.Operator.RateEnabled)
                {
                    var parent = new MenuItem() { Header = Properties.TextResources.GetString("MediaControl.Speed"), InputGestureText = MediaRateTools.GetDisplayString(_vm.Operator.Rate, false) };
                    menu.Items.Add(parent);
                    foreach (var rate in MediaRateTools.Rates)
                    {
                        parent.Items.Add(CreateRateMenuItem(rate, _vm.Operator));
                    }
                }

                // trim separator
                if (menu.Items.Count > 0 && menu.Items[menu.Items.Count - 1] is Separator)
                {
                    menu.Items.RemoveAt(menu.Items.Count - 1);
                }

                return menu;
            }

            private MenuItem CreateTrackMenuItem(TrackItem track, TrackCollection tracks)
            {
                var menuItem = new MenuItem()
                {
                    Header = track.Name,
                };
                menuItem.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(tracks.Selected))
                {
                    Source = tracks,
                    Converter = _matchingTrackItemConverter,
                    ConverterParameter = track,
                });

                menuItem.Click += (s, e) => { tracks.Selected = track; };

                return menuItem;
            }

            private MenuItem CreateRateMenuItem(double rate, MediaPlayerOperator player)
            {
                var menuItem = new MenuItem()
                {
                    Header = MediaRateTools.GetDisplayString(rate, true),
                    IsChecked = Math.Abs(rate - player.Rate) < 0.01
                };

                menuItem.Click += (s, e) => player.Rate = rate;
                return menuItem;
            }

            #endregion MoreMenu

        }
    }


    public class MatchingToBooleanConverter<T> : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T v1 && parameter is T v2)
            {
                return EqualityComparer<T>.Default.Equals(v1, v2);
            }
            return false;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
