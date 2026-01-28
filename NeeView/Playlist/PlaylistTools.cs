using NeeLaboratory.Generators;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// すべてのプレイリストファイルに対する操作
    /// </summary>
    /// <remarks>
    /// 開かれているプレイリストとの差異をここで吸収する
    /// </remarks>
    [LocalDebug]
    public static partial class PlaylistTools
    {
        /// <summary>
        /// 項目を削除
        /// </summary>
        /// <param name="path"></param>
        /// <param name="items"></param>
        public static void Delete(string path, List<PlaylistSourceItem> items)
        {
            using var editor = CreatePlaylistEditor(path);
            editor?.Delete(items);
        }

        /// <summary>
        /// 名前を変更
        /// </summary>
        /// <param name="path"></param>
        /// <param name="item"></param>
        /// <param name="newName"></param>
        public static string? Rename(string path, PlaylistSourceItem item, string newName)
        {
            if (item.Name == newName) return null;

            using var editor = CreatePlaylistEditor(path);
            if (editor is null) return null;

            return editor.Rename(item, newName);
        }

        /// <summary>
        /// パスから IPlaylistEditor を生成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static IPlaylistEditor? CreatePlaylistEditor(string path)
        {
            if (PlaylistHub.Current.SelectedItem == path)
            {
                return new PlaylistEditor(PlaylistHub.Current.Playlist);
            }
            else
            {
                return PlaylistSourceEditor.Create(path);
            }
        }

    }
}

