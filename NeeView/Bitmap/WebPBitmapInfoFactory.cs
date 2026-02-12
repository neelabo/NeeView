using System.IO;

namespace NeeView
{
    public class WebPBitmapInfoFactory : BitmapInfoFactory
    {
        public override bool CheckFormat(Stream stream)
        {
            return AnimatedImageChecker.IsWebp(stream);
        }

        public override BitmapInfo Create(Stream stream)
        {
            var info = WebPDecoder.GetInfo(stream);
            return new BitmapInfo(info, stream);
        }
    }
}
