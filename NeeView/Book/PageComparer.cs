using System;
using System.Collections.Generic;
using System.Threading;

namespace NeeView
{
    public static class PageComparer
    {
        public static IComparer<Page> FileType { get; } = new ComparerFileType();

        public static IComparer<Page> FileName(CancellationToken token) => new ComparerFileName(token);

        /// <summary>
        /// ファイル名ソート用比較クラス
        /// </summary>
        public class ComparerFileName : IComparer<Page>
        {
            private readonly CancellationToken _token;

            public ComparerFileName(CancellationToken token)
            {
                _token = token;
            }

            public int Compare(Page? x, Page? y)
            {
                _token.ThrowIfCancellationRequested();

                if (x is null) return (y is null) ? 0 : -1;
                if (y is null) return 1;

                var xName = x.GetEntryNameTokens();
                var yName = y.GetEntryNameTokens();

                var limit = Math.Min(xName.Length, yName.Length);
                for (int i = 0; i < limit; ++i)
                {
                    if (xName[i] != yName[i])
                    {
                        return NaturalSort.Compare(xName[i], yName[i]);
                    }
                }

                return xName.Length - yName.Length;
            }
        }

        public class ComparerFileType : IComparer<Page>
        {
            public ComparerFileType()
            {
            }

            public int Compare(Page? x, Page? y)
            {
                if (x is null) return (y is null) ? 0 : -1;
                if (y is null) return 1;

                var xe = x.ArchiveEntry;
                var ye = y.ArchiveEntry;

                if (xe.IsDirectory) return ye.IsDirectory ? 0 : -1;
                if (ye.IsDirectory) return 1;

                return string.CompareOrdinal(xe.Extension, ye.Extension);
            }
        }
    }
}
