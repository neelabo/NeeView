using NeeView.Drawing;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：PdfiumViewer によるPDFアーカイバ
    /// </summary>
    public class PdfPdfiumArchive : PdfArchive
    {
        public PdfPdfiumArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
        }


        public override string ToString()
        {
            return Properties.TextResources.GetString("Archiver.Pdfium");
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        protected override async ValueTask<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            var list = new List<ArchiveEntry>();

            // TODO: ウィンドウが非アクティブになるまではインスタンスを持ちづつけるようにする？要速度調査

            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                var information = pdfDocument.GetInformation();

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
                        CreationTime = information.CreationDate ?? default,
                        LastWriteTime = information.ModificationDate ?? default,
                    });
                }
            }

            await Task.CompletedTask;
            return list;
        }

        // エントリーのストリームを得る
        // PDFは画像化したものをストリームにして返す
        protected override async ValueTask<Stream> OpenStreamInnerAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false); // TODO: async

                var ms = new MemoryStream();
                token.ThrowIfCancellationRequested();
                await Task.Run(() => image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp));
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        // サイズ取得
        public override Size GetSourceSize(ArchiveEntry entry)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                return GetSourceSize(pdfDocument, entry.Id);
            }
        }

        // サイズ取得
        private static Size GetSourceSize(PdfDocument pdfDocument, int page)
        {
            return SizeExtensions.FromDrawingSize(pdfDocument.PageSizes[page]);
        }

        // 標準サイズで取得
        public Size GetRenderSize(ArchiveEntry entry)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                return GetRenderSize(pdfDocument, entry.Id);
            }
        }

        // 標準サイズで取得
        private static Size GetRenderSize(PdfDocument pdfDocument, int page)
        {
            var size = SizeExtensions.FromDrawingSize(pdfDocument.PageSizes[page]);
            if (PdfArchiveProfile.SizeLimitedRenderSize.IsContains(size))
            {
                size = size.Uniformed(PdfArchiveProfile.SizeLimitedRenderSize);
            }
            return size;
        }


        // ファイルとして出力
        protected override async ValueTask ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            // 上書き時は移動前に削除
            if (isOverwrite && File.Exists(exportFileName))
            {
                File.Delete(exportFileName);
            }

            // ensure directory
            var outputDir = System.IO.Path.GetDirectoryName(exportFileName) ?? throw new IOException($"Illegal path: {exportFileName}");
            Directory.CreateDirectory(outputDir);

            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                var size = GetRenderSize(pdfDocument, entry.Id);
                var image = pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false); // TODO: async
                token.ThrowIfCancellationRequested();
                await Task.Run(() => image.Save(exportFileName, System.Drawing.Imaging.ImageFormat.Png));
            }
        }

        /// <summary>
        /// To Bitmap
        /// </summary>
        public override System.Drawing.Image CreateBitmap(ArchiveEntry entry, Size size)
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var pdfDocument = PdfDocument.Load(stream))
            {
                return pdfDocument.Render(entry.Id, (int)size.Width, (int)size.Height, 96, 96, false);
            }
        }

        /// <summary>
        /// To BitmapSource
        /// </summary>
        public override BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            return CreateBitmap(entry, size).ToBitmapSource() ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// To BitmapData
        /// </summary>
        public override byte[] CreateBitmapData(ArchiveEntry entry, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality)
        {
            using (var outStream = new MemoryStream())
            {
                CreateBitmap(entry, size).SaveWithQuality(outStream, CreateFormat(format), quality);
                return outStream.ToArray();
            }

            static System.Drawing.Imaging.ImageFormat CreateFormat(BitmapImageFormat format)
            {
                return format switch
                {
                    BitmapImageFormat.Png => System.Drawing.Imaging.ImageFormat.Png,
                    _ => System.Drawing.Imaging.ImageFormat.Jpeg,
                };
            }
        }
    }
}
