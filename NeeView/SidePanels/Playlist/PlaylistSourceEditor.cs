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
    public partial class PlaylistSourceEditor
    {
        private readonly string _path;
        private readonly PlaylistSource _playlist;
        private bool _isDarty;

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

        public void Save()
        {
            if (!_isDarty) return;

            _isDarty = false;
            PlaylistSourceTools.Save(_playlist, _path, true, false);
        }

        public bool RenamePath(string src, string dst)
        {
            if (_playlist is null) return false;
            if (src == dst) return false;

            var items = _playlist.Items.Where(e => string.Compare(e.Path, src, StringComparison.OrdinalIgnoreCase) == 0).ToList();
            if (items.Count == 0) return false;

            foreach (var item in items)
            {
                LocalDebug.WriteLine($"Rename: {item.Path} => {dst}");
                item.Path = dst;
            }
            _isDarty = true;
            return true;
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

            _isDarty = true;
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
    }
}
