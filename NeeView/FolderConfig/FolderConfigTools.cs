//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// FolderConfig 用ツール
    /// </summary>
    [LocalDebug]
    public static partial class FolderConfigTools
    {
        /// <summary>
        /// ブックサムネイル指定を設定
        /// </summary>
        /// <param name="bookPath">ブックのパス</param>
        /// <param name="thumb">サムネイルのパス。nullでサムネイル解除</param>
        public static void SetThumbnailTarget(string bookPath, string? thumb)
        {
            LocalDebug.WriteLine($"{bookPath}, Thumb={thumb}");

            var place = LoosePath.GetDirectoryName(bookPath);
            if (string.IsNullOrEmpty(place)) throw new IOException("Cannot get directory");

            var name = bookPath.Substring(place.Length).TrimStart('\\');
            if (string.IsNullOrEmpty(name)) throw new IOException("Cannot get name");

            FolderConfigCollection.Current.SetThumbnail(place, name, thumb);
        }

        /// <summary>
        /// ブックサムネイル指定を取得
        /// </summary>
        /// <param name="bookPath"></param>
        public static string? GetThumbnailTarget(string bookPath)
        {
            LocalDebug.WriteLine($"{bookPath}");

            var place = LoosePath.GetDirectoryName(bookPath);
            if (string.IsNullOrEmpty(place)) throw new IOException("Cannot get directory");

            var name = bookPath.Substring(place.Length).TrimStart('\\');
            if (string.IsNullOrEmpty(name)) throw new IOException("Cannot get name");

            var folder = FolderConfigCollection.Current.GetFolderConfig(place);
            if (folder?.Thumbs is null || folder.Thumbs.Count == 0) return null;

            return folder.GetThumbnail(name);
        }

        /// <summary>
        /// 名前変更に追従
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void RenameRecursive(string src, string dst)
        {
            FolderConfigCollection.Current.RenameRecursive(src, dst);
        }
    }

}
