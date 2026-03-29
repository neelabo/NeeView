using System.Collections.Generic;

namespace NeeView
{
    public interface IExportPageSource
    {
        string BookAddress { get; }
        List<Page> Pages { get; }
    }


    public record ExportPageSource(string BookAddress, List<Page> Pages) : IExportPageSource
    {
        public ExportPageSource(Page page) : this(page.BookPath, [page])
        {
        }
    }

}
