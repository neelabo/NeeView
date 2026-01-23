//#define LOCAL_DEBUG

using AnimatedImage;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [LocalDebug]
    public partial class FolderConfigCollection
    {
        static FolderConfigCollection() => Current = new FolderConfigCollection();
        public static FolderConfigCollection Current { get; }


        [Subscribable]
        public event EventHandler<FolderConfigChangedEventArgs>? FolderChanged;


        public Dictionary<string, FolderConfig> Folders { get; set; } = new();


        public void SetFolderConfig(FolderConfig folder)
        {
            Folders[folder.Place] = folder;
            FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(folder));
        }

        public FolderConfig? GetFolderConfig(string place)
        {
            return Folders.TryGetValue(place, out var config) ? config : null;
        }

        public FolderConfig EnsureFolderConfig(string place)
        {
            if (!Folders.TryGetValue(place, out var config))
            {
                config = new FolderConfig(place);
                Folders[config.Place] = config;
            }
            return config;
        }

        /// <summary>
        /// フォルダー設定初期化
        /// </summary>
        public void ClearAllFolderParameter()
        {
            foreach (var folder in Folders.Values)
            {
                folder.Parameter = null;
            }
            FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(null));
        }

        /// <summary>
        /// フォルダー設定
        /// </summary>
        /// <param name="place"></param>
        /// <param name="parameter"></param>
        public void SetFolderParameter(string place, FolderParameter.Memento parameter)
        {
            place = place ?? "<<root>>";

            // 標準設定は記憶しない
            if (parameter.IsDefault(place))
            {
                var folder = GetFolderConfig(place);
                if (folder is not null)
                {
                    folder.Parameter = null;
                    FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(folder));
                }
            }
            else
            {
                var folder = EnsureFolderConfig(place);
                folder.Parameter = parameter;
                FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(folder));
            }
        }

        /// <summary>
        /// フォルダー設定取得
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public FolderParameter.Memento GetFolderParameter(string place)
        {
            place = place ?? "<<root>>";

            var config = GetFolderConfig(place);
            return config?.Parameter ?? FolderParameter.Memento.GetDefault(place);
        }

        /// <summary>
        /// 名前変更を反映
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void RenameRecursive(string src, string dst)
        {
            if (src == dst) return;

            foreach (var folder in Folders.Values.ToList())
            {
                var isChanged = false;

                // folder place
                if (LoosePath.TryReplaceStartsWith(folder.Place, src, dst, out var renamed))
                {
                    LocalDebug.WriteLine($"Folder: {folder.Place} => {renamed}");
                    Folders.Remove(src);
                    folder.Place = dst;
                    Folders[dst] = folder;
                    isChanged = true;
                }

                foreach (var thumb in folder.Thumbs.ToList())
                {
                    var name = thumb.Key;
                    var bookPath = FolderConfig.GetBookPath(folder, name);

                    // Rename thumbnail book
                    if (LoosePath.TryReplaceStartsWith(bookPath, src, dst, out var newBookPath))
                    {
                        var newName = newBookPath.Substring(folder.Place.Length).TrimStart('\\');
                        LocalDebug.WriteLine($"Thumbnail.Key: {name} => {newName}");
                        folder.RemoveThumbnail(name);
                        folder.SetThumbnail(newName, thumb.Value);
                        name = newName;
                        isChanged = true;
                    }

                    // Rename thumbnail target
                    var target = folder.GetThumbnail(name);
                    if (target is not null && LoosePath.TryReplaceStartsWith(target, src, dst, out var newTarget))
                    {
                        LocalDebug.WriteLine($"Thumbnail[{name}]: {target} => {newTarget}");
                        folder.SetThumbnail(name, newTarget);
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(folder));
                }
            }
        }


        #region Memento

        public class FolderConfigUnit
        {
            public string Place { get; set; } = "";

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public FolderParameter.Memento? Parameter { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public Dictionary<string, string>? Thumbs { get; set; }

            public FolderConfig ToFolderConfig()
            {
                var config = new FolderConfig(Place);
                config.Parameter = Parameter;
                config.Thumbs = Thumbs ?? new();
                return config;
            }

            public bool IsDefault()
            {
                return Parameter is null
                    && Thumbs is null;
            }

            public static FolderConfigUnit Create(FolderConfig config)
            {
                return new FolderConfigUnit()
                {
                    Place = config.Place,
                    Parameter = Config.Current.History.IsKeepFolderStatus ? config.Parameter : null,
                    Thumbs = config.Thumbs.Count > 0 ? config.Thumbs : null,
                };
            }
        }

        public class Memento
        {
            public static string FormatName { get; } = Environment.SolutionName + ".Folders";
            public static FormatVersion FormatVersion { get; } = new FormatVersion(FormatName);

            /// <summary>
            /// フォーマットID
            /// </summary>
            public FormatVersion Format { get; set; } = FormatVersion;

            /// <summary>
            /// フォルダー設定
            /// </summary>
            public List<FolderConfigUnit> Folders { get; set; } = new();


            public void Save(string path)
            {
                var json = JsonSerializer.SerializeToUtf8Bytes(this, UserSettingTools.GetSerializerOptions());
                File.WriteAllBytes(path, json);
            }

            public static Memento Load(string path)
            {
                using var stream = File.OpenRead(path);
                return Load(stream);
            }

            public static Memento Load(Stream stream)
            {
                var memento = JsonSerializer.Deserialize<Memento>(stream, UserSettingTools.GetSerializerOptions());
                if (memento is null) throw new FormatException();
                return memento.Validate();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Folders = Folders.Values.Select(e => FolderConfigUnit.Create(e)).Where(e => !e.IsDefault()).ToList();
            return memento;
        }

        public void Restore(Memento? memento)
        {
            if (memento == null) return;

            Folders = memento.Folders.ToDictionary(e => e.Place, e => e.ToFolderConfig());
        }

        #endregion

    }

    public class FolderConfigChangedEventArgs : EventArgs
    {
        public FolderConfig? FolderConfig { get; }
        public FolderConfigChangedEventArgs(FolderConfig? folderConfig)
        {
            FolderConfig = folderConfig;
        }
    }
}
