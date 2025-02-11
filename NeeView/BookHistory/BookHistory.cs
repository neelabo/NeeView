using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO.Search;
using NeeView.Collections;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Data;

namespace NeeView
{
    public class BookHistory : BindableBase, IHasPage, IHasName, IHasKey<string>, ISearchItem
    {
        private string _path;
        private BookMementoUnit? _unit;

        public BookHistory()
        {
            _path = "";
        }

        public BookHistory(string path, DateTime lastAccessTime)
        {
            _path = path;
            LastAccessTime = lastAccessTime;
        }

        public BookHistory(BookMementoUnit unit, DateTime lastAccessTime)
        {
            _path = unit.Path;
            LastAccessTime = lastAccessTime;
            Unit = unit;
        }

        public string Key => _path;

        public string Path
        {
            get { return _path; }
            set
            {
                if (SetProperty(ref _path, value))
                {
                    _unit = null;
                    RaisePropertyChanged(null);
                }
            }
        }

        public DateTime LastAccessTime { get; set; }

        public Page ArchivePage => Unit.ArchivePage;

        public string Name => Unit.Memento.Name;
        public string? Note => Unit.ArchivePage.ArchiveEntry?.RootArchiveName;
        public string Detail => Path + "\n" + LastAccessTime;

        public IThumbnail Thumbnail => Unit.ArchivePage.Thumbnail;

        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Path); }
            private set { _unit = value; }
        }

        public override string? ToString()
        {
            return Name;
        }

        public Page GetPage()
        {
            return Unit.ArchivePage;
        }

        public long GetLength()
        {
            var file = new FileInfo(_path);
            if (file.Exists) return file.Length;

            return -1;
        }

        public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(Name);
                case "date":
                    return new DateTimeSearchValue(LastAccessTime);
                case "size":
                    return new IntegerSearchValue(GetLength());
                case "bookmark":
                    return new BooleanSearchValue(BookmarkCollection.Current.Contains(_path));
                case "history":
                    return new BooleanSearchValue(true);
                default:
                    throw new NotSupportedException($"Not supported SearchProperty: {profile.Name}");
            }
        }
    }
}
