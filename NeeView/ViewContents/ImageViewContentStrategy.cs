//#define LOCAL_DEBUG

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.Generators;
using NeeView.Threading;

namespace NeeView
{
    [LocalDebug]
    public partial class ImageViewContentStrategy : IDisposable, IViewContentStrategy, IHasImageSource, IHasScalingMode
    {
        private readonly ViewContent _viewContent;
        private ImageContentControl? _imageControl;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;
        private readonly System.Threading.Lock _lock = new();

        public ImageViewContentStrategy(ViewContent viewContent)
        {
            _viewContent = viewContent;
        }


        public ImageSource? ImageSource => _imageControl?.ImageSource;

        public BitmapScalingMode? ScalingMode
        {
            get { return _scalingMode; }
            set
            {
                if (_scalingMode != value)
                {
                    _scalingMode = value;
                    if (_imageControl != null)
                    {
                        _imageControl.ScalingMode = _scalingMode;
                    }
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _imageControl?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void OnSourceChanged()
        {
            if (_disposedValue) return;

            _viewContent.RequestLoadViewSource(CancellationToken.None);
        }

        public FrameworkElement CreateLoadedContent(object data)
        {
            if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);

            var viewData = data as ImageViewData ?? throw new InvalidOperationException();

            LocalDebug.WriteLine($"Create={_viewContent.Page}, {_imageControl is not null}");

            lock (_lock)
            {
                _imageControl?.Dispose();
                _imageControl = null;

                if (viewData.ImageSource is DrawingImage)
                {
                    _imageControl = new CropImageContentControl(_viewContent.Element, viewData.ImageSource, _viewContent.ViewContentSize, _viewContent.BackgroundSource);
                }
                else
                {
                    _imageControl = new BrushImageContentControl(_viewContent.Element, viewData.ImageSource, _viewContent.ViewContentSize, _viewContent.BackgroundSource);
                }
                _imageControl.ScalingMode = ScalingMode;
                return _imageControl;
            }
        }

    }
}
