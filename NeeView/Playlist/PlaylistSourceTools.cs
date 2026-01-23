#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace NeeView
{
    [LocalDebug]
    public static partial class PlaylistSourceTools
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);


        public static string? CreateTempPlaylist(IEnumerable<string> files)
        {
            if (files is null || !files.Any())
            {
                return null;
            }

            return CreateTmpPlaylist(files, Temporary.Current.TempDownloadDirectory);
        }

        private static string CreateTmpPlaylist(IEnumerable<string> files, string outputDirectory)
        {
            string name = DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + PlaylistArchive.Extension;
            string path = FileIO.CreateUniquePath(System.IO.Path.Combine(outputDirectory, name));
            Save(new PlaylistSource(files), path, true, true);
            return path;
        }


        public static void Save(this PlaylistSource playlist, string path, bool overwrite, bool createDirectory)
        {
            if (!overwrite && File.Exists(path))
            {
                throw new IOException($"Cannot overwrite: {path}");
            }

            _semaphore.Wait();
            try
            {
                Debug.WriteLine($"Save: {path}");
                playlist.Format = PlaylistSource.FormatVersion;
                var json = JsonSerializer.SerializeToUtf8Bytes(playlist, UserSettingTools.GetSerializerOptions());

                var directory = Path.GetDirectoryName(path);
                if (directory is null) throw new IOException("Directory must not be null.");
                if (!Directory.Exists(directory))
                {
                    if (createDirectory)
                    {
                        Directory.CreateDirectory(directory);
                    }
                    else
                    {
                        throw new IOException($"Directory does not exist: {directory}");
                    }
                }
                File.WriteAllBytes(path, json);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static PlaylistSource Load(string path)
        {
            _semaphore.Wait();
            try
            {
                Debug.WriteLine($"Load: {path}");
                var json = FileTools.ReadAllBytes(path, FileShare.Read);
                return Deserialize(json);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static PlaylistSource Deserialize(byte[] json)
        {
            var fileHeader = JsonSerializer.Deserialize<PlaylistFileHeader>(json, UserSettingTools.GetSerializerOptions());

#pragma warning disable CS0612, CS0618 // 型またはメンバーが旧型式です
            if (fileHeader?.Format != null && fileHeader.Format.Name == PlaylistSourceV1.FormatVersion)
            {
                var playlistV1 = JsonSerializer.Deserialize<PlaylistSourceV1>(json, UserSettingTools.GetSerializerOptions());
                if (playlistV1 is null) throw new FormatException();
                return playlistV1.ToPlaylist().Validate();
            }
#pragma warning restore CS0612, CS0618 // 型またはメンバーが旧型式です

            else
            {
                var playlistSource = JsonSerializer.Deserialize<PlaylistSource>(json, UserSettingTools.GetSerializerOptions());
                if (playlistSource is null) throw new FormatException();
                return playlistSource.Validate();
            }
        }

        /// <summary>
        /// プレイリストを FileResolver に登録
        /// </summary>
        /// <param name="path">プレイリストファイル</param>
        public static void AddToFileResolver(string path)
        {
            using (ProcessLock.Lock())
            {
                var playlist = Load(path);
                if (playlist.AddToFileResolver())
                {
                    playlist.Save(path, true, false);
                }
            }
        }

        /// <summary>
        /// プレイリストを FileResolver に登録
        /// </summary>
        /// <param name="playlist">プレイリスト</param>
        public static bool AddToFileResolver(this PlaylistSource playlist)
        {
            // Ver 2.0.1 より古いプレイリストのみ処理する
            if (playlist.Format.CompareTo(new FormatVersion(PlaylistSource.FormatName, 2, 0, 1)) < 0)
            {
                var count = FileResolver.Current.AddRangeArchivePath(playlist.Items.Select(e => e.Path));
                LocalDebug.WriteLine($"Count = {count}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// プレイリスト項目のパスの復元と読み込み
        /// </summary>
        /// <param name="path"></param>
        public static PlaylistSource LoadFileResolved(string path)
        {
            using (ProcessLock.Lock())
            {
                var playlist = Load(path);
                if (playlist.ResolveFilePath())
                {
                    playlist.Save(path, true, false);
                }
                return playlist;
            }
        }

        /// <summary>
        /// プレイリスト項目のパスの復元
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public static bool ResolveFilePath(this PlaylistSource playlist)
        {
            int count = 0;
            foreach (var item in playlist.Items)
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

        private class PlaylistFileHeader
        {
            public FormatVersion? Format { get; set; }
        }
    }

}
