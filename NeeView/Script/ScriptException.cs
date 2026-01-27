using System;

namespace NeeView
{
    /// <summary>
    /// スクリプトの実行時例外。
    /// Obsoleteなプロパティの呼び出し等、仕様上の例外をCLRからエンジンに投げるときに使用する。
    /// </summary>
    public class ScriptException : Exception
    {
        public ScriptNotice ScriptNotice { get; set; }


        public ScriptException(ScriptNotice notice, Exception innerException) : base(notice.Message, innerException)
        {
            this.ScriptNotice = notice;
        }


        public override string Message
        {
            get
            {
                return ScriptNotice?.ToString() ?? base.Message;
            }
        }
    }
}
