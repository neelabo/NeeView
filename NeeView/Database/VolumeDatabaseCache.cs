using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// VolumePath と VolumePathId 変換をデータベースを介して管理する
    /// </summary>
    public class VolumeDatabaseCache
    {
        private VolumeDatabase _db;
        private Dictionary<int, string> _map = new();
        private Dictionary<string, int> _mapReverse = new();
        private Lock _lock = new();

        public VolumeDatabaseCache(Database db)
        {
            _db = new VolumeDatabase(db);

            _map = _db.ReadTable();
            _mapReverse = _map.ToDictionary(e => e.Value, e => e.Key);
        }

        public int AddVolumePath(string volumePath)
        {
            lock (_lock)
            {
                if (_mapReverse.TryGetValue(volumePath, out var volumeId))
                {
                    return volumeId;
                }
                else
                {
                    volumeId = _map.Count;
                    _map.Add(volumeId, volumePath);
                    _mapReverse = _map.ToDictionary(e => e.Value, e => e.Key);
                    _db.WriteIfNotExist(volumeId, volumePath);
                    return volumeId;
                }
            }
        }

        public string? GetVolumePath(int id)
        {
            lock (_lock)
            {
                return id < 0 || id >= _map.Count ? null : _map[id];
            }
        }
    }

}

