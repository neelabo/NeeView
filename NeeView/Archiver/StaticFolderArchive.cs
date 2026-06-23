using System.Collections.Generic;
using System.Diagnostics;
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


        protected override Task<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            return Task.FromResult(new List<ArchiveEntry>());
        }

        /// <summary>
        /// エントリ仮作成
        /// </summary>
        /// <remarks>
        /// 指定したパスがファイルシステム上に存在すると仮定して発行する。アーカイブ内等の実エントリーにするにはCreateAsync()を使用する。
        /// </remarks>
        public FolderArchiveEntry CreateTempArchiveEntry(string path, ArchiveHint archiveHint)
        {
            return new FolderArchiveEntry(this) { RawEntryName = path, IsTemporary = true, ArchiveHint = archiveHint };
        }

        /// <summary>
        /// エントリ作成
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">パスが存在しない</exception>
        public FolderArchiveEntry CreateArchiveEntry(string path, ArchiveHint archiveHint)
        {
            var fullPath = System.IO.Path.GetFullPath(path);
            var directoryInfo = new DirectoryInfo(fullPath);
            if (FileIO.Exists(directoryInfo))
            {
                return CreateArchiveEntry(directoryInfo, 0, archiveHint);
            }
            var fileInfo = new FileInfo(fullPath);
            if (FileIO.Exists(fileInfo))
            {
                return CreateArchiveEntry(fileInfo, 0, archiveHint);
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

        protected override void OnStartWatch()
        {
            Debug.Assert(false, "Not supported");
        }

        protected override void OnStopWatch()
        {
            Debug.Assert(false, "Not supported");
        }
    }
}
