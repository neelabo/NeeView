using System;

namespace NeeView
{
    [Obsolete]
    public enum ExportImageFileNameMode
    {
        [AliasName]
        Original,

        [AliasName]
        BookPageNumber,

        [AliasName(IsVisible = false)]
        Default,
    }
}
