//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeeView
{
    [LocalDebug]
    public partial class FolderConfig
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
        public Dictionary<string, string>? Thumbs { get; set; } = new();

        /// <summary>
        /// 標準判定
        /// </summary>
        /// <returns></returns>
        public bool IsDefault()
        {
            return (Parameter is null || Parameter.IsDefault(Place))
                && (Thumbs is null || Thumbs.Count == 0);
        }

        /// <summary>
        /// サムネイルパス取得
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string? GetThumbnail(string name)
        {
            if (Thumbs is not null && Thumbs.TryGetValue(name, out var target))
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
                Thumbs ??= new();
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
            Thumbs?.Remove(name);
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

        /// <summary>
        /// パス変更を反映
        /// </summary>
        /// <param name="src">元のパス</param>
        /// <param name="dst">変更後のパス</param>
        /// <returns></returns>
        public bool RenameRecursive(string src, string dst)
        {
            if (src == dst) return false;

            var isChanged = false;

            // folder place
            if (LoosePath.TryReplaceStartsWith(Place, src, dst, out var renamed))
            {
                LocalDebug.WriteLine($"Folder: {Place} => {renamed}");
                Place = renamed;
                isChanged = true;
            }

            if (Thumbs is not null)
            {
                foreach (var thumb in Thumbs.ToList())
                {
                    var t0 = new ThumbPathUnit(Place, thumb.Key, thumb.Value);
                    var t1 = RenameThumb(t0, src, dst);

                    if (t0.Name != t1.Name)
                    {
                        RemoveThumbnail(t0.Name);
                    }
                    if (t0 != t1)
                    {
                        SetThumbnail(t1.Name, t1.Target);
                        isChanged = true;
                    }
                }
            }

            return isChanged;
        }


        /// <summary>
        /// サムネイル情報にパス変更を適用したものを生成
        /// </summary>
        /// <param name="src">元のパス</param>
        /// <param name="dst">変更後のパス</param>
        /// <returns></returns>
        private ThumbPathUnit RenameThumb(ThumbPathUnit thumbSource, string src, string dst)
        {
            var thumb = thumbSource;

            // Rename thumbnail book
            if (LoosePath.TryReplaceStartsWith(thumb.BookPath, src, dst, out var newBookPath))
            {
                if (LoosePath.GetDirectoryName(newBookPath) == Place)
                {
                    var newName = LoosePath.GetFileName(newBookPath);
                    LocalDebug.WriteLine($"Thumb.Name: {thumb.Name} => {newName}");
                    thumb = thumb with { Name = newName };
                }
            }

            // Rename thumbnail target
            if (Path.IsPathFullyQualified(thumb.Target))
            {
                // absolute path
                if (LoosePath.TryReplaceStartsWith(thumb.Target, src, dst, out var newTargetPath))
                {
                    LocalDebug.WriteLine($"Thumb.Target: {thumb.Target} => {newTargetPath}");
                    thumb = thumb with { Target = newTargetPath };
                }
            }
            else
            {
                // relative path
                if (LoosePath.TryReplaceStartsWith(thumb.TargetPath, src, dst, out var newTargetPath))
                {
                    if (LoosePath.IsDirectoryStartsWith(newTargetPath, thumb.BookPath))
                    {
                        var newTarget = newTargetPath.Substring(thumb.BookPath.Length).TrimStart('\\');
                        LocalDebug.WriteLine($"Thumb.Target: {thumb.Target} => {newTarget}");
                        thumb = thumb with { Target = newTarget };
                    }
                }
            }

            return thumb;
        }


        /// <summary>
        /// サムネイルパス編集用
        /// </summary>
        /// <param name="Place">フォルダー設定のパス</param>
        /// <param name="Name">ブックネーム</param>
        /// <param name="Target">サムネイルパス</param>
        private record class ThumbPathUnit(string Place, string Name, string Target)
        {
            public string BookPath => LoosePath.Combine(Place, Name);
            public string TargetPath => Path.IsPathFullyQualified(Target) ? Target : Path.Combine(BookPath, Target);
        }
    }
}
