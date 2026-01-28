//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// Playlistファイルの編集
    /// </summary>
    [LocalDebug]
    public partial class PlaylistSourceEditor : IPlaylistEditor
    {
        private readonly string _path;
        private readonly PlaylistSource _playlist;
        private bool _isDirty;

        public PlaylistSourceEditor(string path, PlaylistSource playlist)
        {
            _path = path;
            _playlist = playlist;
        }

        public static PlaylistSourceEditor? Create(string path)
        {
            if (File.Exists(path))
            {
                var playlist = PlaylistSourceTools.Load(path);
                return new PlaylistSourceEditor(path, playlist);
            }
            else
            {
                LocalDebug.WriteLine($"File not found: {path}");
                return null;
            }
        }

        public void Dispose()
        {
            Save();
        }

        public void Save()
        {
            if (!_isDirty) return;
            _isDirty = false;

            PlaylistSourceTools.Save(_playlist, _path, true, false);
        }

        public bool RenamePathRecursive(string src, string dst)
        {
            if (src == dst) return false;

            var items = CollectPathMembers(_playlist.Items, src);
            LocalDebug.WriteLine($"RenamePathItems.Count = {items.Count}");
            if (items.Count == 0) return false;

            foreach (var item in items)
            {
                var srcPath = item.Path;
                var dstPath = dst + srcPath[src.Length..];
                LocalDebug.WriteLine($"Rename: {srcPath} => {dstPath}");
                item.Path = dstPath;
            }

            _isDirty = true;
            return true;
        }

        /// <summary>
        /// 指定パスに影響する項目を収集する
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static List<PlaylistSourceItem> CollectPathMembers(IEnumerable<PlaylistSourceItem> items, string src)
        {
            return items.Where(e => Contains(e.Path, src)).ToList();

            static bool Contains(string src, string target)
            {
                return src.StartsWith(target, StringComparison.OrdinalIgnoreCase)
                    && (src.Length == target.Length || src[target.Length] == LoosePath.DefaultSeparator);
            }
        }

        /// <summary>
        /// 項目を削除する
        /// </summary>
        /// <param name="items"></param>
        public void Delete(IEnumerable<PlaylistSourceItem> items)
        {
            var targets = new HashSet<PlaylistSourceItem>(items);
            _playlist.Items.RemoveAll(e => targets.Contains(e));
            _isDirty = true;
        }

        /// <summary>
        /// 名前を変更
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public string? Rename(PlaylistSourceItem item, string newName)
        {
            if (item.Name == newName) return null; 

            var target = _playlist.Items.FirstOrDefault(e => e == item);
            if (target == null) return null;
            
            target.Name = newName;
            _isDirty = true;
            return target.Name;
        }

    }
}
