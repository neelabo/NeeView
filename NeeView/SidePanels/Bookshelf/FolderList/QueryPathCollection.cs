using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// QueryPathCollection collection for DataObject
    /// </summary>
    public class QueryPathCollection : List<QueryPath>
    {
        public static readonly string Format = FormatVersion.CreateFormatName(nameof(QueryPathCollection));

        public QueryPathCollection()
        {
        }

        public QueryPathCollection(IEnumerable<QueryPath> collection) : base(collection)
        {
        }
    }


    public static class QueryPathCollectionExtensions
    {
        public static QueryPathCollection ToQueryPathCollection(this IEnumerable<QueryPath> collection)
        {
            return new QueryPathCollection(collection);
        }

        public static void SetQueryPathCollection(this IDataObject data, IEnumerable<QueryPath> collection)
        {
            var queries = collection.Select(e => e.ToString()).ToArray();
            data.SetData(QueryPathCollection.Format, queries);
        }

        public static QueryPathCollection? GetQueryPathCollection(this IDataObject data)
        {
            if (data.GetData(QueryPathCollection.Format) is string[] collection)
            {
                return new QueryPathCollection(collection.Select(e => new QueryPath(e)));
            }
            return null;
        }
    }

}
