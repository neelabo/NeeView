using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 表示のスクロール中判定とスクロールキャンセル処理
    /// </summary>
    public class ViewScrollContext
    {
        private readonly Dictionary<IScrollable, int> _map = new();
        private readonly System.Threading.Lock _lock = new();

        public ViewScrollContext()
        {
        }

        public void AddScrollTime(IScrollable scrollable, TimeSpan span)
        {
            if (span <= TimeSpan.Zero)
            {
                RemoveScrollTime(scrollable);
            }
            else
            {
                AddScrollTime(scrollable, System.Environment.TickCount + (int)span.TotalMilliseconds);
            }
        }

        public void AddScrollTime(IScrollable scrollable, int timestamp)
        {
            lock (_lock)
            {
                _map[scrollable] = timestamp;
            }
        }

        public void RemoveScrollTime(IScrollable scrollable)
        {
            lock (_lock)
            {
                _map.Remove(scrollable);
            }
        }

        public void CancelScroll()
        {
            lock (_lock)
            {
                var now = System.Environment.TickCount;
                foreach (var scrollable in _map.Where(e => e.Value - now > 0).Select(e => e.Key))
                {
                    scrollable.CancelScroll();
                }
                _map.Clear();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _map.Clear();
            }
        }

        public bool IsScrolling()
        {
            lock (_lock)
            {
                if (!_map.Any()) return false;

                var now = System.Environment.TickCount;
                return _map.Values.Max(e => e - now) > 0;
            }
        }

        public TimeSpan GetScrollSpan()
        {
            lock (_lock)
            {
                if (!_map.Any()) return TimeSpan.Zero;

                var now = System.Environment.TickCount;
                var span = TimeSpan.FromMilliseconds(Math.Max(_map.Values.Max(e => e - now), 0));
                return span;
            }
        }
    }

}
