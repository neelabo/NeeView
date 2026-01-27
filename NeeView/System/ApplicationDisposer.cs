using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// アプリ終了時のデータ保存前に非同期動作しているサービスを停止させる仕組み。
    /// 事前にDisposableなサービスを登録しておく。
    /// </summary>
    public class ApplicationDisposer : DisposableCollection
    {
        static ApplicationDisposer() => Current = new ApplicationDisposer();
        public static ApplicationDisposer Current { get; }
    }
}
