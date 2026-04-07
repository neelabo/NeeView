using System.Collections.Generic;


namespace NeeView
{
    /// <summary>
    /// タイトル文字列フォーマット情報
    /// </summary>
    public class TitleFormatSource
    {
        public TitleFormatSource() : this("", [])
        {
        }

        public TitleFormatSource(string format, IEnumerable<TitleStringWordInfo> words)
        {
            Format = format;
            Words = [.. words];
        }

        public string Format { get; }
        public List<TitleStringWordInfo> Words { get; }
    }
}
