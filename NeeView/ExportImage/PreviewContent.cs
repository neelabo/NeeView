using System;
using System.Windows.Media;

namespace NeeView
{
    public class PreviewContent
    {
        public PreviewContent(ImageSource? imageSource, DateTime lastWriteTime, long length, int width, int height)
        {
            ImageSource = imageSource;
            LastWriteTime = lastWriteTime;
            Length = length;
            Width = width;
            Height = height;
        }

        public ImageSource? ImageSource { get; }
        public DateTime LastWriteTime { get; }
        public long Length { get; }
        public int Width { get; }
        public int Height { get; }
    }
}