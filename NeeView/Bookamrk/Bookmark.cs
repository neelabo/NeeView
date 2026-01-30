using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;

namespace NeeView
{
    public interface IBookmarkEntry : ITreeListNode
    {
    }


    public class Bookmark : BindableBase, IBookmarkEntry, ICloneable
    {
        private string? _name;
        private string _path;
        private BookMementoUnit? _unit;


        public Bookmark(string path)
        {
            _path = path;
        }

        public Bookmark(BookMementoUnit unit)
        {
            _path = unit.Path;
            _unit = unit;
        }


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

        // NOTE: ver44.0 以降では未使用ですが、フォーマット互換性のために保持している。
        public DateTime EntryTime { get; set; } = DateTime.Now;

        public string? RawName => _name;

        public string Name
        {
            get { return _name ?? Unit.Memento.Name; }
            set { SetProperty(ref _name, CreateRawName(value)); }
        }

        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Path); }
            private set { _unit = value; }
        }

        public bool IsUnlinked { get; set; }

        private string? CreateRawName(string? s)
        {
            var name = BookmarkTools.GetValidateName(s);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (name == Unit.Memento.Name)
            {
                return null;
            }

            return name;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool IsEqual(IBookmarkEntry entry)
        {
            return entry is Bookmark bookmark && this.Name == bookmark.Name && this.Path == bookmark.Path;
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }

    }
}
