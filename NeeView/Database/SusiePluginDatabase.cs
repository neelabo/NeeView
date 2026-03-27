//#define LOCAL_DEBUG

using NeeLaboratory.Collections.Specialized;
using NeeLaboratory.Generators;
using NeeView.Susie;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    [LocalDebug]
    public partial class SusiePluginDatabase
    {
        private readonly Database _db;


        public SusiePluginDatabase(Database db)
        {
            _db = db;
            CreateTable();
        }


        public Database.DatabaseTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        public void CreateTable()
        {
            _db.ThrowIfDiposed();

            if (_db.Connection is null) return;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS susie_plugins (
                        path TEXT NOT NULL PRIMARY KEY,
                        api_ver TEXT NOT NULL,
                        plugin_ver TEXT NOT NULL,
                        has_dialog BOOLEAN NOT NULL,
                        exts TEXT NOT NULL
                    )
                    WITHOUT ROWID
                    """;
                command.ExecuteNonQuery();
            }
        }

        public SusiePluginBaseInfo? Read(string path)
        {
            if (_db.Connection is null) return null;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT path, api_ver, plugin_ver, has_dialog, exts
                    FROM susie_plugins
                    WHERE path = @path
                    """;
                command.Parameters.Add(new SQLiteParameter("@path", path));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var pathDb = reader.GetString(0);
                        var apiVer = reader.GetString(1);
                        var pluginVer = reader.GetString(2);
                        var hasDialog = reader.GetBoolean(3);
                        var exts = reader.GetString(4);

                        Debug.Assert(path == pathDb);
                        var plugin = new SusiePluginBaseInfo(path, apiVer, pluginVer, hasDialog, new FileExtensionCollection(exts));
                        LocalDebug.WriteLine($"Read: {plugin}");
                        return plugin;
                    }
                }
            }

            return null;
        }

        public List<SusiePluginBaseInfo> Read(IEnumerable<string> paths)
        {
            var result = new List<SusiePluginBaseInfo>();

            if (_db.Connection is null) return result;

            var queryPaths = paths
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();

            if (queryPaths.Count == 0) return result;

            foreach (string[] chunk in queryPaths.Chunk(100))
            {
                using (SQLiteCommand command = _db.Connection.CreateCommand())
                {
                    var parameterNames = new List<string>(chunk.Length);
                    for (int i = 0; i < chunk.Length; i++)
                    {
                        var name = $"@p{i}";
                        parameterNames.Add(name);
                        command.Parameters.Add(new SQLiteParameter(name, chunk[i]));
                    }

                    command.CommandText = $"""
                        SELECT path, api_ver, plugin_ver, has_dialog, exts
                        FROM susie_plugins
                        WHERE path IN ({string.Join(", ", parameterNames)})
                        """;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var path = reader.GetString(0);
                            var apiVer = reader.GetString(1);
                            var pluginVer = reader.GetString(2);
                            var hasDialog = reader.GetBoolean(3);
                            var exts = reader.GetString(4);

                            var plugin = new SusiePluginBaseInfo(path, apiVer, pluginVer, hasDialog, new FileExtensionCollection(exts));
                            LocalDebug.WriteLine($"Read: {plugin}");

                            result.Add(plugin);
                        }
                    }
                }
            }

            return result;
        }


        public void Write(SusiePluginBaseInfo plugin)
        {
            if (_db.Connection is null) return;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    REPLACE INTO susie_plugins (path, api_ver, plugin_ver, has_dialog, exts)
                    VALUES (@path, @api_ver, @plugin_ver, @has_dialog, @exts);
                    """;
                command.Parameters.Add(new SQLiteParameter("@path", plugin.Path));
                command.Parameters.Add(new SQLiteParameter("@api_ver", plugin.ApiVersion));
                command.Parameters.Add(new SQLiteParameter("@plugin_ver", plugin.PluginVersion));
                command.Parameters.Add(new SQLiteParameter("@has_dialog", plugin.HasConfigurationDlg));
                command.Parameters.Add(new SQLiteParameter("@exts", plugin.DefaultExtensions.ToOneLine() ?? ""));
                command.ExecuteNonQuery();

                LocalDebug.WriteLine($"Write: {plugin}");
            }
        }
    }


    public record SusiePluginBaseInfo
    {
        public SusiePluginBaseInfo()
        {
        }

        public SusiePluginBaseInfo(string path, string apiVersion, string pluginVersion, bool hasDialog, FileExtensionCollection defaultExtensions)
        {
            Path = path;
            ApiVersion = apiVersion;
            PluginVersion = pluginVersion;
            HasConfigurationDlg = hasDialog;
            DefaultExtensions = defaultExtensions;
        }

        public SusiePluginBaseInfo(ISusiePluginInfo pluginInfo, string pluginFolder)
        {
            Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(pluginFolder, pluginInfo.Name));
            ApiVersion = pluginInfo.ApiVersion ?? "";
            PluginVersion = pluginInfo.PluginVersion ?? "";
            HasConfigurationDlg = pluginInfo.HasConfigurationDlg;
            DefaultExtensions = pluginInfo.DefaultExtensions ?? new();
        }

        public string Path { get; init; } = "";
        public string ApiVersion { get; init; } = "";
        public string PluginVersion { get; init; } = "";
        public bool HasConfigurationDlg { get; init; }
        public FileExtensionCollection DefaultExtensions { get; init; } = new();

        public void WriteTo(ISusiePluginInfo pluginInfo)
        {
            pluginInfo.ApiVersion = ApiVersion;
            pluginInfo.PluginVersion = PluginVersion;
            pluginInfo.HasConfigurationDlg = HasConfigurationDlg;
            pluginInfo.DefaultExtensions = DefaultExtensions;
        }
    }
}