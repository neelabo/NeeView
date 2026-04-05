using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public interface IExportPageSource
    {
        string BookAddress { get; }

        int Direction { get; }

        List<PageNameElement> Elements { get; }

        //[Obsolete("Use Elements instead.")]
        //List<Page> Pages { get; }
    }


    public record ExportPageSource(string BookAddress, int Direction, List<PageNameElement> Elements) : IExportPageSource
    {
        public ExportPageSource(Page page) : this(page.BookPath, 1, [new PageNameElement(page)])
        {
        }
    }

}
