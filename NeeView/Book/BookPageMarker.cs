using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class BookPageMarker
    {
        private readonly BookSource _book;


        public BookPageMarker(BookSource book)
        {
            _book = book;

            _book.Pages.PageRemoved +=
                (s, e) => AppDispatcher.Invoke(() => Pages_PageRemoved(s, e));
        }


        public List<Page> Markers { get; private set; } = new List<Page>();


        /// <summary>
        /// マーカー判定
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool IsMarked(Page page)
        {
            return Markers.Contains(page);
        }

        public void SetMarkers(IEnumerable<Page> pages)
        {
            var newMarks = pages.ToList();

            var removes = Markers.Except(newMarks);
            foreach (var page in removes)
            {
                page.IsMarked = false;
            }

            var news = newMarks.Except(Markers);
            foreach (var page in news)
            {
                page.IsMarked = true;
            }

            Markers = newMarks;
        }

        /// <summary>
        /// マーカー移動可能判定
        /// </summary>
        /// <param name="index">基準座標</param>
        /// <param name="direction">移動方向(+1 or -1)</param>
        /// <param name="isLoop">ループ移動</param>
        /// <returns></returns>
        public bool CanJumpToMarker(int index, int direction, bool isLoop)
        {
            if (Markers == null || Markers.Count == 0) return false;

            if (isLoop) return true;

            var list = Markers.OrderBy(e => e.Index).ToList();

            return direction > 0
                ? list.Last().Index > index
                : list.First().Index < index;
        }

        /// <summary>
        /// ブック内のマーカーを取得
        /// </summary>
        /// <param name="index">基準座標</param>
        /// <param name="direction">移動方向(+1 or -1)</param>
        /// <param name="isLoop">ループ移動</param>
        /// <param name="isIncludeTerminal">終端を含める</param>
        /// <returns>一致するページ。見つからなければ null</returns>
        public Page? GetNearMarkedPage(int index, int direction, bool isLoop, bool isIncludeTerminal)
        {
            Debug.Assert(direction == 1 || direction == -1);

            if (_book.Pages.Count < 2) return null;

            var list = Markers != null ? Markers.OrderBy(e => e.Index).ToList() : new List<Page>();

            if (isIncludeTerminal)
            {
                if (list.FirstOrDefault() != _book.Pages.First())
                {
                    list.Insert(0, _book.Pages.First());
                }
                if (list.LastOrDefault() != _book.Pages.Last())
                {
                    list.Add(_book.Pages.Last());
                }
            }

            if (list.Count == 0) return null;

            var target =
                direction > 0
                ? list.FirstOrDefault(e => e.Index > index) ?? (isLoop ? list.First() : null)
                : list.LastOrDefault(e => e.Index < index) ?? (isLoop ? list.Last() : null);

            return target;
        }

        private void Pages_PageRemoved(object? sender, PageRemovedEventArgs e)
        {
            foreach (var page in e.Pages)
            {
                Markers.Remove(page);
            }
        }
    }
}
