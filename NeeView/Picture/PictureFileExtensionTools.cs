using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル拡張子
    /// </summary>
    public static class PictureFileExtensionTools
    {
        // デフォルトローダーのサポート拡張子を更新
        public static FileTypeCollection CreateDefaultSupportedFileTypes(bool useWic)
        {
            var collection = (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
                ? CreateSystemExtensions(useWic)
                : AppDispatcher.Invoke(() => CreateSystemExtensions(useWic));

            var list = new List<string>();
            foreach (var pair in collection)
            {
                list.AddRange(pair.Value.Split(','));
            }

            var defaultExtensions = new FileTypeCollection();
            defaultExtensions.Restore(list);
            Debug.WriteLine($"DefaultExtensions: {defaultExtensions}");

            return defaultExtensions;
        }

        // 標準対応拡張子取得
        private static Dictionary<string, string> CreateSystemExtensions(bool useWic)
        {
            var map = CreateDefaultExtensions();

            if (useWic)
            {
                try
                {
                    var wicMap =  WicDecoders.ListUp();
                    foreach(var pair in map)
                    {
                        if (!wicMap.ContainsKey(pair.Key))
                        {
                            wicMap.Add(pair.Key, pair.Value);
                        }
                    }
                    return wicMap;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WicDecoders.ListUp failed: {ex.Message}");
                }
            }

            return map;
        }

        private static Dictionary<string, string> CreateDefaultExtensions()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "BMP Decoder", ".bmp,.dib,.rle" },
                { "GIF Decoder", ".gif" },
                { "ICO Decoder", ".ico,.icon" },
                { "JPEG Decoder", ".jpeg,.jpe,.jpg,.jfif,.exif" },
                { "PNG Decoder", ".png" },
                { "TIFF Decoder", ".tiff,.tif" },
                { "WMPhoto Decoder", ".wdp,.jxr" },
                { "DDS Decoder", ".dds" },
                
                { "libwebp", ".webp" }
            };
            return dictionary;
        }
    }
}
