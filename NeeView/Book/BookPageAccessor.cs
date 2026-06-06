using NeeLaboratory;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ページ範囲に関する処理
    /// </summary>
    public class BookPageAccessor : IBookPageAccessor
    {
        public BookPageAccessor(IReadOnlyList<Page> pages)
        {
            Pages = pages;
        }

        public IReadOnlyList<Page> Pages { get; }

        public Page? First => Pages.FirstOrDefault();
        public Page? Last => Pages.LastOrDefault();

        public int FirstIndex => Pages.Any() ? 0 : -1;
        public int LastIndex => Pages.Any() ? Pages.Count - 1 : -1;

        public PagePosition FirstPosition => Pages.Any() ? PagePosition.Zero : PagePosition.Empty;
        public PagePosition LastPosition => Pages.Any() ? new(Pages.Count - 1, 1) : PagePosition.Empty;

        public PageRange PageRange => new PageRange(FirstPosition, LastPosition);


        public bool ContainsIndex(int index)
        {
            return 0 <= index && index < Pages.Count;
        }

        public int ClampIndex(int index)
        {
            return Pages.Any() ? MathUtility.Clamp(index, FirstIndex, LastIndex) : -1;
        }

        public int NormalizeIndex(int index)
        {
            return Pages.Any() ? MathUtility.NormalizeLoopRange(index, FirstIndex, LastIndex) : -1;
        }

        public int NormalizeCycle(int index)
        {
            return Pages.Any() ? MathUtility.CycleLoopRange(index, FirstIndex, LastIndex) : 0;
        }

        public PagePosition NormalizePosition(PagePosition position)
        {
            return Pages.Any() ? new PagePosition(NormalizeIndex(position.Index), position.Part) : PagePosition.Empty;
        }


        public Page? GetPage(int index, bool normalized = false)
        {
            if (normalized)
            {
                index = NormalizeIndex(index);
            }

            if (ContainsIndex(index))
            {
                return Pages[index];
            }
            else
            {
                return null;
            }
        }

        public PagePosition ValidatePosition(PagePosition position, bool normalized = false)
        {
            if (!Pages.Any())
            {
                return PagePosition.Empty;
            }

            if (!normalized && !ContainsIndex(position.Index))
            {
                return PagePosition.Empty;
            }
            return position;
        }

        public PagePosition ClampPosition(PagePosition position)
        {
            return MathUtility.Clamp(position, FirstPosition, LastPosition);
        }
    }

}
