using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public class ExportImageService : IDisposable
    {
        private readonly ExportImageSource _source;
        private readonly ExportImageParameter _parameter;
        private readonly IExportImageFileNamePolicy _fileNamePolicy;
        protected IImageExporter _exporter;


        public ExportImageService(ExportImageSource source, IExportImageParameter parameter)
        {
            _source = source;

            // parameter はここで複製される
            _parameter = new ExportImageParameter(parameter);

            _fileNamePolicy = new DefaultExportImageFileNamePolicy(_parameter);

            _exporter = CreateExporter(parameter.Mode, _source);
        }


        public ExportImageSource Source => _source;

        public ExportImageParameter Parameter => _parameter;

        public string? ExportFolder => _parameter.ExportFolder;

        public ExportImageMode Mode => _parameter.Mode;


        private static IImageExporter CreateExporter(ExportImageMode mode, ExportImageSource source)
        {
            return mode switch
            {
                ExportImageMode.Original
                    => new OriginalImageExporter(source),
                ExportImageMode.View
                    => new ViewImageExporter(source),
                _
                    => throw new InvalidOperationException(),
            };
        }

        public ImageSource? CreateImageSource()
        {
            return _exporter.CreateImageSource(_parameter);
        }

        public async ValueTask ExportStreamAsync(Stream stream, CancellationToken token)
        {
            await _exporter.ExportAsync(stream, true, _parameter.FileFormat, _parameter, token);
        }

        public async ValueTask ExportAsync(string path, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));
            path = System.IO.Path.GetFullPath(path); // 念のため絶対パス化を保証しておく

            await _exporter.ExportAsync(path, isOverwrite, _parameter, token);
        }

        public string CreateFileName()
        {
            return _fileNamePolicy.CreateFileName(_source, 1);
        }

        public bool CanExport()
        {
            return _exporter.CanExport();
        }

        public void ThrowIfCannotExport()
        {
            _exporter.ThrowIfCannotExport();
        }

        public DateTime GetLastWriteTime()
        {
            return _exporter.GetLastWriteTime();
        }

        public long GetLength(string path)
        {
            return _exporter.GetLength(path, _parameter);
        }

        public void Dispose()
        {
            _exporter.Dispose();
        }
    }

}