using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NeeLaboratory.IO;
using NeeView.Susie;

namespace NeeView
{
    public class SusieBitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        public async Task<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, bool createSource, CancellationToken token)
        {
            var entry = streamSource.ArchiveEntry;
            if (!Config.Current.Image.Standard.IsAllFileSupported && !PictureProfile.Current.IsSusieSupported(entry.TargetPath))
            {
                return BitmapPageSource.CreateError("not support format");
            }

            try
            {
                var susieImage = entry.EntityPath is not null ? await LoadFromFileAsync(streamSource, token) : await LoadFromStreamAsync(streamSource, token);
                return await CreateImageDataSourceAsync(susieImage, createPictureInfo, createSource, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return BitmapPageSource.CreateError(ex.Message);
            }
        }

        // Bitmap読み込み(stream)
        private static async Task<SusieImage?> LoadFromStreamAsync(ArchiveEntryStreamSource streamSource, CancellationToken token)
        {
            var entry = streamSource.ArchiveEntry;

            byte[] buff;
            using (var stream = await streamSource.OpenStreamAsync(token))
            {
                buff = stream.ToArray(0, (int)entry.Length);
            }

#if false
            byte[] buff;
            var rawData = entry.GetRawData();
            if (rawData != null)
            {
                ////Debug.WriteLine($"SusiePictureStream: {entry.EntryLastName} from RawData");
                buff = rawData;
            }
            else
            {
                ////Debug.WriteLine($"SusiePictureStream: {entry.EntryLastName} from Stream");
                using var stream = entry.OpenEntry();
                buff = stream.ToArray(0, (int)entry.Length);
            }
#endif

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var isCheckExtension = !Config.Current.Image.Standard.IsAllFileSupported;
            var result = accessor.GetPicture(entry.RawEntryName, buff, isCheckExtension); // TODO: await
            await Task.CompletedTask;

            return result;
        }

        // Bitmap読み込み(ファイル版)
        private static async Task<SusieImage?> LoadFromFileAsync(ArchiveEntryStreamSource streamSource, CancellationToken token)
        {
            var entry = streamSource.ArchiveEntry;

            Debug.Assert(entry.EntityPath != null);
            var path = entry.EntityPath ?? throw new InvalidOperationException();

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var isCheckExtension = !Config.Current.Image.Standard.IsAllFileSupported;
            var result = accessor.GetPicture(path, null, isCheckExtension); // TODO: await
            await Task.CompletedTask;

            return result;
        }

        private async Task<BitmapPageSource> CreateImageDataSourceAsync(SusieImage? susieImage, bool createPictureInfo, bool createSource, CancellationToken token)
        {
            if (susieImage == null || susieImage.Plugin == null || susieImage.BitmapData == null)
            {
                return BitmapPageSource.CreateError("SusieIOException");
            }
            else
            {
                var streamSource = new MemoryStreamSource(susieImage.BitmapData);
                var pictureInfo = createPictureInfo ? await PictureInfo.CreateAsync(streamSource, susieImage.Plugin.Name, token) : null;
                var data = createSource ? new BitmapPageData(streamSource) : null;
                return BitmapPageSource.Create(data, pictureInfo, this);
            }
        }
    }
}
