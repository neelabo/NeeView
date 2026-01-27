using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.Susie.Server
{
    /// <summary>
    /// Susie アーカイブエントリ
    /// </summary>
    public class ArchiveEntry
    {
        // 対応するプラグイン
        private readonly SusiePlugin _spi;

        // エントリ情報(RAW)
        private ArchiveFileInfoRaw _info;

        // アーカイブのショートファイル名
        // UNICODE対応のため、ショートファイル名でアクセスします
        public string ArchiveShortFileName { get; private set; }

        // エントリのパス名
        public string Path => _info.path;

        // エントリ名
        public string FileName => _info.filename;

        // ディレクトリ？
        // これに当てはまらないサイズ0のエントリの場合はどちらとも解釈できる
        public bool IsDirectory => string.IsNullOrEmpty(_info.filename) || _info.filename.Last() == '/' || _info.filename.Last() == '\\';

        // 展開後ファイルサイズ
        // 正確でない可能性がある
        public uint FileSize => _info.filesize;

        // タイムスタンプ
        public DateTime TimeStamp => Time_T2DateTime(_info.timestamp);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ArchiveEntry(SusiePlugin spi, string archiveFileName, ArchiveFileInfoRaw info)
        {
            _spi = spi;
            ArchiveShortFileName = archiveFileName;
            _info = info;
        }


        /// メモリに展開
        public byte[] Load()
        {
            return _spi.LoadArchiveEntry(ArchiveShortFileName, _info);
        }

        /// フォルダーに出力。ファイル名は変更しない
        public void ExtractToFolder(string extractFolder)
        {
            _spi.ExtractArchiveEntryToFolder(ArchiveShortFileName, _info, extractFolder);
        }

        /// ファイルに出力
        public void ExtractToFile(string extract)
        {
            using (var ms = new System.IO.MemoryStream(Load(), false))
            using (var stream = new System.IO.FileStream(extract, System.IO.FileMode.Create))
            {
                ms.WriteTo(stream);
            }
            GC.Collect();
        }

        // UNIX時間をDateTimeに変換
        private static DateTime Time_T2DateTime(uint time_t)
        {
            long win32FileTime = 10000000 * (long)time_t + 116444736000000000;
            return DateTime.FromFileTime(win32FileTime);
        }


        public SusieArchiveEntry ToSusieArchiveEntry()
        {
            var entry = new SusieArchiveEntry(this.Path, this.FileName)
            {
                Position = (int)_info.position,
                IsDirectory = this.IsDirectory,
                FileSize = this.FileSize,
                TimeStamp = this.TimeStamp
            };

            return entry;
        }
    }

    /// <summary>
    /// Susie アーカイブエントリ リスト
    /// </summary>
    public class ArchiveEntryCollection : List<ArchiveEntry>
    {
        public SusiePlugin SusiePlugin { get; set; }

        public ArchiveEntryCollection(SusiePlugin spi, string archiveFileName, List<ArchiveFileInfoRaw> entries)
        {
            SusiePlugin = spi;

            string shortPath = NativeMethods.GetShortPathName(archiveFileName);
            foreach (var entry in entries)
            {
                this.Add(new ArchiveEntry(spi, shortPath, entry));
            }
        }
    }
}
