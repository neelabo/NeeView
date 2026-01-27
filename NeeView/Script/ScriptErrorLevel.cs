namespace NeeView
{
    public enum ScriptErrorLevel
    {
        /// <summary>
        /// 廃止されたメンバーアクセスを情報として通知する。トースト通知は行わない。
        /// スクリプトは続行される。
        /// 廃止されたメンバーは型のデフォルト値を返す。nullを返すこともあり、当然ながらその後の動作は保証されない。
        /// </summary>
        Info = 0,

        /// <summary>
        /// 廃止されたメンバーアクセスを警告として通知する。トースト通知を行う。
        /// スクリプトは続行される。
        /// 廃止されたメンバーは型のデフォルト値を返す。nullを返すこともあり、当然ながらその後の動作は保証されない。
        /// </summary>
        Warning = 1,

        /// <summary>
        /// 廃止されたメンバーアクセスをエラーとして通知する。トースト通知を行う。
        /// スクリプトは停止する。
        /// </summary>
        Error = 2,
    }


    public static class ScriptErrorLevelExtension
    {
        public static bool IsOpenConsole(this ScriptErrorLevel self)
        {
            return ScriptErrorLevel.Warning <= self;
        }

        public static bool IsError(this ScriptErrorLevel self)
        {
            return ScriptErrorLevel.Error <= self;
        }
    }
}
