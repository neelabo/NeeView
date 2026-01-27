//#define LOCAL_DEBUG

using NeeView.Collections;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public struct PageHistoryUnit : IEquatable<PageHistoryUnit>
    {
        public static readonly PageHistoryUnit Empty = new("", "");

        public PageHistoryUnit(string bookAddress, string pageName)
        {
            BookAddress = bookAddress;
            PageName = pageName;
        }

        public string BookAddress { get; private set; }
        public string PageName { get; private set; }

        public string EntryFullName => LoosePath.Combine(BookAddress, PageName);


        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(BookAddress) && string.IsNullOrEmpty(PageName);
        }

        public override bool Equals(object? obj)
        {
            if (obj is PageHistoryUnit other)
            {
                return this.Equals(other);
            }
            return false;
        }

        public bool Equals(PageHistoryUnit other)
        {
            return (BookAddress == other.BookAddress) && (PageName == other.PageName);
        }

        public override int GetHashCode()
        {
            return BookAddress.GetHashCode(StringComparison.Ordinal) ^ PageName.GetHashCode(StringComparison.Ordinal);
        }

        public static bool operator ==(PageHistoryUnit lhs, PageHistoryUnit rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PageHistoryUnit lhs, PageHistoryUnit rhs)
        {
            return !(lhs.Equals(rhs));
        }

        public override string ToString()
        {
            return LoosePath.Combine(BookAddress, PageName);
        }
    }

    /// <summary>
    /// BookHubの履歴。
    /// 本を開いた順番そのままを記録している。
    /// </summary>
    public class PageHistory
    {
        static PageHistory() => Current = new PageHistory();
        public static PageHistory Current { get; }


        private const int _historyCapacity = 100;
        private readonly HistoryLimitedCollection<PageHistoryUnit> _history = new(_historyCapacity);
        private PageFrameBoxPresenter _presenter;


        public PageHistory()
        {
            _history.Changed += (s, e) => Changed?.Invoke(s, e);

            _presenter = MainViewComponent.Current.PageFrameBoxPresenter;

            _presenter.ViewContentChanged += Presenter_ViewContentChanged;
        }



        public event EventHandler? Changed;


        private void Presenter_ViewContentChanged(object? sender, PageFrames.FrameViewContentChangedEventArgs e)
        {
            if (e.Action < ViewContentChangedAction.Selection) return;

            var viewPages = e.ViewContents.Select(e => e.Page).ToList();

            PageHistoryUnit pageHistoryUnit;
            if (viewPages != null && viewPages.Count > 0)
            {
                var page = viewPages.Select(page => (page.Index, page)).Min().page;
                pageHistoryUnit = new PageHistoryUnit(page.BookPath, page.EntryName);
            }
            else
            {
                // NOTE: 空白ページを登録することで直前ページを正常に参照できるようにする
                pageHistoryUnit = PageHistoryUnit.Empty;
            }

            Add(sender, pageHistoryUnit);
        }


        public void Add(object? sender, PageHistoryUnit unit)
        {
            // NOTE: 履歴操からの操作では履歴を変更しない .. なくても問題なし？
            //if (sender == this) return;

            _history.TrimEnd(PageHistoryUnit.Empty);

            if (unit != _history.GetCurrent())
            {
                _history.Add(unit);
            }
        }

        public bool CanMoveToPrevious()
        {
            return _history.CanPrevious();
        }

        public void MoveToPrevious()
        {
            if (!_history.CanPrevious()) return;

            var query = _history.GetPrevious();
            _history.Move(-1);
            LoadPage(query);
        }

        public bool CanMoveToNext()
        {
            return _history.CanNext();
        }

        public void MoveToNext()
        {
            if (!_history.CanNext()) return;

            var query = _history.GetNext();
            _history.Move(+1);
            LoadPage(query);
        }

        public void MoveToHistory(KeyValuePair<int, PageHistoryUnit> item)
        {
            var query = _history.GetHistory(item.Key);
            _history.SetCurrent(item.Key + 1);
            LoadPage(query);
        }

        private void LoadPage(PageHistoryUnit unit)
        {
            if (unit.IsEmpty()) return;

            if (BookOperation.Current.Address == unit.BookAddress)
            {
                BookOperation.Current.JumpPageWithPath(this, unit.EntryFullName);
            }
            else
            {
                var option = BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe;
                BookHub.Current.RequestLoad(this, unit.BookAddress, unit.PageName, option, true);
            }
        }

        internal List<KeyValuePair<int, PageHistoryUnit>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }

    }
}

