using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace NeeView
{
    public class ExportImagePreview : BindableBase, IDisposable
    {
        private ExportImageParameter _parameter;
        private ExportImageSource _source;
        private IImageExporter _exporter;

        private FrameworkElement? _preview;
        private double _previewWidth = double.NaN;
        private double _previewHeight = double.NaN;
        private string _imageFormatNote = "";
        private bool _disposedValue;
        private DisposableCollection _disposables = new();

        public ExportImagePreview(ExportImageParameter parameter, ExportImageSource source)
        {
            _parameter = parameter;
            _source = source;

            _disposables.Add(_parameter.SubscribePropertyChanged(nameof(_parameter.Mode),
                (s, e) => { UpdateExporter(); UpdatePreview(); }));

            _disposables.Add(_parameter.SubscribePropertyChanged(nameof(_parameter.HasBackground),
                (s, e) => UpdatePreview()));

            _disposables.Add(_parameter.SubscribePropertyChanged(nameof(_parameter.IsOriginalSize),
                (s, e) => UpdatePreview()));

            _disposables.Add(_parameter.SubscribePropertyChanged(nameof(_parameter.IsDotKeep),
                (s, e) => UpdatePreview()));

            UpdateExporter();
            UpdatePreview();
        }


        public FrameworkElement? Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }

        public double PreviewWidth
        {
            get { return _previewWidth; }
            set { SetProperty(ref _previewWidth, value); }
        }

        public double PreviewHeight
        {
            get { return _previewHeight; }
            set { SetProperty(ref _previewHeight, value); }
        }

        public string ImageFormatNote
        {
            get { return _imageFormatNote; }
            set { SetProperty(ref _imageFormatNote, value); }
        }

        public IImageExporter Exporter
        {
            get { return _exporter; }

            [MemberNotNull(nameof(_exporter))]
            set
            {
                if (_exporter != value)
                {
                    _exporter?.Dispose();
                    _exporter = value;
                    RaisePropertyChanged();
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    _exporter?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [MemberNotNull(nameof(_exporter))]
        private void UpdateExporter()
        {
            Exporter = ImageExporterFactory.CreateExporter(_source, _parameter.Mode);
        }

        private void UpdatePreview()
        {
            try
            {
                var content = _exporter.CreateView(_parameter);
                if (content is null) throw new InvalidOperationException();
                Preview = content.View;
                PreviewWidth = content.Size.IsEmpty ? double.NaN : content.Size.Width;
                PreviewHeight = content.Size.IsEmpty ? double.NaN : content.Size.Height;
                ImageFormatNote = content.Size.IsEmpty ? "" : $"{(int)content.Size.Width} x {(int)content.Size.Height}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Preview = null;
                PreviewWidth = double.NaN;
                PreviewHeight = double.NaN;
                ImageFormatNote = "Error.";
            }
        }
    }
}