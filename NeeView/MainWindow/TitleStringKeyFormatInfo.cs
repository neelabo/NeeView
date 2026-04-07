using System;


namespace NeeView
{
    /// <summary>
    /// タイトル文字列キー情報
    /// </summary>
    public class TitleStringKeyFormatInfo
    {
        public TitleStringKeyFormatInfo(Func<string, TitleSource, string, string> formatter, TitleStringChangedAction updateReason)
        {
            Formatter = formatter;
            ChangedAction = updateReason;
        }

        public Func<string, TitleSource, string, string> Formatter { get; set; }
        public TitleStringChangedAction ChangedAction { get; set; }
    }
}
