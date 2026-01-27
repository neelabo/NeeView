// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    /// <summary>
    /// ページが終わったときのアクション
    /// </summary>
    public enum PageEndAction
    {
        [AliasName]
        None,

        [AliasName]
        NextBook,

        [AliasName]
        Loop,

        [AliasName]
        SeamlessLoop,

        [AliasName]
        Dialog,
    }
}

