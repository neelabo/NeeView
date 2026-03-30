#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// [作業中]

namespace NeeView
{
    [LocalDebug]
    public partial class ExportBook
    {
        private readonly BookOperation _operation;
        private readonly Book _book;
        private bool _terminated;


        public ExportBook(BookOperation operation)
        {
            _operation = operation;
            _book = operation.Book ?? throw new InvalidOperationException("Book is null");
        }


        public async Task RunAsync(bool showToast, CancellationToken token)
        {
            LocalDebug.WriteLine("start...");

            // ブックをロック
            // ページ終端挙動を停止
            // :

            // TODO: 出力設定ダイアログ表示
            // 画像出力ダイアログ + 出力先フォルダー or ZIPファイル
            // プレビューはどうしよう。先頭ページのみ？

            var parameter = new ExportImageParameter()
            {
                Mode = ExportImageMode.Original,
                IsOriginalSize = true,
                OverwriteMode = ExportImageOverwriteMode.AddNumber,
                FileNameMode = ExportImageFileNameMode.Original,
                FileFormat = BitmapImageFormat.Jpeg,
                ExportFolder = @"F:\Temp\Export"
            };

            parameter.ExportFolder = @"F:\Temp\Export\test.zip";
            var exportBookType = ExportBookType.Zip;

            var overwritePolicy = ExportImageOverwritePolicyFactory.Create(parameter.OverwriteMode);

            try
            {
                if (parameter.OverwriteMode == ExportImageOverwriteMode.Confirm)
                {
                    throw new InvalidOperationException("Overwrite confirm mode is not supported for book export.");
                }

                // TODO: Mode = ExportImageMode.Origin の場合、ページの表示自体不要。
                // TODO: ページが存在していないときの処理

                switch (parameter.Mode)
                {
                    case ExportImageMode.Original:
                        await ExportOriginalAsync(parameter, exportBookType, overwritePolicy, token);
                        break;

                    case ExportImageMode.View:
                        await ExportViewAsync(parameter, exportBookType, overwritePolicy, token);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported export image mode: {parameter.Mode}");
                }

                if (showToast)
                {
                    var toast = new Toast(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ExportImage.Message.Success"), parameter.ExportFolder));
                    ToastService.Current.Show(toast);
                }
            }
            catch (Exception ex)
            {
                var toast = new Toast(ex.Message, TextResources.GetString("Word.Error"), ToastIcon.Error);
                ToastService.Current.Show(toast);
            }

            // 表示ページ復元
            // :

            LocalDebug.WriteLine("done.");
        }

        private async Task ExportOriginalAsync(ExportImageParameter parameter, ExportBookType bookType, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            using (var writer = ExportImageWriterFactory.Create(bookType, parameter.ExportFolder, parameter.OverwriteMode == ExportImageOverwriteMode.Confirm))
            {
                var pages = _operation.Book?.Pages;
                if (pages is null || pages.Count == 0)
                {
                    throw new ApplicationException("No pages to export.");
                }

                // TODO: ProgressBar

                foreach (var page in pages)
                {
                    // TODO: ページが存在しないときの処理

                    var pageSource = new ExportPageSource(page);
                    using (var stream = await writer.OpenEntryAsync(pageSource, null,  parameter, overwritePolicy, token))
                    {
                        using var inputStream = await page.ArchiveEntry.OpenEntryAsync(true, token);
                        inputStream.CopyTo(stream);
                    }
                }

                writer.Close();
            }
        }



        private async Task ExportViewAsync(ExportImageParameter parameter, ExportBookType bookType, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            _terminated = false;
            _operation.Control.MoveToFirst(this);

            using (var writer = ExportImageWriterFactory.Create(bookType, parameter.ExportFolder, parameter.OverwriteMode == ExportImageOverwriteMode.Confirm))
            {
                // TODO: ProgressBar
                while (await ExportViewPageAsync(writer, parameter, overwritePolicy, token))
                {
                    _operation.Control.MoveNext(this);
                }
                writer.Close();
            }
        }

        private async Task<bool> ExportViewPageAsync(IExportImageWriter writer, ExportImageParameter parameter, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            if (_terminated) return false;

            // 表示確定
            await _operation.WaitAsync(token);

            // 表示ページの取得
            var pages = _operation.ViewPages;
            LocalDebug.WriteLine("ViewPage:" + string.Join(',', pages.Select(e => e.Index.ToString())));

            // TODO: ページの処理

            var source = ExportImageSourceFactory.Create();
            using var service = new ExportImageService(source, parameter);
            service.ThrowIfCannotExport();

            // TODO: 例外発生時の後処理。ファイルのクリーンアップなど。ExportBook全体での例外処理(通知)も必要。
            var pageSource = new ExportPageSource(service.Source.BookAddress, service.Source.Pages);
            using (var stream = await writer.OpenEntryAsync(pageSource, service, parameter, overwritePolicy, token))
            {
                // TODO: ページが存在していないときの処理
                await service.ExportStreamAsync(stream, token);
            }

            // 最終ページ？
            if (pages.Count == 0 || pages.Any(e => e == _book.Pages.Last()))
            {
                LocalDebug.WriteLine("End of book");
                _terminated = true;
                return false;
            }

            return true;
        }
    }

}
