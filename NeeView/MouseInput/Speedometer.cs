//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 移動速度計測
    /// </summary>
    [LocalDebug]
    public partial class Speedometer : ISpeedometer
    {
        private readonly RingList<PointRecord> _points;
        private readonly System.Threading.Lock _lock = new();

        public Speedometer()
        {
            _points = new RingList<PointRecord>(5);
        }

        /// <summary>
        /// 計測座標初期化
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _points.Clear();
            }
        }

        /// <summary>
        /// 現在の計測座標追加
        /// </summary>
        /// <param name="point"></param>
        public void Add(Point point)
        {
            Add(point, System.Environment.TickCount);
        }

        public void Add(Point point, int timestamp)
        {
            lock (_lock)
            {
                Chop(timestamp);
                LocalDebug.WriteLine($"add: point={point:f0}, time={timestamp}");
                _points.Add(new PointRecord(point, timestamp));
            }
        }

        /// <summary>
        /// 指定時間以降のデータを削除
        /// </summary>
        /// <param name="timestamp"></param>
        private void Chop(int timestamp)
        {
            lock (_lock)
            {
                while (true)
                {
                    if (_points.Any() && timestamp - _points.LastOrDefault().Timestamp <= 0)
                    {
                        LocalDebug.WriteLine($"chop record over {timestamp}");
                        _points.RemoveLast();
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 計測座標確定
        /// </summary>
        public void Touch()
        {
            if (!_points.Any()) return;
            Add(_points.Last().Point);
        }

        public int GetTimestamp()
        {
            return _points.Any() ? _points.Last().Timestamp : int.MinValue;
        }

        /// <summary>
        /// 計測座標から速度を求める
        /// </summary>
        /// <remarks>
        /// スパイクデータを軽減するため一定期間の平均速度を求める
        /// </remarks>
        /// <returns></returns>
        public Vector GetVelocity()
        {
            var maxSpan = 64; // 計測時間
            var totalSpan = 0;
            var speedSum = default(Vector);

            lock (_lock)
            {
                LocalWriteDump();

                for (int i = _points.Count - 1; i > 0; i--)
                {
                    var p0 = _points[i - 1];
                    var p1 = _points[i - 0];
                    var delta = p1.Point - p0.Point;
                    var span = p1.Timestamp - p0.Timestamp;
                    if (0 < span)
                    {
                        span = Math.Min(span, maxSpan - totalSpan);
                        if (span <= 0) break;
                        var speed = delta / span;
                        totalSpan += span;
                        speedSum += speed * span;
                        LocalDebug.WriteLine($"{i}: Speed = {speed:f2}, Span = {span}");
                    }
                }
            }

            if (totalSpan <= 0) return default;
            var velocity = speedSum / totalSpan;

            LocalDebug.WriteLine($"Velocity = {velocity.Length:f2} ({velocity:f2}), TotalSpan = {totalSpan}");
            return velocity;
        }


        [Conditional("LOCAL_DEBUG")]
        private void LocalWriteDump()
        {
            lock (_lock)
            {
                for (int i = 0; i < _points.Count; i++)
                {
                    var p = _points[i];
                    LocalDebug.WriteLine($"{i}: Point = {p.Point:f0}, Timestamp = {p.Timestamp}");
                }
            }
        }


        /// <summary>
        /// 座標記録
        /// </summary>
        /// <param name="Point">座標</param>
        /// <param name="Timestamp">タイムスタンプ</param>
        private record struct PointRecord(Point Point, int Timestamp);
    }
}

