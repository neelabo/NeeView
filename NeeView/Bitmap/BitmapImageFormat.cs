namespace NeeView
{
    /// <summary>
    /// Bitmap出力画像フォーマット
    /// </summary>
    public enum BitmapImageFormat
    {
        [AliasName]
        Jpeg,

        [AliasName]
        Png,
    }

    public static class BitmapImageFormatExtensions
    {
        public static string GetExtension(this BitmapImageFormat format)
        {
            return format switch
            {
                BitmapImageFormat.Jpeg => ".jpg",
                BitmapImageFormat.Png => ".png",
                _ => throw new System.ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
    }
}
