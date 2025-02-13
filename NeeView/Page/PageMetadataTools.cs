using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NeeView.Media.Imaging.Metadata;
using NeeView.Text;

namespace NeeView
{
    public static class PageMetadataTools
    {
        public static int GetRating(Page page, CancellationToken token)
        {
            var pictureInfo = LoadPictureInfo(page, token);
            if (pictureInfo == null) return 0;

            if (pictureInfo.Metadata?.ElementAt(BitmapMetadataKey.Rating) is ExifRating rating)
            {
                return rating.ToInteger();
            }

            return 0;
        }

        public static string GetValueString(Page page, string? name, CancellationToken token)
        {
            var value = GetValue(page, name, token);
            if (value is null) return "";

            return MetadataValueTools.ToDisplayString(value) ?? "";
        }

        public static Dictionary<string, string> GetValueStringMap(Page page, CancellationToken token)
        {
            var dic1 = InformationKeyExtensions.DefaultKeys.ToDictionary(e => e.ToString(), e => GetDefinedValue(page, e, token));

            var pictureInfo = LoadPictureInfo(page, token);
            var dic2 = pictureInfo?.Metadata?.ExtraMap.ToDictionary(e => e.Key, e => e.Value) ?? new();

            var merged = dic1.Concat(dic2.Where(pair => !dic1.ContainsKey(pair.Key)))
                .ToDictionary(pair => pair.Key, pair => MetadataValueTools.ToDisplayString(pair.Value) ?? "");

            return merged;
        }

        public static object? GetValue(Page page, string? name, CancellationToken token)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (InformationKeyExtensions.TryParse(name, out var key))
            {
                return GetDefinedValue(page, key, token);
            }
            else
            {
                return GetExtraValue(page, name, token);
            }
        }

        public static object? GetDefinedValue(Page page, InformationKey key, CancellationToken token)
        {
            return key.ToInformationCategory() switch
            {
                InformationCategory.File => CreateInformationFileValue(page, key),
                InformationCategory.Image => CreateInformationImageValue(page, key, token),
                InformationCategory.Metadata => CreateInformationMetaValue(page, key, token),
                _ => throw new NotSupportedException(),
            };
        }

        public static object? GetExtraValue(Page page, string name, CancellationToken token)
        {
            var pictureInfo = LoadPictureInfo(page, token);
            return pictureInfo?.Metadata?.LowerExtraMap.TryGetValue(name.ToLowerInvariant(), out var value) == true ? value : null;
        }

        public static object? GetValue(Page page, InformationKey key)
        {
            return key.ToInformationCategory() switch
            {
                InformationCategory.File => CreateInformationFileValue(page, key),
                InformationCategory.Image => CreateInformationImageValue(page.GetContentPictureInfo(), key),
                InformationCategory.Metadata => CreateInformationMetaValue(page.GetContentPictureInfo(), key),
                _ => throw new NotSupportedException(),
            };
        }


        private static object? CreateInformationFileValue(Page page, InformationKey key)
        {
            Debug.Assert(key.ToInformationCategory() == InformationCategory.File);

            switch (key)
            {
                case InformationKey.FileName:
                    return page?.EntryLastName;
                case InformationKey.FilePath:
                    return page?.TargetPath;
                case InformationKey.FileSize:
                    if (page is null || page.Length <= 0) return null;
                    return new FormatValue(page.Length > 0 ? (page.Length + 1023) / 1024 : 0, "{0:#,0} KB");
                case InformationKey.CreationTime:
                    return page?.CreationTime;
                case InformationKey.LastWriteTime:
                    return page?.LastWriteTime;
                case InformationKey.ArchivePath:
                    return page?.GetFolderPlace();
                case InformationKey.Archiver:
                    return page?.ArchiveEntry.Archive;
                default:
                    throw new NotSupportedException();
            }
        }

        private static object? CreateInformationImageValue(Page page, InformationKey key, CancellationToken token)
        {
            Debug.Assert(key.ToInformationCategory() == InformationCategory.Image);

            var pictureInfo = LoadPictureInfo(page, token);
            return CreateInformationImageValue(page.GetContentPictureInfo(pictureInfo), key);
        }

        private static object? CreateInformationImageValue(PictureInfo? pictureInfo, InformationKey key)
        {
            Debug.Assert(key.ToInformationCategory() == InformationCategory.Image);
            if (pictureInfo is null) return null;

            switch (key)
            {
                case InformationKey.Dimensions:
                    if (pictureInfo is null || pictureInfo.OriginalSize.Width <= 0.0 || pictureInfo.OriginalSize.Height <= 0.0) return null;
                    return $"{(int)pictureInfo.OriginalSize.Width} x {(int)pictureInfo.OriginalSize.Height}" + (pictureInfo.IsLimited ? "*" : "");
                case InformationKey.BitDepth:
                    return new FormatValue(pictureInfo?.BitsPerPixel, "{0}", FormatValue.NotDefaultValueConverter<int>);
                case InformationKey.HorizontalResolution:
                    return new FormatValue(pictureInfo?.BitmapInfo?.DpiX, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>);
                case InformationKey.VerticalResolution:
                    return new FormatValue(pictureInfo?.BitmapInfo?.DpiY, "{0:0.# dpi}", FormatValue.NotDefaultValueConverter<double>);
                case InformationKey.Decoder:
                    return pictureInfo?.Decoder;
                default:
                    throw new NotSupportedException();
            }
        }

        private static object? CreateInformationMetaValue(Page page, InformationKey key, CancellationToken token)
        {
            Debug.Assert(key.ToInformationCategory() == InformationCategory.Metadata);

            var pictureInfo = LoadPictureInfo(page, token);
            return CreateInformationMetaValue(pictureInfo, key);
        }


        private static object? CreateInformationMetaValue(PictureInfo? pictureInfo, InformationKey key)
        {
            Debug.Assert(key.ToInformationCategory() == InformationCategory.Metadata);
            if (pictureInfo is null) return null;

            return pictureInfo?.Metadata?.ElementAt(key.ToBitmapMetadataKey());
        }


        /// <summary>
        /// PictureInfo 読み込み
        /// </summary>
        /// <Remarks>
        /// 重いブロック処理なので取り扱い注意
        /// </Remarks>
        /// <param name="page"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static PictureInfo? LoadPictureInfo(Page page, CancellationToken token)
        {
            var pictureInfo = page.Content.PictureInfo;
            if (pictureInfo is null)
            {
                pictureInfo = page.Content.LoadPictureInfoAsync(token).Result;
            }
            return pictureInfo;
        }

    }
}
