﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookMementoCollection
    {
        public static BookMementoCollection Current { get; } = new BookMementoCollection();

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
                return Set(BookMementoUnit.Create(CreateBookMemento(place)));
            }
        }

        // 指定したブックの設定作成
        // TODO: ここではない。 BookHubか？
        private Book.Memento CreateBookMemento(string place)
        {
            if (place == null) throw new ArgumentNullException();

            var memento = BookHub.Current.CreateBookMemento();
            if (memento == null || memento.Place != place)
            {
                memento = BookSetting.Current.BookMementoDefault.Clone();
                memento.Place = place;
            }
            return memento;
        }

        public BookMementoUnit Set(Book.Memento memento)
        {
            var unit = Get(memento.Place);
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
            Items[unit.Memento.Place] = unit;
            return unit;
        }

        public BookMementoUnit Get(string place)
        {
            return Items.TryGetValue(place, out BookMementoUnit memento) ? memento : null;
        }


        public void Clear()
        {
            Items.Clear();
        }


        internal void Rename(string src, string dst)
        {
            if (src == null || dst == null) return;

            var unit = Get(src);
            if (unit != null)
            {
                Items.Remove(src);
                Items.Remove(dst);
                unit.Memento.Place = dst;
                Items.Add(dst, unit);

                BookHistoryCollection.Current.Rename(src, dst);
                BookmarkCollection.Current.Rename(src, dst);
                PagemarkCollection.Current.Rename(src, dst);
            }
        }

        
        public BookMementoUnit GetValid(string place)
        {
            return BookHistoryCollection.Current.FindUnit(place) ?? BookmarkCollection.Current.FindUnit(place) ?? PagemarkCollection.Current.FindUnit(place);
        }

        public void CleanUp()
        {
            var histories = BookHistoryCollection.Current.Items.Select(e => e.Unit);
            var bookmarks = BookmarkCollection.Current.Items.Select(e => e.Value).OfType<Bookmark>().Select(e => e.Unit).Distinct();
            var pagemarks = PagemarkCollection.Current.Items.Select(e => e.Value).OfType<Pagemark>().Select(e => e.Unit).Distinct();

            Items = histories.Union(bookmarks.Union(pagemarks)).ToDictionary(e => e.Place, e => e);
        }
    }
}
