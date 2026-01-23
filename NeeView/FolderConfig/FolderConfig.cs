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
        /// フォルダーのソート順
        /// </summary>
        public FolderParameter.Memento? Parameter { get; set; }

        /// <summary>
        /// サムネイル設定
        /// </summary>
        public Dictionary<string, string> Thumbs { get; set; } = new();


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
    }
}
