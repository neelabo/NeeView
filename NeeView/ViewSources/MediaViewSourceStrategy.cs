using NeeView.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class MediaViewSourceStrategy : IViewSourceStrategy
    {
        public MediaViewSourceStrategy()
        {
        }

        public async ValueTask<DataSource> LoadCoreAsync(PageDataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not MediaPageData pageData) throw new InvalidOperationException(nameof(data.Data));

            var viewData = new MediaViewData(new MediaSource(pageData.Path, pageData.AudioInfo), null);
            await Task.CompletedTask;
            return new DataSource(viewData, 0, null);
        }
    }

}
