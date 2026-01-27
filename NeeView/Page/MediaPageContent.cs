using MediaInfoLib;
using NeeLaboratory;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class MediaPageContent : PageContent
    {
        public MediaPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
        }


        public override bool IsBook => Config.Current.Archive.Media.IsEnabled && ArchiveEntry.Archive is not MediaArchive;


        protected override async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            // ArchiveFileの場合はTempFile化
            var fileProxy = await ArchiveEntry.GetFileProxyAsync(false, token);
            var mediaInfo = CreateMediaInfo(fileProxy.Path); // TODO: async化
            await Task.CompletedTask;
            var pictureInfo = mediaInfo.PictureInfo;
            return pictureInfo;
        }

        protected override async ValueTask<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                // ArchiveFileの場合はTempFile化
                var fileProxy = await ArchiveEntry.GetFileProxyAsync(false, token);
                var mediaInfo = CreateMediaInfo(fileProxy.Path); // TODO: async化
                var pictureInfo = mediaInfo.PictureInfo;
                await Task.CompletedTask;
                return new PageSource(new MediaPageData(fileProxy.Path, mediaInfo.AudioInfo), null, pictureInfo);
            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return new PageSource(null, ex.Message, new PictureInfo(new Size(1920, 1080)));
            }
        }

        private (PictureInfo PictureInfo, AudioInfo? AudioInfo) CreateMediaInfo(string path)
        {
            // 幅と高さを非同期で得るために MediaInfoLib を使用している
            var mediaInfo = new MediaInfoLib.MediaInfo();
            if (!mediaInfo.IsEnabled) throw new ApplicationException("Cannot load MediaInfo.dll");

            mediaInfo.Option("Internet", "No");
            mediaInfo.Option("Cover_Data", "base64");

            int result = mediaInfo.Open(path);
            if (result == 0) throw new IOException($"Cannot open MediaInfo: {path}");

            var pictureInfo = CreatePictureInfo(mediaInfo);
            var audioInfo = CreateAudioInfo(mediaInfo);

            mediaInfo.Close();

            return (pictureInfo, audioInfo);
        }

        private PictureInfo CreatePictureInfo(MediaInfo mediaInfo)
        {
            int width = 512;
            int height = 512;

            int videoCount = mediaInfo.Count_Get(StreamKind.Video);
            if (videoCount > 0)
            {
                if (!int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Width"), out width))
                {
                    width = 512;
                }

                if (!int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Height"), out height))
                {
                    height = 512;
                }

                if (double.TryParse(mediaInfo.Get(StreamKind.Video, 0, "PixelAspectRatio"), out var aspectRatio))
                {
                    width = (int)(width * aspectRatio);
                }

                // libVlc 使用時は回転を反映。 MediaPlayer 使用時は回転を無視。
                if (Config.Current.Archive.Media.IsLibVlcEnabled)
                {
                    if (double.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Rotation"), out var rotation))
                    {
                        if (MathUtility.DegreeToDirection(rotation).IsHorizontal())
                        {
                            (width, height) = (height, width);
                        }
                    }
                }
            }

            var pictureInfo = new PictureInfo();
            pictureInfo.OriginalSize = new Size(width, height);
            pictureInfo.Size = pictureInfo.OriginalSize;

            return pictureInfo;
        }

        private AudioInfo? CreateAudioInfo(MediaInfo mediaInfo)
        {
            int audioCount = mediaInfo.Count_Get(StreamKind.Audio);
            if (audioCount > 0)
            {
                // NOTE: 文字化け対処方法不明
                var title = mediaInfo.Get(StreamKind.General, 0, "Title");
                var album = mediaInfo.Get(StreamKind.General, 0, "Album");
                var artist = mediaInfo.Get(StreamKind.General, 0, "Performer");

                BitmapImage? bitmap = null;
                var coverBase64 = mediaInfo.Get(StreamKind.General, 0, "Cover_Data");
                if (!string.IsNullOrEmpty(coverBase64))
                {
                    try
                    {
                        var ms = new MemoryStream(Convert.FromBase64String(coverBase64));
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CreateOptions = BitmapCreateOptions.None;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"cannot get cover image: {ex.Message}");
                    }
                }

                return new AudioInfo(ArchiveEntry, title, album, artist, bitmap);
            }

            return null;
        }

    }
}
