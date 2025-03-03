//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// サムネイルの ImageSource の寿命管理
    /// </summary>
    [LocalDebug]
    public partial class ThumbnailLifetimeManagement
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
                LocalWriteLine($"Added: {thumbnail.SerialNumber}");
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
                    LocalWriteLine($"Removed: {thumbnail.SerialNumber}");
                    _map.Remove(thumbnail);
                }
            }
        }

        [Conditional("LOCAL_DEBUG")]
        private void LocalWriteLine(string s)
        {
            LocalDebug.WriteLine($"({_map.Count}): " + s);
        }
    }
}
