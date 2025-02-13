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
    public static class PlaylistSourceTools
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
                return playlistV1.ToPlaylist();
            }
#pragma warning restore CS0612, CS0618 // 型またはメンバーが旧型式です

            else
            {
                var playlistSource = JsonSerializer.Deserialize<PlaylistSource>(json, UserSettingTools.GetSerializerOptions());
                if (playlistSource is null) throw new FormatException();
                return playlistSource;
            }
        }


        private class PlaylistFileHeader
        {
            public FormatVersion? Format { get; set; }
        }
    }

}
