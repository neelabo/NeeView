using NeeLaboratory.IO.Search;
using NeeLaboratory.Linq;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public class SearchBookmarkFolderCollection : BookmarkFolderCollection
    {
        private readonly string _searchKeyword;
        private readonly bool _includeSubdirectories;
        private readonly Searcher _searcher;

        public SearchBookmarkFolderCollection(QueryPath path, bool isOverlayEnabled, bool includeSubdirectories) : base(path, isOverlayEnabled)
        {
            _searchKeyword = Place.Search ?? throw new ArgumentException("Search keywords are required");
            _includeSubdirectories = includeSubdirectories;

            var searchContext = new SearchContext()
                .AddProfile(new DateSearchProfile())
                .AddProfile(new SizeSearchProfile())
                .AddProfile(new BookSearchProfile());
            _searcher = new Searcher(searchContext);
        }

        protected override List<FolderItem> CreateFolderItemCollection(TreeListNode<IBookmarkEntry> root, CancellationToken token)
        {
            var items = CreateFolderItemCollectionRaw(root);

            items = _searcher.Search(_searchKeyword, items, token).Cast<FolderItem>().ToList();

            foreach (var item in items)
            {
                if (item.Source is not TreeListNode<IBookmarkEntry> node) continue;
                var path = string.Join("\\", node.Hierarchy.SkipLast(1).Select(e => e.Value.Name));
                var query = new QueryPath(path, QueryScheme.Bookmark);
                item.TargetPlace = query.FullPath;
            }

            return items;
        }

        private List<FolderItem> CreateFolderItemCollectionRaw(TreeListNode<IBookmarkEntry> root)
        {
            IEnumerable<TreeListNode<IBookmarkEntry>> collection = _includeSubdirectories ? root.WalkChildren() : root;

            var items = collection
                .Select(e => CreateFolderItem(e))
                .WhereNotNull()
                .ToList();

            return items;
        }
    }
}
