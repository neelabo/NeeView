using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class NavigateThumbnailViewModel : BindableBase
    {
        private readonly PageFrameBoxPresenter _presenter;
        private bool _isEnabled;
        private bool _isVisible;
        private double _rate;
        private double _thumbnailWidth;
        private double _thumbnailHeight = 256.0;
        private Brush _mainViewVisualBrush;
        private StreamGeometry _viewboxGeometry;
        private Size _canvasSize;


        public NavigateThumbnailViewModel(MainViewComponent mainViewComponent)
        {
            _presenter = mainViewComponent.PageFrameBoxPresenter;

            _mainViewVisualBrush = Brushes.Transparent;

            InitializeThumbnail();
            InitializeViewbox();
        }


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    UpdateThumbnail();
                    UpdateVisibility();
                }
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set { SetProperty(ref _isVisible, value); }
        }

        public double ThumbnailWidth
        {
            get { return _thumbnailWidth; }
            set { SetProperty(ref _thumbnailWidth, value); }
        }

        public double ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set { SetProperty(ref _thumbnailHeight, value); }
        }

        public Brush MainViewVisualBrush
        {
            get { return _mainViewVisualBrush; }
            set { SetProperty(ref _mainViewVisualBrush, value); }
        }

        public StreamGeometry ViewboxGeometry
        {
            get { return _viewboxGeometry; }
            set { SetProperty(ref _viewboxGeometry, value); }
        }


        private bool _isStaticFrame;
        public bool IsStaticFrame
        {
            get { return _isStaticFrame; }
            set { SetProperty(ref _isStaticFrame, value); }
        }



        private void InitializeThumbnail()
        {
            _presenter.PageFrameBoxChanged +=
                (s, e) => UpdateThumbnail();

            _presenter.SelectedContentSizeChanged +=
                (s, e) => UpdateThumbnail();

            _presenter.ViewContentChanged +=
                (s, e) => UpdateThumbnail();
        }

        public void SetCanvasSize(Size newSize)
        {
            _canvasSize = newSize;
            UpdateThumbnail();
        }

        private void UpdateThumbnail()
        {
            if (!_isEnabled) return;

            var pageFrameContent = _presenter.GetSelectedPageFrameContent();
            var sourceWidth = pageFrameContent?.GetRawContentRect().Width ?? 0.0;
            var sourceHeight = pageFrameContent?.GetRawContentRect().Height ?? 0.0;

            IsStaticFrame = pageFrameContent?.IsStaticFrame ?? true;

            if (sourceWidth <= 0.0 || sourceHeight <= 0.0 || !IsStaticFrame)
            {
                this.ThumbnailWidth = 0.0;
                this.ThumbnailHeight = 0.0;
                _rate = 0.0;

                MainViewVisualBrush = Brushes.Transparent;
            }
            else
            {
                var sourceSize = new Size(sourceWidth, sourceHeight);
                var limitSize = new Size(Math.Max(_canvasSize.Width - 1.0, 0.0), Math.Max(_canvasSize.Height - 1.0, 0.0));
                var size = sourceSize.Limit(limitSize);
                this.ThumbnailWidth = size.Width;
                this.ThumbnailHeight = size.Height;
                _rate = size.Width / sourceWidth;

                MainViewVisualBrush = new VisualBrush()
                {
                    Stretch = Stretch.Uniform,
                    Visual = _presenter.GetSelectedPageFrameContent()?.ViewElement, // _mainViewComponent.MainView.PageContents,
                };

            }

            UpdateViewbox();
        }


        [MemberNotNull(nameof(_viewboxGeometry))]
        private void InitializeViewbox()
        {
            var points = new List<Point>()
            {
                new Point(-0.5, -0.5),
                new Point(0.5, -0.5),
                new Point(0.5, 0.5),
                new Point(-0.5, 0.5)
            };

            _viewboxGeometry = new StreamGeometry();
            _viewboxGeometry.FillRule = FillRule.Nonzero;
            using (StreamGeometryContext context = _viewboxGeometry.Open())
            {
                context.BeginFigure(points[0], false, true);
                context.PolyLineTo(new List<Point> { points[1], points[2], points[3] }, true, false);
            }

            _presenter.TransformChanged +=
                (s, e) => UpdateViewbox();

            _presenter.SelectedContentSizeChanged +=
                (s, e) => UpdateViewbox();

            //_presenter.ViewContentChanged +=
            //    (s, e) => UpdateViewbox();

            _presenter.ViewSizeChanged +=
                (s, e) => UpdateViewbox();
        }

        private void UpdateViewbox()
        {
            if (!_isEnabled) return;

            var transformGroup = new TransformGroup();

            // 表示エリアの大きさに変換
            var viewWidth = _presenter.ViewWidth;
            var viewHeight = _presenter.ViewHeight;
            transformGroup.Children.Add(new ScaleTransform(viewWidth, viewHeight));

            //NVDebug.WriteInfo("Thumb.AreaSize", $"{viewWidth:f0},{viewHeight:f0}");

            // コンテンツ座標系の逆変換
            var mainViewTransform = _presenter.GetSelectedPageFrameContent()?.CalcTransform;
            var inverse = mainViewTransform?.Inverse;
            if (inverse is Transform inverseTransform)
            {
                transformGroup.Children.Add(inverseTransform);
            }
            else
            {
                // NOTE: 拡大することで範囲外にする
                transformGroup.Children.Add(new ScaleTransform(2.0, 2.0));
            }

            // キャンバス座標系に変換
            transformGroup.Children.Add(new ScaleTransform(_rate, _rate));
            transformGroup.Children.Add(new TranslateTransform(_canvasSize.Width * 0.5, _canvasSize.Height * 0.5));

            //NVDebug.WriteInfo("Thumb.CanvasScale", $"rate={_rate:f2}, size={_canvasSize:f0}");

            _viewboxGeometry.Transform = transformGroup;

            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            this.IsVisible = _isEnabled && _rate >= 0.0 && _presenter.GetSelectedPageFrameContent() != null;
        }

        public void LookAt(Point point)
        {
            if (_rate <= 0.0) return;

            var x = (this.ThumbnailWidth * 0.5 - point.X) / _rate;
            var y = (this.ThumbnailHeight * 0.5 - point.Y) / _rate;

            var transformContext = _presenter.CreateContentDragTransformContext(false);
            if (transformContext is null) return;

            var baseScale = 1.0;
            var frameAngle = 0.0;
            var pageFrameContent = transformContext.GetPageFrameContent();
            if (pageFrameContent is not null)
            {
                baseScale = pageFrameContent.BaseScaleTransform.Scale;
                frameAngle = pageFrameContent.PageFrame.Angle;
            }

            LookAt(transformContext.Transform, baseScale, frameAngle, new Point(x, y));
        }

        private void LookAt(ITransformControl transform, double scale, double angle, Point point)
        {
            var transformGroup = new TransformGroup();
            var scaleX = scale * transform.Scale * (transform.IsFlipHorizontal ? -1.0 : 1.0);
            var scaleY = scale * transform.Scale * (transform.IsFlipVertical ? -1.0 : 1.0);
            transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
            transformGroup.Children.Add(new RotateTransform(transform.Angle));

            transformGroup.Children.Add(new RotateTransform(angle));

            var pos = transformGroup.Transform(point);
            transform.SetPoint(pos, TimeSpan.Zero);
        }
    }
}
