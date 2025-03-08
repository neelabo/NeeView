using NeeLaboratory.ComponentModel;
using System.Collections.Generic;

namespace NeeView
{
    public class RecentBooks : BindableBase
    {
        static RecentBooks() => Current = new RecentBooks();
        public static RecentBooks Current { get; }


        private List<BookHistory> _lastFiles = new();


        private RecentBooks()
        {
            BookHub.Current.BookChanged +=
                (s, e) => UpdateLastFiles();

            BookHistoryCollection.Current.HistoryChanged +=
                (s, e) =>
                {
                    switch (e.HistoryChangedType)
                    {
                        case BookMementoCollectionChangedType.Reset:
                        case BookMementoCollectionChangedType.Load:
                            UpdateLastFiles();
                            break;
                    }
                };
        }

        // 最近使ったフォルダー
        public List<BookHistory> LastFiles
        {
            get { return _lastFiles; }
            set { if (SetProperty(ref _lastFiles, value)) { RaisePropertyChanged(nameof(IsEnableLastFiles)); } }
        }

        // 最近使ったフォルダーの有効フラグ
        public bool IsEnableLastFiles { get { return LastFiles.Count > 0; } }


        // 最近使ったファイル 更新
        private void UpdateLastFiles()
        {
            LastFiles = BookHistoryCollection.Current.ListUp(10);
        }
    }
}
