using System.IO;

namespace NeeView
{
    public abstract class BitmapInfoFactory
    {
        public abstract bool CheckFormat(Stream stream);
        public abstract BitmapInfo Create(Stream stream);
    }
}
