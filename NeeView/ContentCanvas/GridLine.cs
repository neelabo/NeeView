using NeeLaboratory.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class GridLine : ContentControl, IDisposable
    {
        private readonly ImageGridConfig _imageGrid;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private readonly ImageGridTarget _target;

        public GridLine(ImageGridTarget target)
        {
            _target = target;
            _imageGrid = Config.Current.ImageGrid;

            this.Focusable = false;

            _disposables.Add(_imageGrid.SubscribePropertyChanged((s, e) => Update()));
            _disposables.Add(this.SubscribeSizeChanged((s, e) => Update()));

            Update();
        }


        public IDisposable SubscribeSizeChanged(SizeChangedEventHandler handler)
        {
            SizeChanged += handler;
            return new AnonymousDisposable(() => SizeChanged -= handler);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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

        private bool IsActive()
        {
            return _imageGrid.IsEnabled && _imageGrid.Target == _target;
        }

        private void Update()
        {
            if (_disposedValue) return;

            this.Visibility = IsActive() ? Visibility.Visible : Visibility.Collapsed;
            this.Content = CreatePath();

            // センタリング配置
            Canvas.SetLeft(this, -ActualWidth * 0.5);
            Canvas.SetTop(this, -ActualHeight * 0.5);
        }

        private UIElement? CreatePath()
        {
            var width = ActualWidth;
            var height = ActualHeight;

            if (!IsActive() || width <= 0.0 || height <= 0.0) return null;

            double cellX = _imageGrid.DivX > 0 ? width / _imageGrid.DivX : width;
            double cellY = _imageGrid.DivY > 0 ? height / _imageGrid.DivY : height;

            if (_imageGrid.IsSquare)
            {
                if (cellX < cellY)
                {
                    cellX = cellY;
                }
                else
                {
                    cellY = cellX;
                }
            }

            var canvas = new Canvas();
            canvas.Width = width;
            canvas.Height = height;

            var stroke = new SolidColorBrush(_imageGrid.Color);

            canvas.Children.Add(CreatePath(new Point(0, 0), new Point(0, height), stroke));
            canvas.Children.Add(CreatePath(new Point(width, 0), new Point(width, height), stroke));
            canvas.Children.Add(CreatePath(new Point(0, 0), new Point(width, 0), stroke));
            canvas.Children.Add(CreatePath(new Point(0, height), new Point(width, height), stroke));

            for (double i = cellX; i < width - 1; i += cellX)
            {
                canvas.Children.Add(CreatePath(new Point(i, 0), new Point(i, height), stroke));
            }

            for (double i = cellY; i < height - 1; i += cellY)
            {
                canvas.Children.Add(CreatePath(new Point(0, i), new Point(width, i), stroke));
            }

            return canvas;
        }

        private static Path CreatePath(Point startPoint, Point endPoint, Brush stroke)
        {
            var geometry = new LineGeometry(startPoint, endPoint);
            geometry.Freeze();

            return new Path()
            {
                Data = geometry,
                Stroke = stroke,
                StrokeThickness = 1,
            };
        }

    }

}
