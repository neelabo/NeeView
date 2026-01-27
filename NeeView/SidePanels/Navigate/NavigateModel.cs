using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{

    /// <summary>
    /// 画像コントロール
    /// </summary>
    public class NavigateModel : BindableBase
    {
        static NavigateModel() => Current = new NavigateModel();
        public static NavigateModel Current { get; }


        private readonly double[] _scaleSnaps = new double[]
        {
            0.01, 0.1, 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 2.0, 4.0, 8.0, 16.0, 32.0
        };


        //private readonly DragTransform _dragTransform;
        //private readonly ContentCanvas _contentCanvas;

        private readonly NavigateTransformControl _dragTransform;


        private readonly MediaControl _mediaControl;


        public NavigateModel()
        {
            _dragTransform = new NavigateTransformControl(PageFrameBoxPresenter.Current);
            _dragTransform.PropertyChanged += NavigateTransformControl_PropertyChanged;

            _mediaControl = new MediaControl();

            //_dragTransform = MainViewComponent.Current.DragTransform;
            //_contentCanvas = MainViewComponent.Current.ContentCanvas;

            MainViewComponent.Current.PageFrameBoxPresenter.ViewContentChanged += PageFrameBoxPresenter_ViewContentChanged;

            Config.Current.View.PropertyChanged += ViewConfig_PropertyChanged;
        }

        private void PageFrameBoxPresenter_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            if (e.Action < ViewContentChangedAction.ContentLoading) return;

            var viewContent = e.ViewContents.FirstOrDefault() as IHasViewContentMediaPlayer;
            var player = viewContent?.Player;
            if (player is not null && !MainViewComponent.Current.PageFrameBoxPresenter.IsMedia)
            {
                _mediaControl.RaiseContentChanged(this, new MediaPlayerChanged(player, false));
            }
            else
            {
                _mediaControl.RaiseContentChanged(this, new MediaPlayerChanged());
            }
        }

        public NavigateTransformControl DragTransform => _dragTransform;

        public MediaControl MediaControl => _mediaControl;


        public double Angle
        {
            get => _dragTransform.Angle;
            set => _dragTransform.SetAngle(value, TimeSpan.Zero);
        }

        public double Scale
        {
            get => _dragTransform.Scale;
            set => _dragTransform.SetScale(value, TimeSpan.Zero);
        }

        public bool IsFlipHorizontal
        {
            get => _dragTransform.IsFlipHorizontal;
            set => _dragTransform.SetFlipHorizontal(value, TimeSpan.Zero);
        }

        public bool IsFlipVertical
        {
            get => _dragTransform.IsFlipVertical;
            set => _dragTransform.SetFlipVertical(value, TimeSpan.Zero);
        }

        public bool IsRotateStretchEnabled
        {
            get => Config.Current.View.IsRotateStretchEnabled;
            set => Config.Current.View.IsRotateStretchEnabled = value;
        }

        public bool IsKeepAngle
        {
            get => Config.Current.View.IsKeepAngle;
            set => Config.Current.View.IsKeepAngle = value;
        }

        public bool IsKeepAngleBooks
        {
            get => Config.Current.View.IsKeepAngleBooks;
            set => Config.Current.View.IsKeepAngleBooks = value;
        }

        public bool IsScaleStretchTracking
        {
            get => Config.Current.View.IsScaleStretchTracking;
            set => Config.Current.View.IsScaleStretchTracking = value;
        }

        public bool IsKeepScale
        {
            get => Config.Current.View.IsKeepScale;
            set => Config.Current.View.IsKeepScale = value;
        }

        public bool IsKeepScaleBooks
        {
            get => Config.Current.View.IsKeepScaleBooks;
            set => Config.Current.View.IsKeepScaleBooks = value;
        }

        public bool IsKeepFlip
        {
            get => Config.Current.View.IsKeepFlip;
            set => Config.Current.View.IsKeepFlip = value;
        }

        public bool IsKeepFlipBooks
        {
            get => Config.Current.View.IsKeepFlipBooks;
            set => Config.Current.View.IsKeepFlipBooks = value;
        }


        private void NavigateTransformControl_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(NavigateTransformControl.Angle):
                    RaisePropertyChanged(nameof(Angle));
                    break;
                case nameof(NavigateTransformControl.Scale):
                    RaisePropertyChanged(nameof(Scale));
                    break;
                case nameof(NavigateTransformControl.IsFlipHorizontal):
                    RaisePropertyChanged(nameof(IsFlipHorizontal));
                    break;
                case nameof(NavigateTransformControl.IsFlipVertical):
                    RaisePropertyChanged(nameof(IsFlipVertical));
                    break;
            }
        }

        private void ViewConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                    RaisePropertyChanged("");
                    break;

                case nameof(ViewConfig.IsKeepAngle):
                    RaisePropertyChanged(nameof(IsKeepAngle));
                    break;
                case nameof(ViewConfig.IsKeepAngleBooks):
                    RaisePropertyChanged(nameof(IsKeepAngleBooks));
                    break;
                case nameof(ViewConfig.IsScaleStretchTracking):
                    RaisePropertyChanged(nameof(IsScaleStretchTracking));
                    break;
                case nameof(ViewConfig.IsKeepScale):
                    RaisePropertyChanged(nameof(IsKeepScale));
                    break;
                case nameof(ViewConfig.IsKeepScaleBooks):
                    RaisePropertyChanged(nameof(IsKeepScaleBooks));
                    break;
                case nameof(ViewConfig.IsKeepFlip):
                    RaisePropertyChanged(nameof(IsKeepFlip));
                    break;
                case nameof(ViewConfig.IsKeepFlipBooks):
                    RaisePropertyChanged(nameof(IsKeepFlipBooks));
                    break;
                case nameof(ViewConfig.IsRotateStretchEnabled):
                    RaisePropertyChanged(nameof(IsRotateStretchEnabled));
                    break;
            }
        }

        public void RotateLeft()
        {
            var angle = MathUtility.NormalizeLoopRange(_dragTransform.Angle - 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            _dragTransform.SetAngle(angle, TimeSpan.Zero);

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateRight()
        {
            var angle = MathUtility.NormalizeLoopRange(_dragTransform.Angle + 90.0, -180.0, 180.0);
            angle = Math.Truncate((angle + 180.0) / 90.0) * 90.0 - 180.0;

            _dragTransform.SetAngle(angle, TimeSpan.Zero);

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void RotateReset()
        {
            _dragTransform.SetAngle(0.0, TimeSpan.Zero);

            if (IsRotateStretchEnabled)
            {
                Stretch();
            }
        }

        public void ScaleDown()
        {
            var scale = _dragTransform.Scale - 0.01;
            var index = _scaleSnaps.FindIndex(e => scale < e);
            if (0 < index)
            {
                scale = _scaleSnaps[index - 1];
            }
            else
            {
                scale = _scaleSnaps.First();
            }

            _dragTransform.SetScale(scale, TimeSpan.Zero);
        }

        public void ScaleUp()
        {
            var scale = _dragTransform.Scale + 0.01;
            var index = _scaleSnaps.FindIndex(e => scale < e);
            if (0 <= index)
            {
                scale = _scaleSnaps[index];
            }
            else
            {
                scale = _scaleSnaps.Last();
            }

            _dragTransform.SetScale(scale, TimeSpan.Zero);
        }

        public void ScaleReset()
        {
            _dragTransform.SetScale(1.0, TimeSpan.Zero);
        }

        public void Stretch()
        {
            _dragTransform.SnapView();
        }
    }


}
