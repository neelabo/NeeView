//#define LOCAL_DEBUG

using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class FolderConfig
    {
        private readonly static string _fileName = ".neeview.json";
        public static string GetFileName(string place) => System.IO.Path.Combine(place, _fileName);


        public FolderConfig() : this("")
        {
        }

        public FolderConfig(string place)
        {
            Place = place;
        }

        /// <summary>
        /// 設定ファイルの場所
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// 最終アクセス時間 (キャッシュの有効期限用)
        /// </summary>
        public int LastTick { get; set; }

        /// <summary>
        /// サムネイル設定
        /// </summary>
        public Dictionary<string, string> Thumbs { get; set; } = new();


        /// <summary>
        /// 最終アクセス時間更新。キャッシュ有効期限用
        /// </summary>
        public void UpdateLastAccessTime()
        {
            LastTick = System.Environment.TickCount;
        }

        /// <summary>
        /// キャッシュの有効期限切れチェック
        /// </summary>
        public bool CacheExpired(int ms)
        {
            return System.Environment.TickCount - LastTick >= ms;
        }

        /// <summary>
        /// サムネイルパス取得
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string? GetThumbnail(string name)
        {
            if (Thumbs.TryGetValue(name, out var target))
            {
                var bookPath = GetBookPath(this, name);
                return GetThumbnailFullPath(bookPath, target);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// サムネイルパス設定
        /// </summary>
        /// <param name="name"></param>
        /// <param name="target"></param>
        public void SetThumbnail(string name, string? target)
        {
            var bookPath = GetBookPath(this, name);

            if (!string.IsNullOrEmpty(target))
            {
                Thumbs[name] = GetThumbnailSmartPath(bookPath, target);
            }
            else
            {
                RemoveThumbnail(name);
            }
        }

        /// <summary>
        /// サムネイルパス削除
        /// </summary>
        /// <param name="name"></param>
        public void RemoveThumbnail(string name)
        {
            Thumbs.Remove(name);
        }

        /// <summary>
        /// ブックパス取得
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetBookPath(FolderConfig config, string name)
        {
            return LoosePath.Combine(config.Place, name);
        }

        /// <summary>
        /// サムネイルフルパス取得
        /// </summary>
        /// <param name="bookPath"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetThumbnailFullPath(string bookPath, string target)
        {
            return Path.IsPathFullyQualified(target) ? target : Path.GetFullPath(Path.Combine(bookPath, target));
        }

        /// <summary>
        /// サムネイルスマートパス取得
        /// </summary>
        /// <remarks>
        /// 可能な限り bookPath からの相対パスに変換する
        /// </remarks>
        /// <param name="bookPath"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetThumbnailSmartPath(string bookPath, string target)
        {
            if (LoosePath.IsDirectoryStartsWith(target, bookPath))
            {
                return target.Substring(bookPath.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            else
            {
                return target;
            }
        }


        #region Memento

        public class Memento
        {
            public static string FormatName { get; } = Environment.SolutionName + ".Folder";
            public static FormatVersion FormatVersion { get; } = new FormatVersion(FormatName, 1, 0, 0);

            /// <summary>
            /// フォーマットID
            /// </summary>
            public FormatVersion Format { get; set; } = FormatVersion;

            /// <summary>
            /// サムネイル設定
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public Dictionary<string, string>? Thumbs { get; set; }


            public void Save(string path)
            {
                var json = JsonSerializer.SerializeToUtf8Bytes(this, UserSettingTools.GetSerializerOptions());

                if (File.Exists(path))
                {
                    FileTools.TruncateAllBytes(path, json);
                }
                else
                {
                    File.WriteAllBytes(path, json);
                    File.SetAttributes(path, FileAttributes.Hidden);
                }
            }

            public static Memento Load(string path)
            {
                var json = File.ReadAllBytes(path);
                var memento = JsonSerializer.Deserialize<Memento>(json, UserSettingTools.GetSerializerOptions());
                if (memento is null) throw new FormatException();
                return memento.Validate();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Thumbs = this.Thumbs.Count > 0 ? this.Thumbs : null;
            return memento;
        }

        public void Restore(Memento? memento)
        {
            if (memento == null) return;
            this.Thumbs = memento.Thumbs ?? new();
        }

        public static FolderConfig Load(string place)
        {
            var path = GetFileName(place);
            var memento = Memento.Load(path);
            var item = new FolderConfig(place);
            item.Restore(memento);
            return item;
        }

        public void Save()
        {
            var path = GetFileName(Place);
            var memento = CreateMemento();
            memento.Save(path);
        }

        #endregion

    }
}
