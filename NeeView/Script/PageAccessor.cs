using System;
using System.Collections.Generic;
using System.Threading;

namespace NeeView
{
    public record class PageAccessor
    {
        private readonly Page _page;

        public PageAccessor(Page page)
        {
            _page = page;
        }

        internal Page Source => _page;

        [WordNodeMember]
        public int Index => _page.Index;

        [WordNodeMember]
        [Alternative("@_ScriptManual.PagePathChangeNote", 42, ErrorLevel = ScriptErrorLevel.Error, IsFullName = true)] // ver.42
        public string Path => _page.TargetPath;

        [WordNodeMember]
        public string RawPath => _page.EntryFullName;

        [WordNodeMember]
        public long Size => _page.Length;

        [WordNodeMember]
        [Alternative("@_ScriptManual.DateTypeChangeNote", 42, ErrorLevel = ScriptErrorLevel.Error, IsFullName = true)] // ver.42
        public DateTime LastWriteTime => _page.LastWriteTime;

        [WordNodeMember]
        public DateTime CreationTime => _page.CreationTime;

        [WordNodeMember]
        public bool IsBook => _page.IsBook;


        [WordNodeMember]
        public string GetMetaValue(string key)
        {
            // TODO: スクリプト実行のキャンセルトークンを指定するように
            return _page.GetMetaValue(key, CancellationToken.None);
        }

        [WordNodeMember]
        public Dictionary<string, string> GetMetaValueMap()
        {
            // TODO: スクリプト実行のキャンセルトークンを指定するように
            return _page.GetMetaValueMap(CancellationToken.None);
        }


        [WordNodeMember]
        public void Open()
        {
            AppDispatcher.BeginInvoke(() =>
            {
                var handled = BookOperation.Current.JumpPageWithPath(this, _page.EntryFullName);
                if (!handled)
                {
                    BookHub.Current.RequestLoad(this, _page.BookPath, _page.EntryName, BookLoadOption.IsPage, true);
                }
            });
        }

        [WordNodeMember]
        public void OpenAsBook()
        {
            BookHub.Current.RequestLoad(this, _page.EntryFullName, null, BookLoadOption.IsBook, true);
        }
    }
}
