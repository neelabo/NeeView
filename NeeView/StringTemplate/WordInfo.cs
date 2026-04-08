namespace NeeView.StringTemplate
{
    public class WordInfo<TSource>
    {
        public WordInfo(string placeholder, KeyInfo<TSource>? formatInfo, string suffix, string format)
        {
            Placeholder = placeholder;
            FormatInfo = formatInfo;
            Suffix = suffix;
            Format = format;
        }

        public string Placeholder { get; }
        public KeyInfo<TSource>? FormatInfo { get; }
        public string Suffix { get; }
        public string Format { get; }
    }
}

