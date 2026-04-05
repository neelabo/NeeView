using System;

namespace NeeView
{
    public record class HistoryItemAccessor
    {
        private readonly BookHistory _source;

        public HistoryItemAccessor(BookHistory source)
        {
            _source = source;
        }

        internal BookHistory Source => _source;

        [WordNodeMember]
        public string Name => _source.Name;

        [WordNodeMember]
        public string Path => _source.Path;

        [WordNodeMember]
        [Alternative("@_ScriptManual.DateTypeChangeNote", 42, ScriptErrorLevel.Error, IsFullName = true)] // ver.42
        public DateTime LastAccessTime => _source.LastAccessTime;


        [WordNodeMember]
        public void Open()
        {
            var option = BookLoadOption.IsBook | BookLoadOption.KeepHistoryOrder;
            BookHub.Current.RequestLoad(this, _source.Path, null, option, true);
        }
    }
}
