//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    [LocalDebug]
    public partial class BookMementoCollection
    {
        static BookMementoCollection() => Current = new BookMementoCollection();
        public static BookMementoCollection Current { get; }

        public Dictionary<string, BookMementoUnit> Items { get; private set; } = new Dictionary<string, BookMementoUnit>();


        public BookMementoUnit Set(string place)
        {
            var unit = Get(place);
            if (unit != null)
            {
                return unit;
            }
            else
            {
                return Set(BookMementoUnit.Create(BookMementoTools.CreateBookMemento(place)));
            }
        }

        public BookMementoUnit Set(BookMemento memento)
        {
            var unit = Get(memento.Path);
            if (unit != null)
            {
                unit.Memento = memento;
                return unit;
            }
            else
            {
                return Set(BookMementoUnit.Create(memento));
            }
        }

        public BookMementoUnit Set(BookMementoUnit unit)
        {
            Items[unit.Memento.Path] = unit;
            return unit;
        }

        public BookMementoUnit? Get(string place)
        {
            return Items.TryGetValue(place, out BookMementoUnit? memento) ? memento : null;
        }

        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// 影響するパスすべてを名前変更する
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void RenameRecursive(string src, string dst)
        {
            var items = CollectPathMembers(src);
            LocalDebug.WriteLine($"RenameItems.Count = {items.Count}");

            foreach (var item in items)
            {
                var srcPath = item.Path;
                var dstPath = dst + srcPath[src.Length..];
                Rename(srcPath, dstPath);
            }
        }

        /// <summary>
        /// 指定パスに影響する項目を収集する
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private List<BookMementoUnit> CollectPathMembers(string src)
        { 
            var srcDirectory = LoosePath.TrimDirectoryEnd(src);
            var items = Items.Values.Where(e => Contains(e.Path, src, srcDirectory));
            return items.ToList();

            static bool Contains(string path, string target, string targetDirectory)
            {
                if (string.Compare(path, target, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                if (path.StartsWith(targetDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 名前変更
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void Rename(string src, string dst)
        {
            if (src == null || dst == null) return;
            if (src == dst) return;

            var unit = Get(src);
            if (unit != null)
            {
                LocalDebug.WriteLine($"Rename: {src} => {dst}");

                Items.Remove(src);
                Items.Remove(dst);
                unit.Memento.Path = dst;
                Items.Add(dst, unit);

                BookHistoryCollection.Current.Rename(src, dst);
                BookmarkCollection.Current.Rename(src, dst);
            }
        }
        
        public BookMementoUnit? GetValid(string place)
        {
            return BookHistoryCollection.Current.FindUnit(place) ?? BookmarkCollection.Current.FindUnit(place);
        }

        public void CleanUp()
        {
            var histories = BookHistoryCollection.Current.Items.Select(e => e.Unit);
            var bookmarks = BookmarkCollection.Current.Items.WalkChildren().Select(e => e.Value).OfType<Bookmark>().Select(e => e.Unit).Distinct();

            Items = histories.Union(bookmarks).ToDictionary(e => e.Path, e => e);
        }
    }
}
