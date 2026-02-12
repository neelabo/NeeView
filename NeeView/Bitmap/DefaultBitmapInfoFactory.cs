using System.IO;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class DefaultBitmapInfoFactory : BitmapInfoFactory
    {
        public override bool CheckFormat(Stream stream)
        {
            return true;
        }

        public override BitmapInfo Create(Stream stream)
        {
            var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
            return new BitmapInfo(bitmapFrame, stream);
        }
    }
}
