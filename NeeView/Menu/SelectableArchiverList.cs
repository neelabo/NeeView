using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class SelectableArchiverList : BindableBase
    {
        static SelectableArchiverList() => Current = new SelectableArchiverList();
        public static SelectableArchiverList Current { get; }


        private List<SelectableArchiverItem> _archivers = new();
        private bool _isDirty = true;


        public SelectableArchiverList()
        {
            BookHub.Current.BookChanged +=
                (s, e) => _isDirty = true;
        }


        public List<SelectableArchiverItem> Archivers
        {
            get { return _archivers; }
            set
            {
                if (SetProperty(ref _archivers, value))
                {
                    RaisePropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public bool IsEnabled
        {
            get { return Archivers.Count > 0; }
        }


        public List<SelectableArchiverItem> GetLatestArchivers()
        {
            Update();
            return Archivers;
        }

        public void Update()
        {
            if (!_isDirty) return;
            _isDirty = false;

            var book = BookOperation.Current.Book;
            if (book is not null)
            {
                var bookArchiveIdentifider = book.Source.ArchiverIdentifier;
                Archivers = ArchiveManager.Current.GetSupportedArchiverList(book.Path)
                    .Select(e => new SelectableArchiverItem(e, e == bookArchiveIdentifider))
                    .ToList();
            }
            else
            {
                Archivers = new();
            }
        }
    }

    public record SelectableArchiverItem(ArchiverIdentifier ArchiverIdentifier, bool IsChecked);
}
