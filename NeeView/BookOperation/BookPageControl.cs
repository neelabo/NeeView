using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookPageControl : IBookPageControl, IDisposable
    {
        private readonly PageFrameBox _box;
        private readonly Book _book;
        private readonly IBookPageMoveControl _moveControl;
        private readonly IBookPageActionControl _actionControl;
        private bool _disposedValue;



        public BookPageControl(PageFrameBox box, IBookControl bookControl)
        {
            _box = box;
            _book = _box.Book;

            if (_book.IsMedia)
            {
                _moveControl = new MediaPageMoveControl(_box);
                _actionControl = new BookPageActionControl(_box, bookControl);
            }
            else
            {
                _moveControl = new BookPageMoveControl(_box);
                _actionControl = new BookPageActionControl(_box, bookControl);
            }

            _book.Pages.PagesSorted += Book_PagesSorted;
            _book.Pages.PageRemoved += Book_PageRemoved;
            _box.SelectedRangeChanged += Book_SelectedRangeChanged;
        }


        public event EventHandler? PagesChanged;
        public event EventHandler<PageRangeChangedEventArgs>? SelectedRangeChanged;

        public IReadOnlyList<Page> Pages => _book.Pages;
        public PageRange SelectedRange => _box.SelectedRange;
        public IReadOnlyList<Page> SelectedPages => _box.SelectedPages;



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _book.Pages.PagesSorted -= Book_PagesSorted;
                    _book.Pages.PageRemoved -= Book_PageRemoved;
                    _box.SelectedRangeChanged -= Book_SelectedRangeChanged;

                    _actionControl.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }



        private void Book_PagesSorted(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() => PagesChanged?.Invoke(sender, EventArgs.Empty));
        }

        private void Book_PageRemoved(object? sender, PageRemovedEventArgs e)
        {
            AppDispatcher.Invoke(() => PagesChanged?.Invoke(sender, EventArgs.Empty));
        }

        private void Book_SelectedRangeChanged(object? sender, PageRangeChangedEventArgs e)
        {
            AppDispatcher.Invoke(() => SelectedRangeChanged?.Invoke(sender, e));
        }


        #region IBookPageMoveControl

        public void MoveToFirst(object? sender)
        {
            _moveControl.MoveToFirst(sender);
        }

        public void MoveTo(object? sender, int index)
        {
            _moveControl.MoveTo(sender, index);
        }

        public void MoveToRandom(object? sender)
        {
            _moveControl.MoveToRandom(sender);
        }

        public void MoveToLast(object? sender)
        {
            _moveControl.MoveToLast(sender);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            _moveControl.MoveNextFolder(sender, isShowMessage);
        }

        public void MoveNextOne(object? sender)
        {
            _moveControl.MoveNextOne(sender);
        }

        public void MoveNext(object? sender)
        {
            _moveControl.MoveNext(sender);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _moveControl.ScrollToNextFrame(sender, parameter);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _moveControl.MoveNextSize(sender, size);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            _moveControl.MovePrevFolder(sender, isShowMessage);
        }

        public void MovePrevOne(object? sender)
        {
            _moveControl.MovePrevOne(sender);
        }

        public void MovePrev(object? sender)
        {
            _moveControl.MovePrev(sender);
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _moveControl.ScrollToPrevFrame(sender, parameter);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _moveControl.MovePrevSize(sender, size);
        }

        #endregion IBookPageMoveControl

        #region IBookPageActionControl

        public bool CanDeleteFile()
        {
            return _actionControl.CanDeleteFile();
        }

        public bool CanExport()
        {
            return _actionControl.CanExport();
        }

        public bool CanOpenFilePlace()
        {
            return _actionControl.CanOpenFilePlace();
        }

        public bool CanCutToClipboard(CopyFileCommandParameter parameter)
        {
            return _actionControl.CanCutToClipboard(parameter);
        }

        public void CutToClipboard(CopyFileCommandParameter parameter)
        {
            _actionControl.CutToClipboard(parameter);
        }

        public bool CanCopyToClipboard(CopyFileCommandParameter parameter)
        {
            return _actionControl.CanCopyToClipboard(parameter);
        }

        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            _actionControl.CopyToClipboard(parameter);
        }

        public Task DeleteFileAsync()
        {
            return _actionControl.DeleteFileAsync();
        }

        public void Export(ExportImageCommandParameter parameter)
        {
            _actionControl.Export(parameter);
        }

        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            _actionControl.ExportDialog(parameter);
        }

        public bool CanCopyToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            return _actionControl.CanCopyToFolder(parameter, multiPagePolicy);
        }

        public void CopyToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            _actionControl.CopyToFolder(parameter, multiPagePolicy);
        }

        public bool CanMoveToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            return _actionControl.CanMoveToFolder(parameter, multiPagePolicy);
        }

        public void MoveToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy)
        {
            _actionControl.MoveToFolder(parameter, multiPagePolicy);
        }

        public bool CanOpenApplication(IExternalApp parameter, MultiPagePolicy multiPagePolicy)
        {
            return _actionControl.CanOpenApplication(parameter, multiPagePolicy);
        }

        public void OpenApplication(IExternalApp parameter, MultiPagePolicy multiPagePolicy)
        {
            _actionControl.OpenApplication(parameter, multiPagePolicy);
        }

        public void OpenFilePlace()
        {
            _actionControl.OpenFilePlace();
        }

        public bool CanDeleteFile(List<Page> pages)
        {
            return _actionControl.CanDeleteFile(pages);
        }

        public Task DeleteFileAsync(List<Page> pages)
        {
            return _actionControl.DeleteFileAsync(pages);
        }

        public Task RemovePagesAsync(List<Page> pages)
        {
            return _actionControl.RemovePagesAsync(pages);
        }

        #endregion IBookPageActionControl
    }


}
