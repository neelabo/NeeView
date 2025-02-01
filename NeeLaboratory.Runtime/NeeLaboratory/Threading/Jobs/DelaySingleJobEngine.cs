//#define LOCAL_DEBUG

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Globalization;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Job の遅延登録を可能にした SingleJobEngine
    /// </summary>
    public class DelaySingleJobEngine : SingleJobEngine
    {
        record JobDelayUnit(IJob Job, int Timestamp);

        private readonly Timer _timer;
        private readonly System.Threading.Lock _lock = new();
        private List<JobDelayUnit> _items = new();
        private bool _disposedValue = false;

        public DelaySingleJobEngine(string name, bool isLogging = true) : base(name, isLogging)
        {
            _timer = new Timer(e => Update(), null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 遅延登録
        /// </summary>
        public void EnqueueDelay(IJob job, int ms)
        {
            if (_disposedValue) return;

            Trace($"EnqueueDelay: {job}, {ms}ms");

            if (ms <= 0)
            {
                Trace($"Enqueue: {job}");
                Enqueue(job);
                return;
            }

            lock (_lock)
            {
                var now = System.Environment.TickCount;
                _items.Add(new JobDelayUnit(job, now + ms));
                UpdateTimer(now);
            }
        }

        /// <summary>
        /// 遅延 Job の登録処理
        /// </summary>
        private void Update()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                if (_items.Count == 0) return;

                var now = System.Environment.TickCount;
                var items = new List<JobDelayUnit>();
                var count = 0;
                foreach (var item in _items)
                {
                    if (now - item.Timestamp >= 0)
                    {
                        Trace($"Enqueue: {item.Job}");
                        Enqueue(item.Job);
                        count++;
                    }
                    else
                    {
                        Trace($"Skip: {item.Job}, {now - item.Timestamp}ms");
                        items.Add(item);
                    }
                }
                //Debug.Assert(count > 0); // タイマーの精度が悪いのでそこそこ発生する
                _items = items;

                UpdateTimer(now);
            }
        }

        /// <summary>
        /// 遅延 Job 登録処理用のタイマー更新
        /// </summary>
        /// <param name="now"></param>
        private void UpdateTimer(int now)
        {
            if (_items.Count == 0)
            {
                Trace($"Timer: End");
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            var span = _items.Min(e => e.Timestamp - now);
            if (span <= 0)
            {
                Trace($"Timer: Now");
                Update();
            }
            else
            {
                Trace($"Timer: {span}ms");
                _timer.Change(span, Timeout.Infinite);
            }
        }



        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s)
        {
            Debug.WriteLine($"{this.GetType().Name}: {s}");
        }

    }

}
