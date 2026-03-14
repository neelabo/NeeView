using System.Collections.Generic;
using System.Linq;

namespace NeeView.IO
{
    public class FolderOperatonResult
    {
        public FolderOperatonResult(IEnumerable<FolderOperatonItemResult> items)
        {
            Items = items.ToList();
        }

        public List<FolderOperatonItemResult> Items { get; } = new();
    }

    public record FolderOperatonItemResult(string Source, string Destination);
}

