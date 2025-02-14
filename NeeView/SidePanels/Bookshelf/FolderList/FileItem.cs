//#define LOCAL_DEBUG
using NeeLaboratory.IO.Search;
using System;
using System.IO;
using System.Threading;

namespace NeeView
{
    public class FileItem : ISearchItem
    {
        private FileSystemInfo _info;

        public FileItem(FileSystemInfo fileSystemInfo)
        {
            _info = fileSystemInfo;
        }

        public FileSystemInfo FileSystemInfo => _info;

        public bool IsDirectory => _info.Attributes.HasFlag(FileAttributes.Directory);

        public string Path => _info.FullName;

        public DateTime CreationTime => _info.GetSafeCreationTime();

        public DateTime LastWriteTime => _info.GetSafeLastWriteTime();

        public long Size => _info is System.IO.FileInfo fileInfo ? fileInfo.Length : -1;

        public bool IsBookmark => BookmarkCollection.Current.Contains(Path);

        public bool IsHistory => BookHistoryCollection.Current.Contains(Path);


        public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(_info.Name);
                case "date":
                    return new DateTimeSearchValue(_info.GetSafeLastWriteTime());
                case "size":
                    return new IntegerSearchValue(Size);
                case "bookmark":
                    return new BooleanSearchValue(IsBookmark);
                case "history":
                    return new BooleanSearchValue(IsHistory);
                default:
                    throw new NotSupportedException();
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }

}
