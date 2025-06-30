using System;

namespace NeeView
{
    public record class BookItemAccessor
    {
        private readonly FolderItem _source;

        public BookItemAccessor(FolderItem source)
        {
            _source = source;
        }

        internal FolderItem Source => _source;

        [WordNodeMember]
        public string? Name
        {
            get { return _source.DisplayName; }
            set { AppDispatcher.Invoke(() => Rename(value)); }
        }


        [WordNodeMember]
        public string Path => _source.TargetPath.SimplePath;

        [WordNodeMember]
        public long Size => _source.Length;

        [WordNodeMember]
        public DateTime LastWriteTime => _source.LastWriteTime;

        [WordNodeMember]
        public DateTime CreationTime => _source.CreationTime;


        [WordNodeMember]
        public void Open()
        {
            var option = BookLoadOption.IsBook | (_source.CanRemove() ? BookLoadOption.None : BookLoadOption.Undeletable);
            BookHub.Current.RequestLoad(this, _source.TargetPath.SimplePath, null, option, true);
        }

        [WordNodeMember]
        public void Open(bool isRecursive)
        {
            var option = BookLoadOption.IsBook | (_source.CanRemove() ? BookLoadOption.None : BookLoadOption.Undeletable) | (isRecursive ? BookLoadOption.Recursive : BookLoadOption.NotRecursive);
            BookHub.Current.RequestLoad(this, _source.TargetPath.SimplePath, null, option, true);
        }

        protected virtual async void Rename(string? newName)
        {
            if (!_source.CanRename()) return;
            await _source.RenameAsync(newName ?? "");
        }
    }

}
