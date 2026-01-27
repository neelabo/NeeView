using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public static class ResourceBitmapUtility
    {
        /// <summary>
        /// アイコンから画像を取得
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static BitmapFrame GetIconBitmapFrame(string path, int size)
        {
            var uri = new Uri("pack://application:,,," + path);
            var decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);

            var frame = decoder.Frames.SingleOrDefault(f => f.Width == size);
            if (frame == default(BitmapFrame))
            {
                frame = decoder.Frames.OrderBy(f => f.Width).First();
            }

            return frame;
        }
    }
}
