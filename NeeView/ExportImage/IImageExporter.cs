using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public interface IImageExporter : IDisposable
    {
        ImageExporterContent? CreateView(ImageExporterCreateOptions options);
        Task ExportAsync(string path, bool isOverwrite, int qualityLevel, ImageExporterCreateOptions options, CancellationToken token);
        ImageSource? CreateImageSource(ImageExporterCreateOptions options);
        string CreateFileName();
        bool CanExport();
        public void ThrowIfCannotExport();
        DateTime GetLastWriteTime();
        long GetLength(string path, int qualityLevel, ImageExporterCreateOptions options);
    }

    public class ImageExporterCreateOptions
    {
        public bool HasBackground { get; set; }
        public bool IsOriginalSize { get; set; }
        public bool IsDotKeep { get; set; }
    }
}
