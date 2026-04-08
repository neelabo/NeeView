using System.Collections.Generic;

namespace NeeView.StringTemplate
{
    public class StringFormat<TSource>
    {
        public StringFormat() : this("", [])
        {
        }

        public StringFormat(string format, IEnumerable<WordInfo<TSource>> words)
        {
            Format = format;
            Words = [.. words];
        }

        public string Format { get; }
        public List<WordInfo<TSource>> Words { get; }
    }
}

