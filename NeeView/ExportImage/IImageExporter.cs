using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public interface IImageExporter : IDisposable
    {
        ImageExporterContent? CreateView(IImageExporterOptions options);
        Task ExportAsync(Stream stream, bool decrypt, BitmapImageFormat format, IImageExporterOptions options, CancellationToken token);
        Task ExportAsync(string path, bool isOverwrite, IImageExporterOptions options, CancellationToken token);
        ImageSource? CreateImageSource(IImageExporterOptions options);
        bool CanExport();
        public void ThrowIfCannotExport();
        DateTime GetLastWriteTime();
        long GetLength(string path, IImageExporterOptions options);
    }


    public interface IImageExporterOptions
    {
        public bool HasBackground { get; }
        public bool IsOriginalSize { get; }
        public bool IsDotKeep { get; }
        public int QualityLevel { get; }
    }
}
