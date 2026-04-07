namespace NeeView
{
    /// <summary>
    /// タイトル文字列プレースホルダー情報
    /// </summary>
    public class TitleStringWordInfo
    {
        public TitleStringWordInfo(string placeholder, TitleStringKeyFormatInfo? formatInfo, string suffix, string format)
        {
            Placeholder = placeholder;
            FormatInfo = formatInfo;
            Suffix = suffix;
            Format = format;
        }

        public string Placeholder { get; }
        public TitleStringKeyFormatInfo? FormatInfo { get; }
        public string Suffix { get; }
        public string Format { get; }
    }
}
