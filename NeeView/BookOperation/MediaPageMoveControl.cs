using NeeView.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// BookOperation ページ操作 (Media用)
    /// </summary>
    public class MediaPageMoveControl : IBookPageMoveControl
    {
        private readonly PageFrameBox _box;
        private readonly Book _book;

        public MediaPageMoveControl(PageFrameBox box)
        {
            _box = box;
            _book = _box.Book;
            Debug.Assert(_book.IsMedia);
        }

        public void MoveToFirst(object? sender)
        {
            MediaPlayerOperator.BookMediaOperator?.SetPositionFirst();
        }

        public void MoveToLast(object? sender)
        {
            MediaPlayerOperator.BookMediaOperator?.SetPositionLast();
        }

        public void MovePrev(object? sender)
        {
            MoveMediaPage(sender, -1);
        }

        public void MoveNext(object? sender)
        {
            MoveMediaPage(sender, +1);
        }

        public void MovePrevOne(object? sender)
        {
            MoveMediaPage(sender, -1);
        }

        public void MoveNextOne(object? sender)
        {
            MoveMediaPage(sender, +1);
        }

        public void MovePrevSize(object? sender, int size)
        {
            MoveMediaPage(sender, -size);
        }

        public void MoveNextSize(object? sender, int size)
        {
            MoveMediaPage(sender, +size);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            // TODO: チャプター移動？
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            // TODO: チャプター移動？
        }

        public void MoveTo(object? sender, int index)
        {
            // TODO: 時間指定移動？
        }

        public void MoveToRandom(object? sender)
        {
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            if (_box.ScrollToNext(LinkedListDirection.Previous, parameter))
            {
                MoveMediaPage(sender, -1);
            }
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            if (_box.ScrollToNext(LinkedListDirection.Next, parameter))
            {
                MoveMediaPage(sender, +1);
            }
        }


        // ページ移動量をメディアの時間移動量に変換して移動
        private void MoveMediaPage(object? sender, int delta)
        {
            if (MediaPlayerOperator.BookMediaOperator == null) return;

            _box.RaisePageChangeActionEvent(sender, PageChangeAction.Move, true);

            var isTerminated = MediaPlayerOperator.BookMediaOperator.AddPosition(TimeSpan.FromSeconds(delta * Config.Current.Archive.Media.PageSeconds));

            if (isTerminated)
            {
                _box.RaisePageTerminatedEvent(sender, delta < 0 ? -1 : 1, true);
            }
        }
    }
}
