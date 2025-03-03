//#define LOCAL_DEBUG

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 複数入力に対応した速度計測器
    /// </summary>
    public class MultiSpeedometer : ISpeedometer
    {
        private readonly Dictionary<int, Speedometer> _map = new();
        private readonly System.Threading.Lock _lock = new();

        public MultiSpeedometer()
        {
        }

        public void Add(int id, Point point, int timestamp)
        {
            lock (_lock)
            {
                if (!_map.TryGetValue(id, out var speedometer))
                {
                    speedometer = new Speedometer();
                    _map.Add(id, speedometer);
                }
                speedometer.Add(point, timestamp);
            }
        }

        public void Reset(int id)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(id, out var speedometer))
                {
                    speedometer.Reset();
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _map.Clear();
            }
        }

        /// <summary>
        /// 古いデバイスを削除する
        /// </summary>
        public void Cleanup()
        {
            lock (_lock)
            {
                if (!_map.Any()) return;

                var timestamp = _map.Values.Max(e => e.GetTimestamp());

                // 最後のデータが1秒前のデバイスを削除
                var removes = _map.Where(e => e.Value.GetTimestamp() < timestamp - 1000).Select(e => e.Key).ToList();
                foreach (var id in removes)
                {
                    _map.Remove(id);
                }
            }
        }

        public Vector GetVelocity()
        {
            lock (_lock)
            {
                if (!_map.Any()) return default;

                var speedometer = _map.Values.MaxBy(e => e.GetTimestamp());
                return speedometer?.GetVelocity() ?? default;
            }
        }

    }
}

