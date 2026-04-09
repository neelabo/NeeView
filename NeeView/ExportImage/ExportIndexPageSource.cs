using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// IExportPageSource + Index
    /// </summary>
    public class ExportIndexPageSource : IExportPageSource
    {
        public ExportIndexPageSource(IExportPageSource source, int index) : this(source.BookAddress, source.Direction, source.Elements, index)
        {
        }

        public ExportIndexPageSource(string bookAddress, int direction, IEnumerable<PageNameElement> elements, int index)
        {
            BookAddress = bookAddress;
            Direction = direction;
            Elements = [.. elements];
            Index = index;
        }

        public string BookAddress { get; }
        public int Direction { get; }
        public List<PageNameElement> Elements { get; }
        public int Index { get; }
    }
}

