using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページ読み込み
    /// </summary>
    public interface IPageLoader : IDisposable
    {
        public Task LoadAsync(PageRange range, int direction, CancellationToken token);
        public void Cancel();
    }
}
