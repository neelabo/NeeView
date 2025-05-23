using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// ページ読み込み
    /// </summary>
    public interface IPageLoader : IDisposable
    {
        public ValueTask LoadAsync(PageRange range, int direction, CancellationToken token);
        public void Cancel();
    }
}
