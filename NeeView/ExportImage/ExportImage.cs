using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 設定 + ImageExporter
    /// </summary>
    /// <remarks>
    /// ExportImageWindow 用
    /// </remarks>
    public class ExportImage : ExportImageParameter, IDisposable
    {
        private readonly ExportImageSource _source;
        private IImageExporter _exporter;
        private bool _disposedValue;

        public ExportImage(ExportImageSource source)
        {
            _source = source;
            UpdateExporter();
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
        public void UpdateExporter()
        {
            Exporter = ImageExporterFactory.CreateExporter(_source, this.Mode);
        }

        public string CreateFileName(ExportImageFileNameMode fileNameMode, BitmapImageFormat format)
        {
            var fileNamePolicy = new DefaultExportImageFileNamePolicy();
            return fileNamePolicy.CreateFileName(_source, Mode, fileNameMode, format);
        }

        public async ValueTask ExportAsync(string path, bool isOverwrite, CancellationToken token)
        {
            await _exporter.ExportAsync(path, isOverwrite, this, token);
        }
    }
}