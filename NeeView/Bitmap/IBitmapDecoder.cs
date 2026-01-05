using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public interface IBitmapDecoder
    {
        string Name { get; }
        bool CheckFormat(Stream stream);
        BitmapSource Create(Stream stream, BitmapInfo? info, Size size, CancellationToken token);
        void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, CancellationToken token);
    }
}