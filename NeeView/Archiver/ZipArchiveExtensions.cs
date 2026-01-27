using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NeeView
{
    public static class ZipArchiveExtensions
    {
        public static List<ZipArchiveEntry> CollectEntries(this System.IO.Compression.ZipArchive archive, string entryPrefix)
        {
            return archive.Entries.Where(e => LoosePath.NormalizeSeparator(e.FullName).StartsWith(entryPrefix, StringComparison.Ordinal)).ToList();
        }

        /// <summary>
        /// ZipArchiveEntryIdent によるエントリ取得
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="ident"></param>
        /// <returns></returns>
        public static ZipArchiveEntry? FindEntry(this System.IO.Compression.ZipArchive archive, ZipArchiveEntryIdent ident)
        {
            var findEntry = archive.GetEntry(ident.FullName);
            if (findEntry is null) return null;

            // 同名エントリの可能性があるのでサイズと日時で検証する。それも同じエントリは存在しうるがそれは未定義の動作とする
            if (findEntry.FullName == ident.FullName && findEntry.Length == ident.Length && findEntry.LastWriteTime == ident.LastWriteTime)
            {
                return findEntry;
            }
            else
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName == ident.FullName && entry.Length == ident.Length && entry.LastWriteTime == ident.LastWriteTime)
                    {
                        return entry;
                    }
                }
                return null;
            }
        }

        public static ZipArchiveEntry? FindEntry(this System.IO.Compression.ZipArchive archive, ArchiveEntry entry)
        {
            if (entry.Instance is not ZipArchiveEntryIdent ident) throw new InvalidOperationException("Entry does not have a ZipArchiveEntryIdent.");
            Debug.Assert(entry.RawEntryName == ident.FullName);

            if (entry.Attributes.HasFlag(ArchiveEntryAttributes.Duplicate))
            {
                return archive.FindEntry(ident);
            }
            else
            {
                return archive.GetEntry(ident.FullName);
            }
        }
    }

    public static class ZipArchiveEntryExtensions
    {
        public static bool IsDirectory(this ZipArchiveEntry entry)
        {
            var last = entry.FullName.Last();
            return (entry.Name == "" && (last == '\\' || last == '/'));
        }

        public static string CreateExportPath(this ZipArchiveEntry entry, string entryPrefix, string exportDirectory)
        {
            Debug.Assert(string.IsNullOrEmpty(entryPrefix) || LoosePath.ValidPath(entry.FullName).StartsWith(entryPrefix, StringComparison.Ordinal));
            return FileIO.CreateUniquePath(LoosePath.Combine(exportDirectory, LoosePath.ValidPath(entry.FullName[entryPrefix.Length..])));
        }

        public static void Export(this ZipArchiveEntry entry, string output, bool overwrite)
        {
            if (IsDirectory(entry))
            {
                //Debug.WriteLine($"CreateDirectory: {output}");
                Directory.CreateDirectory(output);
            }
            else
            {
                var outputDir = System.IO.Path.GetDirectoryName(output) ?? throw new IOException($"Illegal path: {output}");
                if (!string.IsNullOrWhiteSpace(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                //Debug.WriteLine($"Export: {output}");
                entry.ExtractToFile(output, overwrite);
            }
        }

    }
}
