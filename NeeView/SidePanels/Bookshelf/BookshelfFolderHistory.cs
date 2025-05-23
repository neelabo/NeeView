using NeeView.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookshelfFolderHistory
    {
        private const int _historyCapacity = 100;

        private readonly HistoryLimitedCollection<QueryPath> _history = new(_historyCapacity);

        private readonly BookshelfFolderList _folderList;


        public BookshelfFolderHistory(BookshelfFolderList folder)
        {
            _folderList = folder;
            _history.Changed += (s, e) => Changed?.Invoke(s, e);
        }


        public event EventHandler? Changed;


        public void Add(QueryPath item)
        {
            _history.TrimEnd(null);

            if (item != _history.GetCurrent())
            {
                _history.Add(item);
            }
        }

        public bool CanMoveToPrevious()
        {
            return _history.CanPrevious();
        }

        public async ValueTask MoveToPreviousAsync()
        {
            if (!_history.CanPrevious()) return;

            var item = _history.GetPrevious();
            if (item is null) return;

            await LoadPageAsync(item);
            _history.Move(-1);
        }

        public bool CanMoveToNext()
        {
            return _history.CanNext();
        }

        public async ValueTask MoveToNextAsync()
        {
            if (!_history.CanNext()) return;

            var item = _history.GetNext();
            if (item is null) return;

            await LoadPageAsync(item);
            _history.Move(+1);
        }

        public async ValueTask MoveToHistoryAsync(KeyValuePair<int, QueryPath> item)
        {
            var query = _history.GetHistory(item.Key);
            await LoadPageAsync(query);
            _history.SetCurrent(item.Key + 1);
        }

        private async ValueTask LoadPageAsync(QueryPath? item)
        {
            if (item == null) return;
          
            await _folderList.MoveToHistoryAsync(item);
        }

        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }
    }

}
