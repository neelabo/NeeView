using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    [LocalDebug]
    public partial class PlaylistSource
    {
        public static string FormatName => Environment.SolutionName + ".Playlist";
        public static readonly FormatVersion FormatVersion = new(FormatName, 2, 0, 1);

        public PlaylistSource()
        {
            Items = new List<PlaylistSourceItem>();
        }

        public PlaylistSource(IEnumerable<string> items)
        {
            Items = items.Select(e => new PlaylistSourceItem(e)).ToList();
        }

        public PlaylistSource(IEnumerable<PlaylistSourceItem> items)
        {
            Items = items.ToList();
        }

        public FormatVersion Format { get; set; } = FormatVersion;

        public List<PlaylistSourceItem> Items { get; set; }


        /// <summary>
        /// プレイリストを FileResolver に登録
        /// </summary>
        /// <param name="playlist">プレイリスト</param>
        public void AddToFileResolver()
        {
            var files = Items.Select(e => e.Path).ToList();
            var count = FileResolver.Current.AddRangeArchivePath(files);
            LocalDebug.WriteLine($"Count = {count}");
        }

        /// <summary>
        /// プレイリスト項目のパスの復元
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public bool ResolveFilePath()
        {
            int count = 0;
            foreach (var item in Items)
            {
                // パスの復元
                var sourcePath = item.Path;
                var archivePath = FileResolver.Current.ResolveArchivePath(sourcePath);
                if (archivePath != null && archivePath.Path != sourcePath)
                {
                    item.Path = archivePath.Path;
                    FileResolver.Current.Add(archivePath.SystemPath);
                    count++;
                }
            }
            return count > 0;
        }

    }
}
