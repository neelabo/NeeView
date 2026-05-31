using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public partial class NavigateTransformControl : ObservableObject, ITransformControl
    {
        private PageFrameBoxPresenter _presenter;
        private PageFrameTransformAccessor? _source;

        public NavigateTransformControl(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;
            _presenter.SelectedRangeChanged += (s, e) => UpdateSource();

            Config.Current.BookSetting.SubscribePropertyChanged(nameof(BookSettingConfig.BaseScale),
                (s, e) => OnPropertyChanged(nameof(BaseScale)));

            UpdateSource();
        }

        public double Scale => _source?.Scale ?? 1.0;

        public double Angle => _source?.Angle ?? 0.0;

        public Point Point => _source?.Point ?? default;

        public bool IsFlipHorizontal => _source?.IsFlipHorizontal ?? false;

        public bool IsFlipVertical => _source?.IsFlipVertical ?? false;

        public double BaseScale => Config.Current.BookSetting.BaseScale;


        private void UpdateSource()
        {
            var source = _presenter.CreateSelectedTransform();
            if (_source == source) return;

            Detach();
            Attach(source);
        }

        private void Attach(PageFrameTransformAccessor? source)
        {
            Debug.Assert(_source is null);
            if (source is null) return;
            _source = source;
            _source.TransformChanged += Source_TransformChanged;
            OnPropertyChanged("");
        }

        private void Detach()
        {
            if (_source is null) return;
            _source.TransformChanged -= Source_TransformChanged;
            _source = null;
        }

        private void Source_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            switch (e.Action)
            {
                case TransformAction.Scale:
                    OnPropertyChanged(nameof(Scale));
                    break;
                case TransformAction.Angle:
                    OnPropertyChanged(nameof(Angle));
                    break;
                case TransformAction.Point:
                    OnPropertyChanged(nameof(Point));
                    break;
                case TransformAction.FlipHorizontal:
                    OnPropertyChanged(nameof(IsFlipHorizontal));
                    break;
                case TransformAction.FlipVertical:
                    OnPropertyChanged(nameof(IsFlipVertical));
                    break;
            }
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _source?.SetPoint(value, span, easeX, easeY);

            AdjustPosition(span);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _source?.SetAngle(value, span);

            AdjustPosition(span);
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            _source?.SetFlipHorizontal(value, span);

            AdjustPosition(span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            _source?.SetFlipVertical(value, span);

            AdjustPosition(span);
        }

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            _source?.SetScale(value, span, trigger);

            AdjustPosition(span);
        }

        public void SetBaseScale(double value)
        {
            Config.Current.BookSetting.BaseScale = value;

            AdjustPosition(TimeSpan.Zero);
        }

        public void SnapView()
        {
            _presenter.Stretch(false);
        }

        /// <summary>
        /// 必要であれば座標を補正する
        /// </summary>
        private void AdjustPosition(TimeSpan span)
        {
            if (Config.Current.Book.IsPanorama) return;
            if (Config.Current.View.MovementConstraint < MovementConstraint.Snap) return;

            var transform = GetDragTransform(false);
            if (transform is null) return;

            var p0 = Point;
            var p1 = transform.GetSnapPoint(p0);
            if (p0 != p1)
            {
                _source?.SetPoint(p1, span);
            }
        }

        private DragTransform? GetDragTransform(bool isPointed)
        {
            var context = _presenter.CreateContentDragTransformContext(isPointed);
            if (context is null) return null;
            return new DragTransform(context);
        }

    }
}
