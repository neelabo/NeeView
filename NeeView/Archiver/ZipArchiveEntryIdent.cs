using System;
using System.IO.Compression;

// TODO: 書庫内書庫 ストリームによる多重展開が可能？

namespace NeeView
{
    /// <summary>
    /// ZipArchiveEntry の識別標
    /// </summary>
    /// <remarks>
    /// 名前の重複があり得るので、サイズと日時で補完する
    /// </remarks>
    /// <param name="FullName">エントリ名</param>
    /// <param name="Length">ファイルサイズ</param>
    /// <param name="LastWriteTime">最終更新日</param>
    public record ZipArchiveEntryIdent(string FullName, long Length, DateTimeOffset LastWriteTime)
    {
        public ZipArchiveEntryIdent(ZipArchiveEntry entry) : this(entry.FullName, entry.Length, entry.LastWriteTime)
        {
        }
    }
}
