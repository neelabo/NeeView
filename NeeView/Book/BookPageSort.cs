using NeeView.Linq.BookExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public static class BookPageSort
    {
        public static BookPageSortResult Sort(IEnumerable<Page> pages, PageSortMode sortMode, int sortSeed, CancellationToken token)
        {
            return Sort(pages, sortMode, sortSeed, Config.Current.Book.FolderSortOrder, token);
        }

        public static BookPageSortResult Sort(IEnumerable<Page> pages, PageSortMode sortMode, int sortSeed, FolderSortOrder folderSortOrder, CancellationToken token)
        {
            if (!pages.Any())
            {
                return new BookPageSortResult(pages.ToList(), sortMode, sortSeed, folderSortOrder);
            }

            switch (sortMode)
            {
                case PageSortMode.FileName:
                    pages = pages.OrderByFileName(folderSortOrder, token);
                    break;
                case PageSortMode.FileNameDescending:
                    pages = pages.OrderByFileNameDescending(folderSortOrder, token);
                    break;
                case PageSortMode.FileType:
                    pages = pages.OrderByFolderOrder(folderSortOrder).ThenBy(e => e, PageComparer.FileType).ThenByFolderOrder(folderSortOrder).ThenByFileName(folderSortOrder, token);
                    break;
                case PageSortMode.FileTypeDescending:
                    pages = pages.OrderByFolderOrder(folderSortOrder).ThenByDescending(e => e, PageComparer.FileType).ThenByFolderOrderDescending(folderSortOrder).ThenByFileName(folderSortOrder, token);
                    break;
                case PageSortMode.TimeStamp:
                    pages = pages.OrderByFolderOrder(folderSortOrder).ThenBy(e => e.ArchiveEntry.LastWriteTime).ThenByFileName(folderSortOrder, token);
                    break;
                case PageSortMode.TimeStampDescending:
                    pages = pages.OrderByFolderOrder(folderSortOrder).ThenByDescending(e => e.ArchiveEntry.LastWriteTime).ThenByFileName(folderSortOrder, token);
                    break;
                case PageSortMode.Size:
                    pages = pages.OrderByFolderOrder(folderSortOrder).ThenBy(e => e.ArchiveEntry.Length).ThenByFileName(folderSortOrder, token);
                    break;
                case PageSortMode.SizeDescending:
                    pages = pages.OrderByFolderOrder(folderSortOrder).ThenByDescending(e => e.ArchiveEntry.Length).ThenByFileName(folderSortOrder, token);
                    break;
                case PageSortMode.Random:
                    sortSeed = sortSeed != 0 ? sortSeed : Random.Shared.Next(1, int.MaxValue);
                    var random = new Random(sortSeed);
                    pages = pages.OrderByFolderOrder(folderSortOrder).ThenBy(e => random.Next());
                    break;
                case PageSortMode.Entry:
                    pages = pages.OrderBy(e => e.EntryIndex);
                    break;
                case PageSortMode.EntryDescending:
                    pages = pages.OrderByDescending(e => e.EntryIndex);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new BookPageSortResult(pages.ToList(), sortMode, sortSeed, folderSortOrder);
        }
    }


    public class BookPageSortResult : IEnumerable<Page>
    {
        public BookPageSortResult(List<Page> pages, PageSortMode sortMode, int sortSeed, FolderSortOrder folderSortOrder)
        {
            SortMode = sortMode;
            SortSeed = sortSeed;
            FolderSortOrder = folderSortOrder;
            Pages = pages;
        }

        public List<Page> Pages { get; }
        public PageSortMode SortMode { get; }
        public int SortSeed { get; }
        public FolderSortOrder FolderSortOrder { get; }

        public IEnumerator<Page> GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Pages).GetEnumerator();
        }
    }
}


namespace NeeView.Linq.BookExtensions
{
    public static class IOrderedEnumerableExtensions
    {
        public static IOrderedEnumerable<Page> OrderByFileName(this IEnumerable<Page> items, FolderSortOrder folderSortOrder, CancellationToken token)
        {
            return items.OrderBy(e => e, PageComparer.DirectoryName(token))
                    .ThenByFolderOrder(folderSortOrder)
                    .ThenBy(e => e, PageComparer.FileName(token));
        }

        public static IOrderedEnumerable<Page> OrderByFileNameDescending(this IEnumerable<Page> items, FolderSortOrder folderSortOrder, CancellationToken token)
        {
            return items.OrderByDescending(e => e, PageComparer.DirectoryName(token))
                    .ThenByFolderOrder(folderSortOrder)
                    .ThenByDescending(e => e, PageComparer.FileName(token));
        }

        public static IOrderedEnumerable<Page> ThenByFileName(this IOrderedEnumerable<Page> items, FolderSortOrder folderSortOrder, CancellationToken token)
        {
            return items.ThenBy(e => e, PageComparer.DirectoryName(token))
                    .ThenByFolderOrder(folderSortOrder)
                    .ThenBy(e => e, PageComparer.FileName(token));
        }

        public static IOrderedEnumerable<Page> ThenByFileNameDescending(this IOrderedEnumerable<Page> items, FolderSortOrder folderSortOrder, CancellationToken token)
        {
            return items.ThenByDescending(e => e, PageComparer.DirectoryName(token))
                    .ThenByFolderOrder(folderSortOrder)
                    .ThenByDescending(e => e, PageComparer.FileName(token));
        }

        public static IOrderedEnumerable<Page> OrderByFolderOrder(this IEnumerable<Page> items, FolderSortOrder folderSortOrder)
        {
            return folderSortOrder switch
            {
                FolderSortOrder.First
                    => items.OrderBy(e => e.PageType),
                FolderSortOrder.Last
                    => items.OrderByDescending(e => e.PageType),
                _
                    => items.OrderBy(e => 0),
            };
        }

        public static IOrderedEnumerable<Page> OrderByFolderOrderDescending(this IEnumerable<Page> items, FolderSortOrder folderSortOrder)
        {
            return folderSortOrder switch
            {
                FolderSortOrder.First
                    => items.OrderByDescending(e => e.PageType),
                FolderSortOrder.Last
                    => items.OrderBy(e => e.PageType),
                _
                    => items.OrderBy(e => 0),
            };
        }

        public static IOrderedEnumerable<Page> ThenByFolderOrder(this IOrderedEnumerable<Page> items, FolderSortOrder folderSortOrder)
        {
            return folderSortOrder switch
            {
                FolderSortOrder.First
                    => items.ThenBy(e => e.PageType),
                FolderSortOrder.Last
                    => items.ThenByDescending(e => e.PageType),
                _
                    => items,
            };
        }

        public static IOrderedEnumerable<Page> ThenByFolderOrderDescending(this IOrderedEnumerable<Page> items, FolderSortOrder folderSortOrder)
        {
            return folderSortOrder switch
            {
                FolderSortOrder.First
                    => items.ThenByDescending(e => e.PageType),
                FolderSortOrder.Last
                    => items.ThenBy(e => e.PageType),
                _
                    => items,
            };
        }

    }
}
