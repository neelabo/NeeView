using NeeView.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class EmptyViewSourceStrategy : IViewSourceStrategy
    {
        public EmptyViewSourceStrategy()
        {
        }

        public Task<DataSource> LoadCoreAsync(PageDataSource data, Size size, CancellationToken token)
        {
            return Task.FromResult(new DataSource(new EmptyViewData(), 0, null));
        }
    }


    public class EmptyViewData
    {
    }
}
