using NeeLaboratory.Linq;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            var async = CopyAsync(pages, CancellationToken.None);
        }

        [WordNodeMember]
        public void Copy(string path)
        {
            Copy([path]);
        }

        [WordNodeMember]
        public void Copy(string[] paths)
        {
            var async = CopyAsync(paths, CancellationToken.None);
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
            var async = _folder.MoveAsyncNoExceptions(paths, CancellationToken.None);
        }


        private static Page? GetPage(string path)
        {
            return BookHub.Current.GetCurrentBook()?.Pages.GetPageWithEntryFullName(path);
        }

        private async ValueTask CopyAsync(PageAccessor[] pages, CancellationToken token)
        {
            try
            {
                var entries = pages.Select(e => e.Source.ArchiveEntry);
                var items = await RealizeArchiveEntry(entries, token);
                await _folder.CopyAsyncNoExceptions(items, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                // NOTE: Script からのエラーは Toast で通知する
                ToastService.Current.Show(new Toast(ex.Message, TextResources.GetString("Bookshelf.CopyToFolderFailed"), ToastIcon.Error));
            }
        }

        private async ValueTask CopyAsync(string[] paths, CancellationToken token)
        {
            try
            {
                var entries = await PathToArchiveEntry(paths, token);
                var items = await RealizeArchiveEntry(entries, token);
                await _folder.CopyAsyncNoExceptions(items, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, TextResources.GetString("Bookshelf.CopyToFolderFailed"), ToastIcon.Error));
            }
        }

        private static async ValueTask<List<ArchiveEntry>> PathToArchiveEntry(IEnumerable<string> paths, CancellationToken token)
        {
            var entries = new List<ArchiveEntry>();
            foreach (var path in paths)
            {
                entries.Add(await ArchiveEntryUtility.CreateAsync(path, ArchiveHint.None, true, token));
            }
            return entries;
        }

        private static async ValueTask<List<string>> RealizeArchiveEntry(IEnumerable<ArchiveEntry> entries, CancellationToken token)
        {
            var archivePolicy = Config.Current.System.ArchiveCopyPolicy.LimitedRealization();
            return await ArchiveEntryUtility.RealizeArchiveEntry(entries, archivePolicy, token);
        }
    }
}
