using NeeView.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookTableOfContents
    {
        private readonly BookSource _source;
        private ContentsPageNode? _contentsTree;


        public BookTableOfContents(BookSource source)
        {
            _source = source;
        }


        public async Task<ContentsPageNode> GetContentsTreeAsync(CancellationToken token)
        {
            // 目次をキャッシュする
            if (_contentsTree is not null)
            {
                return _contentsTree;
            }

            if (_source.ArchiveEntryCollection?.Archive is PdfPdfiumArchive archive)
            {
                _contentsTree = await CreatePdfContentsTree(archive, token);
            }
            else
            {
                _contentsTree = CreateDefaultContentsTree();
            }

            return _contentsTree;
        }

        public async Task<ContentsPageNode> CreatePdfContentsTree(PdfPdfiumArchive archive, CancellationToken token)
        {
            var bookmarks = await archive.CreateContentsTreeAsync(token);

            var pages = GetPageCollection();
            var root = new ContentsPageNode() { Page = pages.FirstOrDefault(), Title = TextResources.GetString("Word.TableOfContents") };
            root.Children = CreatePdfContentsList(bookmarks);
            return root;
        }

        public List<ContentsPageNode>? CreatePdfContentsList(List<ContentsArchiveEntryNode>? bookmarks)
        {
            if (bookmarks is null || bookmarks.Count == 0)
            {
                return null;
            }

            var children = new List<ContentsPageNode>();

            foreach (var bookmark in bookmarks)
            {
                var page = _source.Pages.GetPageWithTarget(bookmark.ArchiveEntry.SystemPath);
                var node = new ContentsPageNode() { Name = bookmark.Title, Page = page };
                node.Children = CreatePdfContentsList(bookmark.Children);
                children.Add(node);
            }

            return children;
        }

        public ContentsPageNode CreateDefaultContentsTree()
        {
            var pages = GetPageCollection();
            var groups = pages.GroupBy(e => LoosePath.GetDirectoryName(e.EntryName));

            var root = new ContentsPageNode() { Page = pages.FirstOrDefault(), Title = TextResources.GetString("Word.TableOfContents") };

            foreach (var group in groups)
            {
                root.Add(group.First(), group.Key);
            }

            return root;
        }

        private IEnumerable<Page> GetPageCollection()
        {
            return _source.Pages.SortMode == PageSortMode.FileName ? _source.Pages : BookPageSort.Sort(_source.Pages, PageSortMode.FileName, 0, CancellationToken.None).Pages;
        }

    }
}
