using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeeView
{
    public static class ZipArchiveEntryHelper
    {
        private static bool _initialized;
        private static bool _enabled;
        private static FieldInfo? _generalPurposeBitFlagField;
        private static FieldInfo? _storedEntryNameBytesField;
        private static FieldInfo? _storedEntryNameField;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <remarks>
        /// 修理に必要なプライベートフィールドのリフレクション取得
        /// </remarks>
        private static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                var type = typeof(ZipArchiveEntry);
                _generalPurposeBitFlagField = type.GetField("_generalPurposeBitFlag", BindingFlags.NonPublic | BindingFlags.Instance);
                _storedEntryNameBytesField = type.GetField("_storedEntryNameBytes", BindingFlags.NonPublic | BindingFlags.Instance);
                _storedEntryNameField = type.GetField("_storedEntryName", BindingFlags.NonPublic | BindingFlags.Instance);

                _enabled = _generalPurposeBitFlagField is not null
                    && _storedEntryNameBytesField is not null && _storedEntryNameBytesField.FieldType == typeof(byte[])
                    && _storedEntryNameField is not null && _storedEntryNameField.FieldType == typeof(string);
            }
            catch
            {
                _enabled = false;
            }
        }

        /// <summary>
        /// エントリ名を UTF-8 フラグを反映したものに修正する
        /// </summary>
        /// <remarks>
        /// .NET8 は UTF8 フラグが使用されていない不具合がある。この処理はこの問題に対応されるまでの応急処置です。<br/>
        /// <see href="https://github.com/dotnet/runtime/issues/92283"/>
        /// </remarks>
        /// <param name="entry">修正するエントリ</param>
        public static void RepairEntryName(ZipArchiveEntry entry)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (!_enabled) return;

            // has UTF-8 flag ?
            if (_generalPurposeBitFlagField is null) return;
            try
            {
#pragma warning disable CS8605 // null の可能性がある値をボックス化解除しています。
                // NOTE: BitFlagValues にキャストしたいけどアクセスできないため ushort に静的キャスト
                var generalPurposeBitFlag = (ushort)_generalPurposeBitFlagField.GetValue(entry);
#pragma warning restore CS8605 // null の可能性がある値をボックス化解除しています。
                if ((generalPurposeBitFlag & (1 << 11)) == 0) return;
            }
            catch
            {
                // キャスト例外が発生したときはライブラリの構造が変化したと考えられるため、この機能自体を無効化する    
                _enabled = false;
                return;
            }

            // convert to UTF-8
            if (_storedEntryNameBytesField is null) return;
            if (_storedEntryNameBytesField.GetValue(entry) is not byte[] storedEntryNameBytes) return;
            var entryName = Encoding.UTF8.GetString(storedEntryNameBytes);

            // set correct entry name
            if (_storedEntryNameField is null) return;
            _storedEntryNameField.SetValue(entry, entryName);
        }

        /// <summary>
        /// .NET8 の不具合の修正を適用する
        /// </summary>
        public static ZipArchiveEntry Hotfix(this ZipArchiveEntry entry)
        {
            RepairEntryName(entry);
            return entry;
        }

        /// <summary>
        /// .NET8 の不具合の修正を適用する
        /// </summary>
        public static System.IO.Compression.ZipArchive Hotfix(this System.IO.Compression.ZipArchive archive)
        {
            foreach (var e in archive.Entries)
            {
                RepairEntryName(e);
            }
            return archive;
        }
    }


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
