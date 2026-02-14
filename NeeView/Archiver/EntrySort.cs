using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// エントリ群のソート
    /// TODO: bookと共通化
    /// </summary>
    public static class EntrySort
    {
        // TODO: 入力された entries を変更しないようにする
        /// <summary>
        /// ソート実行
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="sortMode"></param>
        /// <returns></returns>
        public static List<ArchiveEntry> SortEntries(List<ArchiveEntry> entries, PageSortMode sortMode)
        {
            if (entries is null) throw new();
            if (entries.Count <= 0) return entries;

            switch (sortMode)
            {
                case PageSortMode.FileName:
                    entries.Sort((a, b) => CompareFileNameOrder(a, b, NaturalSort.Comparer));
                    break;
                case PageSortMode.FileNameDescending:
                    entries.Sort((a, b) => CompareFileNameOrder(b, a, NaturalSort.Comparer));
                    break;
                case PageSortMode.FileType:
                    entries.Sort((a, b) => CompareFileTypeOrder(a, b, NaturalSort.Comparer));
                    break;
                case PageSortMode.FileTypeDescending:
                    entries.Sort((a, b) => CompareFileTypeOrder(b, a, NaturalSort.Comparer));
                    break;
                case PageSortMode.TimeStamp:
                    entries.Sort((a, b) => CompareDateTimeOrder(a, b, NaturalSort.Comparer));
                    break;
                case PageSortMode.TimeStampDescending:
                    entries.Sort((a, b) => CompareDateTimeOrder(b, a, NaturalSort.Comparer));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    entries = entries.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return entries;
        }

        // ファイル名, 日付, ID の順で比較
        private static int CompareFileNameOrder(ArchiveEntry e1, ArchiveEntry e2, IComparer<string> comparer)
        {
            if (e1.EntryName != e2.EntryName)
                return CompareFileName(e1.EntryName, e2.EntryName, comparer);
            else if (e1.LastWriteTime != e2.LastWriteTime)
                return CompareDateTime(e1.LastWriteTime, e2.LastWriteTime);
            else
                return e1.Id - e2.Id;
        }

        // 種類, ファイル名, ID の順で比較
        private static int CompareFileTypeOrder(ArchiveEntry e1, ArchiveEntry e2, IComparer<string> comparer)
        {
            if (e1.Extension != e2.Extension)
                return CompareFileType(e1.Extension, e2.Extension);
            else if (e1.EntryName != e2.EntryName)
                return CompareFileName(e1.EntryName, e2.EntryName, comparer);
            else
                return e1.Id - e2.Id;
        }

        // 日付, ファイル名, ID の順で比較
        private static int CompareDateTimeOrder(ArchiveEntry e1, ArchiveEntry e2, IComparer<string> comparer)
        {
            if (e1.LastWriteTime != e2.LastWriteTime)
                return CompareDateTime(e1.LastWriteTime, e2.LastWriteTime);
            else if (e1.EntryName != e2.EntryName)
                return CompareFileName(e1.EntryName, e2.EntryName, comparer);
            else
                return e1.Id - e2.Id;
        }

        // ファイル名比較. ディレクトリを優先する
        private static int CompareFileName(string s1, string s2, IComparer<string> comparer)
        {
            string d1 = LoosePath.GetDirectoryName(s1);
            string d2 = LoosePath.GetDirectoryName(s2);

            if (d1 == d2)
                return comparer.Compare(s1, s2);
            else
                return comparer.Compare(d1, d2);
        }

        // 拡張子比較
        private static int CompareFileType(string s1, string s2)
        {
            return string.Compare(s1, s2, StringComparison.Ordinal);
        }

        // 日付比較
        private static int CompareDateTime(DateTime t1, DateTime t2)
        {
            return (t1.Ticks - t2.Ticks < 0) ? -1 : 1;
        }
    }

}
