#if USE_WINRT
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：WinRT によるPDFアーカイバ
    /// </summary>
    public class PdfWinRTArchive : PdfArchive
    {
        public const string Identify = "WinRT";


        public PdfWinRTArchive(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public override string ToString()
        {
            return Identify;
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        protected override async ValueTask<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            var list = new List<ArchiveEntry>();

            // TODO: ウィンドウが非アクティブになるまではインスタンスを持ちづつけるようにする？要速度調査

            StorageFile file = await StorageFile.GetFileFromPathAsync(Path);
            var pdfDocument = await PdfDocument.LoadFromFileAsync(file);
            var creationTime = file.DateCreated;
            var modifiedTime = (await file.GetBasicPropertiesAsync()).DateModified;
            for (int id = 0; id < pdfDocument.PageCount; ++id)
            {
                token.ThrowIfCancellationRequested();

                list.Add(new ArchiveEntry(this)
                {
                    IsValid = true,
                    Id = id,
                    Instance = null,
                    RawEntryName = $"{id + 1:000}.png",
                    Length = 0,
                    CreationTime = creationTime.DateTime,
                    LastWriteTime = modifiedTime.DateTime,
                });
            }

            return list;
        }

        // エントリーのストリームを得る
        // PDFは画像化したものをストリームにして返す
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            var ms = new MemoryStream();
            using (var pdfPage = GetPageAsync(entry.Id).GetAwaiter().GetResult())
            {
                pdfPage.RenderToStreamAsync(ms.AsRandomAccessStream()).AsTask().GetAwaiter().GetResult();
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private async ValueTask<PdfPage> GetPageAsync(int page)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(Path);
            var pdfDocument = await PdfDocument.LoadFromFileAsync(file);
            return pdfDocument.GetPage((uint)page);
        }

        // サイズ取得
        public override Size GetSourceSize(ArchiveEntry entry)
        {
            using (var pdfPage = GetPageAsync(entry.Id).GetAwaiter().GetResult())
            {
                return SizeExtensions.FromFoundationSize(pdfPage.Size);
            }
        }

        // 標準サイズで取得
        public Size GetRenderSize(ArchiveEntry entry)
        {
            using (var pdfPage = GetPageAsync(entry.Id).GetAwaiter().GetResult())
            {
                return GetRenderSize(pdfPage);
            }
        }

        // 標準サイズで取得
        private static Size GetRenderSize(PdfPage pdfPage)
        {
            var size = SizeExtensions.FromFoundationSize(pdfPage.Size);
            if (PdfArchiveProfile.SizeLimitedRenderSize.IsContains(size))
            {
                size = size.Uniformed(PdfArchiveProfile.SizeLimitedRenderSize);
            }
            return size;
        }


        // ファイルとして出力
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            using (var pdfPage = GetPageAsync(entry.Id).GetAwaiter().GetResult())
            {
                using (var fs = File.Create(exportFileName))
                {
                    pdfPage.RenderToStreamAsync(fs.AsRandomAccessStream()).AsTask().GetAwaiter().GetResult();
                }
            }
        }

        // サイズを指定して画像を取得する
        private Stream CraeteBitmapAsStream(ArchiveEntry entry, Size size)
        {
            using (var pdfPage = GetPageAsync(entry.Id).GetAwaiter().GetResult())
            {
                var options = new PdfPageRenderOptions()
                {
                    DestinationHeight = (uint)size.Height,
                    DestinationWidth = (uint)size.Width,
                    // https://docs.microsoft.com/en-us/windows/win32/wic/-wic-guids-clsids
                    // CLSID_WICPngEncoder
                    //BitmapEncoderId = new Guid(0x27949969, 0x876a, 0x41d7, 0x94, 0x47, 0x56, 0x8f, 0x6a, 0x35, 0xa4, 0x6a),
                };
                var ms = new MemoryStream();
                pdfPage.RenderToStreamAsync(ms.AsRandomAccessStream(), options).AsTask().GetAwaiter().GetResult();
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        /// <summary>
        /// To Bitmap
        /// </summary>
        public override System.Drawing.Image CraeteBitmap(ArchiveEntry entry, Size size)
        {
            using var bitmapStream = CraeteBitmapAsStream(entry, size);
            return System.Drawing.Image.FromStream(bitmapStream);
        }

        /// <summary>
        /// To BitmapSource
        /// </summary>
        public override BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            using var bitmapStream = CraeteBitmapAsStream(entry, size);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.None;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = bitmapStream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// To BitmapData
        /// </summary>
        public override byte[] CreateBitmapData(ArchiveEntry entry, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality)
        {
            using var outStream = new MemoryStream();
            using var bitmapStream = CraeteBitmapAsStream(entry, size);

            var bitmap = BitmapFrame.Create(bitmapStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var encoder = CreateFormat(format, quality);
            encoder.Frames.Add(bitmap);
            encoder.Save(outStream);

            return outStream.ToArray();

            static BitmapEncoder CreateFormat(BitmapImageFormat format, int quality)
            {
                return format switch
                {
                    BitmapImageFormat.Png => new PngBitmapEncoder(),
                    _ => new JpegBitmapEncoder() { QualityLevel = quality },
                };
            }
        }
    }
}

#endif
