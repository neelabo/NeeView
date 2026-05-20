using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    public partial class BookmarkFolder : ObservableObject, IBookmarkEntry, ICloneable
    {
        public BookmarkFolder()
        {
        }

        public BookmarkFolder(string name, Color? color, DateTime entryTime) : this()
        {
            Name = name;
            Color = color;
            EntryTime = entryTime;
        }

        [ObservableProperty]
        public partial string? Name { get; set; }

        [ObservableProperty]
        public partial Color? Color { get; set; }

        public DateTime EntryTime { get; set; }


        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool IsEqual(IBookmarkEntry entry)
        {
            return entry is BookmarkFolder folder && this.Name == folder.Name;
        }
    }


    public class BookmarkEmpty : IBookmarkEntry, ICloneable, INotifyPropertyChanged
    {
#pragma warning disable CS0067 
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        public string? Name => "";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
