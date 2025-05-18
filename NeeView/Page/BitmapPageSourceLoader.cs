using NeeView.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        private readonly DefaultBitmapPageSourceLoader _default = new();
        private readonly SusieBitmapPageSourceLoader _susie = new();


        public BitmapPageSourceLoader()
        {
        }

        public async ValueTask<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, bool createSource, CancellationToken token)
        {
            BitmapPageSource? imageData = null;
            foreach (var loader in CreateOrderList())
            {
                imageData = await loader.LoadAsync(streamSource, createPictureInfo, createSource, token);
                if (imageData.DataState == DataState.Loaded)
                {
                    return imageData;
                }
            }
            return imageData ?? BitmapPageSource.CreateError("Failed to load image");
        }


        private List<IBitmapPageSourceLoader> CreateOrderList()
        {
            if (!Config.Current.Susie.IsEnabled)
            {
                return new List<IBitmapPageSourceLoader>() { _default };
            }
            else if (Config.Current.Susie.IsFirstOrderSusieImage)
            {
                return new List<IBitmapPageSourceLoader>() { _susie, _default };
            }
            else
            {
                return new List<IBitmapPageSourceLoader>() { _default, _susie };
            }
        }
    }
}
