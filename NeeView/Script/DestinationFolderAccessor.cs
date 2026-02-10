using NeeLaboratory.Linq;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public record class DestinationFolderAccessor
    {
        private readonly DestinationFolder _folder;

        public DestinationFolderAccessor(DestinationFolder folder)
        {
            _folder = folder;
        }

        internal DestinationFolder Source => _folder;

        [WordNodeMember]
        public string Name
        {
            get { return _folder.Name; }
            set { AppDispatcher.Invoke(() => _folder.Name = value); }
        }

        [WordNodeMember]
        public string Path
        {
            get { return _folder.Path; }
            set { AppDispatcher.Invoke(() => _folder.Path = value); }
        }


        [WordNodeMember]
        public void CopyPage(PageAccessor page)
        {
            CopyPage([page]);
        }

        [WordNodeMember]
        public void CopyPage(PageAccessor[] pages)
        {
            var entries = pages.Select(e => e.Source.ArchiveEntry).ToList();
            var async = _folder.TryCopyAsync(entries, CancellationToken.None);
        }

        [WordNodeMember]
        public void Copy(string path)
        {
            Copy([path]);
        }

        [WordNodeMember]
        public void Copy(string[] paths)
        {
            _ = _folder.TryCopyAsync(paths, CancellationToken.None);
        }

        [WordNodeMember]
        public void MovePage(PageAccessor page)
        {
            MovePage([page]);
        }

        [WordNodeMember]
        public void MovePage(PageAccessor[] pages)
        {
            var paths = pages
                .Select(e => e.Source)
                .Where(e => e.ArchiveEntry.IsFileSystem)
                .Select(e => e.EntryFullName)
                .WhereNotNull()
                .ToArray();

            Move(paths);
        }

        [WordNodeMember]
        public void Move(string path)
        {
            Move([path]);
        }

        [WordNodeMember]
        public void Move(string[] paths)
        {
            _ = _folder.TryMoveAsync(paths, CancellationToken.None);
        }
    }
}
