using System;

namespace NeeView
{
    /// <summary>
    /// 背景の種類
    /// </summary>
    public enum BackgroundType
    {
        [AliasName]
        Black,

        [AliasName]
        White,

        [AliasName]
        Auto,

        [AliasName]
        Check,

        [AliasName]
        CheckDark,

        [AliasName]
        Custom
    };

    public static class BackgroundStyleExceptions
    {
        public static BackgroundType GetToggle(this BackgroundType mode)
        {
            return (BackgroundType)(((int)mode + 1) % Enum.GetNames(typeof(BackgroundType)).Length);
        }
    }
}
