using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{

    /// <summary>
    /// Bitmap生成
    /// </summary>
    public class BitmapFactory
    {
        private readonly MagicScalerBitmapFactory _magicScaler = new();

        /// <summary>
        /// stream から BitmapSource 生成
        /// </summary>
        /// <remarks>
        /// サイズ指定されている場合は MagicScaler を使う
        /// </remarks>
        /// <param name="stream">画像データストリーム</param>
        /// <param name="info">画像情報</param>
        /// <param name="size">画像サイズ。指定しない場合は Size.Empty</param>
        /// <param name="setting">画像生成パラメータ</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public BitmapSource CreateBitmapSource(Stream stream, BitmapInfo? info, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    var pos = stream.Position;
                    try
                    {
                        return _magicScaler.Create(stream, info, size, setting.ProcessImageSettings);
                    }
                    catch (Exception ex) when (ex is InvalidDataException || ex is NotSupportedException)
                    {
                        token.ThrowIfCancellationRequested();
                        Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                        stream.Position = pos;
                        return CreateBitmapSourceWithMagicScaler(stream, info, size, setting, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            return DefaultBitmapFactory.Create(stream, info, size, token);
        }

        /// <summary>
        /// Decode the image using standard functions, then process it with MagicScaler.
        /// </summary>
        /// <remarks>
        /// This is the processing for WIC that MagicScaler does not support.
        /// </remarks>
        private BitmapSource CreateBitmapSourceWithMagicScaler(Stream stream, BitmapInfo? info, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            // convert bitmap to TIFF stream
            using var ms = CreateTiffStream(stream, info, token);

            // convert TIFF stream to BitmapImage using MagicScaler
            return _magicScaler.Create(ms, info, size, setting.ProcessImageSettings);
        }

        /// <summary>
        /// stream から画像ファイルデータ生成
        /// </summary>
        /// <remarks>
        /// サイズ指定されている場合は MagicScaler を使う
        /// </remarks>
        /// <param name="stream">元画像データ</param>
        /// <param name="info">画像情報</param>
        /// <param name="outStream">出力画像データのストリーム</param>
        /// <param name="size">画像サイズ。指定しない場合は Size.Empty</param>
        /// <param name="format">出力画像フォーマット</param>
        /// <param name="quality">出力画像品質</param>
        /// <param name="setting">画像生成パラメータ</param>
        /// <param name="token"></param>
        public void CreateImage(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting, CancellationToken token)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    var pos = stream.Position;
                    try
                    {
                        _magicScaler.CreateImage(stream, info, outStream, size, format, quality, setting.ProcessImageSettings);
                    }
                    catch (Exception ex) when (ex is InvalidDataException || ex is NotSupportedException)
                    {
                        token.ThrowIfCancellationRequested();
                        Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                        stream.Position = pos;
                        CreateImageWithMagicScaler(stream, info, outStream, size, format, quality, setting, token);
                    }
                    return;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            DefaultBitmapFactory.CreateImage(stream, info, outStream, size, format, quality, token);
        }

        /// <summary>
        /// Decode the image using standard functions, then process it with MagicScaler.
        /// </summary>
        /// <remarks>
        /// This is the processing for WIC that MagicScaler does not support.
        /// </remarks>
        private void CreateImageWithMagicScaler(Stream stream, BitmapInfo? info, Stream outStream, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting, CancellationToken token)
        {
            // convert bitmap to TIFF stream
            using var ms = CreateTiffStream(stream, info, token);

            // convert TIFF stream to image file using MagicScaler
            _magicScaler.CreateImage(ms, info, outStream, size, format, quality, setting.ProcessImageSettings);
        }

        /// <summary>
        /// convert bitmap to TIFF stream
        /// </summary>
        private MemoryStream CreateTiffStream(Stream stream, BitmapInfo? info, CancellationToken token)
        {
            // convert stream to raw size BitmapImage using default decoder
            BitmapSource bitmap = DefaultBitmapFactory.Create(stream, info, Size.Empty, token);

            // convert bitmap to TIFF stream
            var encoder = new TiffBitmapEncoder() { Compression = TiffCompressOption.None };
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var ms = new MemoryStream();
            encoder.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }
    }

}
