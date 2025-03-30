using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public abstract class PdfArchive : Archive
    {
        protected PdfArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
        }

        public abstract Size GetSourceSize(ArchiveEntry entry);
        public abstract System.Drawing.Image CreateBitmap(ArchiveEntry entry, Size size);
        public abstract BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size);
        public abstract byte[] CreateBitmapData(ArchiveEntry entry, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality);
    }
}
