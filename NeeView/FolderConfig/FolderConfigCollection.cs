#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var action = Folders.ContainsKey(folder.Place) ? FolderConfigChangedAction.Replace : FolderConfigChangedAction.Add;
            Folders[folder.Place] = folder;
            FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(action, folder));
        }

        public FolderConfig? GetFolderConfig(string place)
        {
            return Folders.TryGetValue(place, out var config) ? config : null;
        }

        public FolderConfig EnsureFolderConfig(string place)
        {
            if (!Folders.TryGetValue(place, out var folder))
            {
                folder = new FolderConfig(place);
                SetFolderConfig(folder);
            }
            return folder;
        }

        /// <summary>
        /// サムネイル設定
        /// </summary>
        /// <param name="place"></param>
        /// <param name="name"></param>
        /// <param name="target"></param>
        public void SetThumbnail(string place, string name, string? target)
        {
            var folder = EnsureFolderConfig(place);
            folder.SetThumbnail(name, target);
            FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(FolderConfigChangedAction.Replace, folder) { ThumbsChanged = true });
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
            FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(FolderConfigChangedAction.Reset, null));
        }

        /// <summary>
        /// フォルダー設定
        /// </summary>
        /// <param name="place"></param>
        /// <param name="parameter"></param>
        public void SetFolderParameter(string place, FolderParameter.Memento parameter)
        {
            Debug.Assert(parameter.GetType() == typeof(FolderParameter.Memento));

            place = place ?? "<<root>>";
            var normalizedParameter = parameter.IsDefault(place) ? null : parameter;

            if (Folders.TryGetValue(place, out var folder))
            {
                Debug.Assert(folder.Parameter is null || folder.Parameter.GetType() == typeof(FolderParameter.Memento));
                //bool equalsAsMemento = FolderParameter.Memento.EqualsAsMemento(folder.Parameter, parameter);
                if (folder.Parameter == normalizedParameter) return;
                folder.Parameter = normalizedParameter;
                FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(FolderConfigChangedAction.Replace, folder));
            }
            else
            {
                folder = new FolderConfig(place) { Parameter = normalizedParameter };
                Folders[folder.Place] = folder;
                FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(FolderConfigChangedAction.Add, folder));
            }
        }

        /// <summary>
        /// フォルダー設定取得
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public FolderParameter.Memento GetFolderParameter(QueryPath query)
        {
            var place = query.SimplePath ?? "<<root>>";

            var config = GetFolderConfig(place);

            if (config is null && query.Scheme == QueryScheme.File && Directory.Exists(place))
            {
                // ファイルの古いパスを取得し、そのフォルダー設定取得を試みる
                var oldPath = FileResolver.Current.GetOldPath(place, e => Folders.ContainsKey(e));
                if (oldPath is not null)
                {
                    RenameRecursive(oldPath, place);
                    config = GetFolderConfig(place);
                }
            }

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

            var list = Folders.Values.ToList();

            foreach (var folderPair in Folders.ToList())
            {
                var folder = folderPair.Value;
                var isChanged = folder.RenameRecursive(src, dst);
                if (isChanged)
                {
                    if (folderPair.Key != folder.Place)
                    {
                        Folders.Remove(folderPair.Key);
                        FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(FolderConfigChangedAction.Remove, folder));
                        Folders[folder.Place] = folder;
                        FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(FolderConfigChangedAction.Add, folder));
                    }
                    else
                    {
                        FolderChanged?.Invoke(this, new FolderConfigChangedEventArgs(FolderConfigChangedAction.Replace, folder));
                    }
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
                config.Thumbs = Thumbs;
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
                    Parameter = config.Parameter is not null && Config.Current.History.IsKeepFolderStatus ? config.Parameter : null,
                    Thumbs = config.Thumbs is not null && config.Thumbs.Count > 0 ? config.Thumbs : null,
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
        public FolderConfigChangedEventArgs(FolderConfigChangedAction action, FolderConfig? folderConfig)
        {
            Action = action;
            FolderConfig = folderConfig;
        }

        public FolderConfigChangedAction Action { get; }
        public FolderConfig? FolderConfig { get; }
        public bool ThumbsChanged { get; init; }
    }

    public enum FolderConfigChangedAction
    {
        Add,
        Remove,
        Replace,
        Reset,
    }
}
