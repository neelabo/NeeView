using System;
using System.Runtime;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// 開発用コマンド
    /// </summary>
    [WordNodeMember(IsEnabled = false)]
    public class DebugAccessor
    {
        /// <summary>
        /// ガベージコレクト
        /// </summary>
        [WordNodeMember]
        public void GarbageCollect()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 全てのページを表示する。ベンチマーク用。
        /// 結果はログファイルに出力される。
        /// </summary>
        [WordNodeMember]
        public void ScrollAll()
        {
            AppDispatcher.Invoke(() => PageFrameBoxPresenter.Current.ScrollThroughAllPages(CancellationToken.None));
        }
    }
}
