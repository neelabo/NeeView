namespace NeeView
{
    public enum ResetNextBookPageMode
    {
        /// <summary>
        /// リセットなし
        /// </summary>
        None,

        /// <summary>
        /// 先頭ページにリセット
        /// </summary>
        Reset,

        /// <summary>
        /// 連続するように先頭か終端にリセット
        /// </summary>
        Continue,
    }

}
