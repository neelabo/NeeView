namespace NeeView
{
    public class BitmapPageSource : PageSource
    {
        public BitmapPageSource(BitmapPageData? data, string? errorMessage, PictureInfo? pictureInfo, IBitmapPageSourceLoader? imageDataLoader)
            : base(data, errorMessage, pictureInfo)
        {
            ImageDataLoader = imageDataLoader;
        }


        public BitmapPageData? BitmapPageData => (BitmapPageData?)Data;
        public override long DataSize => BitmapPageData?.StreamSource.GetMemorySize() ?? 0;
        public IBitmapPageSourceLoader? ImageDataLoader { get; }


        public static BitmapPageSource Create(BitmapPageData? data, PictureInfo? pictureInfo, IBitmapPageSourceLoader imageDataLoader)
        {
            return new BitmapPageSource(data, null, pictureInfo, imageDataLoader);
        }

        public new static BitmapPageSource CreateError(string errorMessage)
        {
            return new BitmapPageSource(null, errorMessage, null, null);
        }
    }



}
