using NeeLaboratory.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public class SusiePluginDatabaseCache
    {
        private readonly SusiePluginDatabase _db;
        private readonly Dictionary<string, SusiePluginBaseInfo> _cache = new();
        private readonly Lock _lock = new();

        public SusiePluginDatabaseCache(Database db)
        {
            _db = new SusiePluginDatabase(db);
        }

        public List<SusiePluginBaseInfo> Read(IEnumerable<string> paths)
        {
            lock (_lock)
            {
                var keys = paths.ToList();

                foreach (var info in _db.Read(keys.Except(_cache.Keys)))
                {
                    _cache[info.Path] = info;
                }

                return keys
                    .Select(e => _cache.TryGetValue(e, out var info) ? info : null)
                    .WhereNotNull()
                    .ToList();
            }
        }

        public void Write(IEnumerable<SusiePluginBaseInfo> plugins)
        {
            lock (_lock)
            {
                using var transaction = _db.BeginTransaction();
                try
                {
                    foreach (var plugin in plugins)
                    {
                        Write(plugin);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        public SusiePluginBaseInfo? Read(string path)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(path, out var plugin))
                {
                    return plugin;
                }

                plugin = _db.Read(path);
                if (plugin is null)
                {
                    return null;
                }

                _cache.Add(path, plugin);
                return plugin;
            }
        }

        public void Write(SusiePluginBaseInfo plugin)
        {
            if (plugin is null) return;

            lock (_lock)
            {
                if (_cache.TryGetValue(plugin.Path, out var oldInfo))
                {
                    if (plugin == oldInfo)
                    {
                        return;
                    }
                }

                _db.Write(plugin);
                _cache[plugin.Path] = plugin;
            }
        }
    }
}