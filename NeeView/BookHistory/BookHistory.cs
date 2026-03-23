using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO.Search;
using NeeView.Collections;
using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;

namespace NeeView
{
    public interface IBookHistory
    {
        string Path { get; set; }
        DateTime LastAccessTime { get; set; }
    }

    public class BookHistory : BindableBase, IHasPage, IHasName, IHasKey<string>, ISearchItem, IBookHistory
    {
        private string _path;
        private BookMementoUnit? _unit;
        private DateTime _lastAccessTime;


        public BookHistory() : this("", default)
        {
        }

        public BookHistory(BookMementoUnit unit, DateTime lastAccessTime) : this(unit.Path, lastAccessTime)
        {
            Unit = unit;
        }

        public BookHistory(string path, DateTime lastAccessTime)
        {
            _path = path;
            LastAccessTime = lastAccessTime;
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

        public DateTime LastAccessTime
        {
            get { return _lastAccessTime; }
            set { SetProperty(ref _lastAccessTime, value); }
        }

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

        public BookHistoryMemento CreateMemento()
        {
            var memento = new BookHistoryMemento();
            memento.Path = Path;
            memento.LastAccessTime = LastAccessTime;
            memento.Page = Unit.Memento.Page;
            memento.Props = Unit.Memento.ToPropertiesString();
            return memento;
        }
    }


    public class BookHistoryMemento : IBookHistory
    {
        public string Path { get; set; } = "";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime LastAccessTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Page { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Props { get; set; }
    }
}
