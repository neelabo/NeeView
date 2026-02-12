using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;




namespace NeeView
{
    public static class PictureFormat
    {
        /// <summary>
        /// 画像フォーマット判定
        /// <param name="buff">判定するデータ</param>
        /// <returns>対応拡張子群。対応できない場合は null</returns>
        /// </summary>
        public static string[]? GetSupportImageExtensions(byte[] buff)
        {
            var extensions = GetWebPSupportImageExtensions(buff)
                ?? GetDefaultSupportImageExtensions(buff)
                ?? GetSusieSupportImageExtensions(buff);

            return extensions;
        }

        /// <summary>
        /// 画像フォーマット判定(WebP)
        /// </summary>
        /// <param name="buff">判定するデータ</param>
        /// <returns>対応拡張子群。対応できない場合は null</returns>
        public static string[]? GetWebPSupportImageExtensions(byte[] buff)
        {
            try
            {
                using (var stream = new MemoryStream(buff))
                {
                    if (WebPDecoder.IsWebP(stream))
                    {
                        return [".webp"];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 画像フォーマット判定(標準)
        /// </summary>
        /// <param name="buff">判定するデータ</param>
        /// <returns>対応拡張子群。対応できない場合は null</returns>
        public static string[]? GetDefaultSupportImageExtensions(byte[] buff)
        {
            try
            {
                using (var stream = new MemoryStream(buff))
                {
                    var bitmap = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);
                    return bitmap.Decoder.CodecInfo.FileExtensions.ToLowerInvariant().Split(',', ';');
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 画像フォーマット判定(Susie)
        /// </summary>
        /// <param name="buff">判定するデータ</param>
        /// <returns>対応拡張子群。対応できない場合は null</returns>
        public static string[]? GetSusieSupportImageExtensions(byte[] buff)
        {
            try
            {
                if (!Config.Current.Susie.IsEnabled) return null;
                var accessor = SusiePluginManager.Current.GetImagePluginAccessor("dummy", buff, false);
                return accessor?.Plugin?.Extensions?.ToArray();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}


