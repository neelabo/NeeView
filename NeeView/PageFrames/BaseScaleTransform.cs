using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Windows.Media;

namespace NeeView.PageFrames
{
    public partial class BaseScaleTransform : IScaleControl, IDisposable
    {
        private double _scale = 1.0;
        private readonly PageFrameContext _context;
        private readonly ViewConfig _viewConfig;
        private readonly ScaleTransform _scaleTransform;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        public BaseScaleTransform(PageFrameContext context)
        {
            _context = context;
            _viewConfig = context.ViewConfig;
            _scaleTransform = new ScaleTransform();

            _disposables.Add(_viewConfig.SubscribePropertyChanged(nameof(ViewConfig.IsBaseScaleEnabled),
                (s, e) => UpdateTransform()));
            _disposables.Add(_context.SubscribePropertyChanged(nameof(PageFrameContext.BaseScale),
                (s, e) => UpdateTransform()));

            UpdateTransform();
        }


        [Subscribable]
        public event EventHandler? ScaleChanged;


        public double Scale => _scale;

        public ScaleTransform ScaleTransform => _scaleTransform;


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

        public void SetScale(double value, TimeSpan span)
        {
            throw new NotImplementedException();
            //_context.BaseScale = value;
        }

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger)
        {
        }

        private void UpdateTransform()
        {
            var scale = _viewConfig.IsBaseScaleEnabled ? _context.BaseScale : 1.0;
            _scaleTransform.ScaleX = scale;
            _scaleTransform.ScaleY = scale;

            if (_scale != scale)
            {
                _scale = scale;
                ScaleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}


