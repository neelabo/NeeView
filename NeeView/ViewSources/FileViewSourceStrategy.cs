using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class FileViewSourceStrategy : IViewSourceStrategy
    {
        public FileViewSourceStrategy()
        {
        }

        public async ValueTask<DataSource> LoadCoreAsync(PageDataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not FilePageData pageData) throw new InvalidOperationException(nameof(data.Data));

            var bitmapSource = await AppDispatcher.InvokeAsync(() =>
            {
                var bitmapSourceCollection = FileIconCollection.Current.CreateFileIcon(pageData.Entry.SystemPath, IO.FileIconType.FileType, true, true);
                bitmapSourceCollection.Freeze();
                return bitmapSourceCollection.GetBitmapSource(48.0);
            });

            return new DataSource(new FileViewData(pageData, bitmapSource), 0, null);
        }
    }

}
