using NeeLaboratory.ComponentModel;
using System.Collections.Generic;

namespace NeeView
{
    public class RecentBookList : BindableBase
    {
        static RecentBookList() => Current = new RecentBookList();
        public static RecentBookList Current { get; }


        private List<BookHistory> _books = new();
        private bool _isDirty = true;


        private RecentBookList()
        {
            BookHub.Current.BookChanged +=
                (s, e) => _isDirty = true;

            BookHistoryCollection.Current.HistoryChanged +=
                (s, e) =>
                {
                    switch (e.HistoryChangedType)
                    {
                        case BookMementoCollectionChangedType.Reset:
                        case BookMementoCollectionChangedType.Load:
                            _isDirty = true;
                            break;
                    }
                };

            Config.Current.History.SubscribePropertyChanged(nameof(HistoryConfig.RecentBookCount),
                (s, e) => _isDirty = true);
        }


        public List<BookHistory> Books
        {
            get { return _books; }
            set
            {
                if (SetProperty(ref _books, value))
                {
                    RaisePropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public bool IsEnabled
        {
            get { return Books.Count > 0; }
        }


        public List<BookHistory> GetBooks()
        {
            Update();
            return Books;
        }

        public void Update()
        {
            if (!_isDirty) return;
            _isDirty = false;

            Books = BookHistoryCollection.Current.ListUp(Config.Current.History.RecentBookCount);
        }
    }
}
