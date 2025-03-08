using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    // BookMementoCollectionChangedイベントの種類
    public enum BookMementoCollectionChangedType
    {
        None = 0,
        
        Add,
        Remove,
        Replace,

        Reset,
        Load,

        // 日時更新。グループに影響
        UpdateLastAccessTime,
    }

    // BookMementoCollectionChangedイベントの引数
    public class BookMementoCollectionChangedArgs : EventArgs
    {
        public BookMementoCollectionChangedType HistoryChangedType { get; }
        public List<BookHistory> NewItems { get; }
        public List<BookHistory> OldItems { get; }

        public BookMementoCollectionChangedArgs(BookMementoCollectionChangedType type) : this(type, null, null)
        {
        }

        public BookMementoCollectionChangedArgs(BookMementoCollectionChangedType type, List<BookHistory> newItems) : this(type, newItems, null)
        {
        }

        public BookMementoCollectionChangedArgs(BookMementoCollectionChangedType type, List<BookHistory>? newItems, List<BookHistory>? oldItems)
        {
            newItems ??= new();
            oldItems ??= new();

#if DEBUG
            Debug.Assert(BookMementoCollectionChangedType.None < type && type <= BookMementoCollectionChangedType.UpdateLastAccessTime);
            switch (type)
            {
                case BookMementoCollectionChangedType.Add:
                    Debug.Assert(newItems.Count > 0);
                    Debug.Assert(oldItems.Count == 0);
                    break;

                case BookMementoCollectionChangedType.Remove:
                    Debug.Assert(newItems.Count == 0);
                    Debug.Assert(oldItems.Count > 0);
                    break;

                case BookMementoCollectionChangedType.Replace:
                    Debug.Assert(newItems.Count > 0);
                    Debug.Assert(oldItems.Count > 0);
                    break;

                case BookMementoCollectionChangedType.Reset:
                case BookMementoCollectionChangedType.Load:
                    Debug.Assert(newItems.Count == 0);
                    Debug.Assert(oldItems.Count == 0);
                    break;

                case BookMementoCollectionChangedType.UpdateLastAccessTime:
                    Debug.Assert(newItems.Count > 0);
                    Debug.Assert(oldItems.Count == 0);
                    break;
            }
#endif

            HistoryChangedType = type;
            NewItems = newItems;
            OldItems = oldItems;
        }

        public static BookMementoCollectionChangedArgs Create(BookMementoCollectionChangedType type, List<BookHistory>? items = null)
        {
            switch (type)
            {
                case BookMementoCollectionChangedType.Add:
                    return new BookMementoCollectionChangedArgs(type, items, null);

                case BookMementoCollectionChangedType.Remove:
                    return new BookMementoCollectionChangedArgs(type, null, items);

                case BookMementoCollectionChangedType.Replace:
                    return new BookMementoCollectionChangedArgs(type, items, items);

                case BookMementoCollectionChangedType.Reset:
                case BookMementoCollectionChangedType.Load:
                    return new BookMementoCollectionChangedArgs(type, null, null);

                case BookMementoCollectionChangedType.UpdateLastAccessTime:
                    return new BookMementoCollectionChangedArgs(type, items, null);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
