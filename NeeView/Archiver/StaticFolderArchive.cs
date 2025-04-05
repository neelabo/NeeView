using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    /// <summary>
    /// ファイル単体用アーカイブ
    /// </summary>
    public class StaticFolderArchive : FolderArchive
    {
        public static StaticFolderArchive Default { get; } = new StaticFolderArchive();


        public StaticFolderArchive() : base("", null, ArchiveHint.None)
        {
        }


        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            return await Task.FromResult(new List<ArchiveEntry>());
        }

        /// <summary>
        /// エントリ作成
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="isForce">パスが存在しなくてもエントリを作成する</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">パスが存在しない</exception>
        public FolderArchiveEntry CreateArchiveEntry(string path, ArchiveHint archiveHint, bool isForce = false)
        {
            var fullPath = System.IO.Path.GetFullPath(path);
            var directoryInfo = new DirectoryInfo(fullPath);
            if (directoryInfo.Exists)
            {
                return CreateArchiveEntry(directoryInfo, 0, archiveHint);
            }
            var fileInfo = new FileInfo(fullPath);
            if (fileInfo.Exists)
            {
                return CreateArchiveEntry(fileInfo, 0, archiveHint);
            }
            else if (isForce)
            {
                return new FolderArchiveEntry(this) { RawEntryName = path, IsTemporary = true, ArchiveHint = archiveHint };
            }

            throw new FileNotFoundException("File not found.", fullPath);
        }

        private FolderArchiveEntry CreateArchiveEntry(FileInfo fileInfo, int id, ArchiveHint archiveHint)
        {
            var entry = CreateArchiveEntry(fileInfo, id);
            entry.ArchiveHint = archiveHint;
            return entry;
        }

        private FolderArchiveEntry CreateArchiveEntry(DirectoryInfo directoryInfo, int id, ArchiveHint archiveHint)
        {
            var entry = CreateArchiveEntry(directoryInfo, id);
            entry.ArchiveHint = archiveHint;
            return entry;
        }
    }
}
