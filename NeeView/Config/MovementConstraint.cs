namespace NeeView
{
    /// <summary>
    /// 移動制限
    /// </summary>
    public enum MovementConstraint
    {
        /// <summary>
        /// なし
        /// </summary>
        None,

        /// <summary>
        /// 画面範囲内
        /// </summary>
        LimitToScreen,

        /// <summary>
        /// リサイズまでロック
        /// </summary>
        LockUntilResized,

        /// <summary>
        /// スナップ
        /// </summary>
        Snap,
    }


    public static class TranslateLockModeExtensions
    {
        extension(MovementConstraint mode)
        {
            public bool IsLimited => mode >= MovementConstraint.LimitToScreen;

            public bool IsLockStart => mode >= MovementConstraint.LockUntilResized;
        }
    }
}
