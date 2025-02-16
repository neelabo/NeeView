using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView
{
    // NOTE: NeeView設定を参照しているので一般化できない
    public static class DataObjectExtensions
    {
        public static void SetQueryPathAndFile(this DataObject data, QueryPath query)
        {
            data.SetQueryPathList([query]);
            if (System.IO.Path.Exists(query.SimplePath))
            {
                data.SetFileDropList([query]);
            }
            data.SetFileTextList([query]);
        }

        public static void SetQueryPathList(this DataObject data, IEnumerable<QueryPath> queries)
        {
            if (!queries.Any()) return;

            data.SetQueryPathCollection(queries);
        }

        public static void SetFileDropList(this DataObject data, IEnumerable<QueryPath> queries)
        {
            var files = queries.Where(e => e.Scheme == QueryScheme.File).Select(e => e.SimplePath).ToArray();
            if (!files.Any()) return;

            data.SetData(DataFormats.FileDrop, files);
        }

        public static void SetFileTextList(this DataObject data, IEnumerable<QueryPath> queries)
        {
            if (Config.Current.System.TextCopyPolicy == TextCopyPolicy.None) return;

            var files = queries.Where(e => e.Scheme == QueryScheme.File).Select(e => e.SimplePath).ToArray();
            if (!files.Any()) return;

            data.SetData(DataFormats.UnicodeText, string.Join(System.Environment.NewLine, files));
        }
    }
}
