//#define LOCAL_DEBUG

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// サムネイルの ImageSource の寿命管理
    /// </summary>
    public class ThumbnailLifetimeManagement
    {
        public static ThumbnailLifetimeManagement Current { get; } = new ThumbnailLifetimeManagement();

        private const int Lifetime = 1000;
        private readonly Dictionary<Thumbnail, int> _map = new();
        private readonly System.Threading.Lock _lock = new();

        public void Add(Thumbnail thumbnail)
        {
            lock (_lock)
            {
                _map[thumbnail] = System.Environment.TickCount;
                Trace($"Added: {thumbnail.SerialNumber}");
                Cleanup();
            }
        }

        public void Cleanup()
        {
            lock (_lock)
            {
                var timestamp = System.Environment.TickCount;
                var removes = _map.Where(e => timestamp - e.Value > Lifetime).Select(e => e.Key).ToList();

                foreach (var thumbnail in removes)
                {
                    thumbnail.RemoveImageSource();
                    Trace($"Removed: {thumbnail.SerialNumber}");
                    _map.Remove(thumbnail);
                }
            }
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}({_map.Count}): {string.Format(CultureInfo.InvariantCulture, s, args)}");
        }
    }
}
