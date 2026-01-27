using System.Collections.Generic;

namespace NeeView
{
    public interface IBookPageAccessor
    {
        IReadOnlyList<Page> Pages { get; }

        int FirstIndex { get; }
        int LastIndex { get; }
        PagePosition FirstPosition { get; }
        PagePosition LastPosition { get; }
        PageRange PageRange { get; }
        Page? First { get; }
        Page? Last { get; }

        bool ContainsIndex(int index);
        int ClampIndex(int index);
        int NormalizeIndex(int index);
        PagePosition ClampPosition(PagePosition position);
        PagePosition NormalizePosition(PagePosition position);
        PagePosition ValidatePosition(PagePosition position, bool normalized = false);
        Page? GetPage(int index, bool normalized = false);
    }
}
