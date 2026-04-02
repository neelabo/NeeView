#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [LocalDebug]
    public partial class ExportBook
    {
        private readonly BookOperation _operation;
        private readonly Book _book;
        private readonly IProgress<ProgressInfo> _progress;
        private bool _terminated;


        public ExportBook(BookOperation operation, IProgress<ProgressInfo> progress)
        {
            _operation = operation;
            _book = operation.Book ?? throw new InvalidOperationException("Book is null");
            _progress = progress;
        }


        public async Task RunAsync(ExportBookParameter parameter, bool showToast, CancellationToken token)
        {
            LocalDebug.WriteLine("start...");

            var currentPage = _operation.ViewPages.FirstOrDefault();
            var isPlayingSlideShow = SlideShow.Current.IsPlayingSlideShow;

            AppState.Instance.IsProcessingBook = true;

            // Progress の表示確定のため、少し待つ
            await Task.Delay(50);

            try
            {
                var overwritePolicy = ExportImageOverwritePolicyFactory.Create(parameter.OverwriteMode);

                if (parameter.OverwriteMode == ExportImageOverwriteMode.Confirm)
                {
                    throw new InvalidOperationException("Overwrite confirm mode is not supported for book export.");
                }

                switch (parameter.Mode)
                {
                    case ExportImageMode.Original:
                        await ExportOriginalAsync(parameter, parameter.BookType, overwritePolicy, token);
                        break;

                    case ExportImageMode.View:
                        await ExportViewAsync(parameter, parameter.BookType, overwritePolicy, token);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported export image mode: {parameter.Mode}");
                }

                if (showToast)
                {
                    var link = $"<a href=\"explorer://{parameter.ExportBookPath}\">{System.IO.Path.GetFileName(parameter.ExportBookPath)}</a>";
                    var toast = new Toast(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ExportImage.Message.Success"), link)) { IsXHtml = true };
                    ToastService.Current.Show(toast);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                var toast = new Toast(ex.Message, TextResources.GetString("Word.Error"), ToastIcon.Error);
                ToastService.Current.Show(toast);
            }
            finally
            {
                await AppDispatcher.InvokeAsync(() => _operation.JumpPage(this, currentPage));
                
                // TODO: Transform の復元

                AppState.Instance.IsProcessingBook = false;
            }

            LocalDebug.WriteLine("done.");
        }

        /// <summary>
        /// オリジナルのページをそのまま出力するモード
        /// </summary>
        private async Task ExportOriginalAsync(ExportBookParameter parameter, ExportBookType bookType, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (var writer = ExportImageWriterFactory.Create(bookType, parameter.ExportBookPath, true))
            {
                var pages = _operation.Book?.Pages;
                if (pages is null || pages.Count == 0)
                {
                    throw new ApplicationException("No pages to export.");
                }

                foreach (var page in pages)
                {
                    token.ThrowIfCancellationRequested();


                    var pageSource = new ExportPageSource(page);
                    using (var stream = await writer.OpenEntryAsync(pageSource, null, parameter, overwritePolicy, token))
                    {
                        _progress.Report(new ProgressInfo((double)(page.Index + 1) / pages.Count, writer.CurrentName));

                        try
                        {
                            using var inputStream = await page.ArchiveEntry.OpenEntryAsync(true, token);
                            inputStream.CopyTo(stream);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }

                writer.Close();
            }
        }

        /// <summary>
        /// 表示を出力するモード
        /// </summary>
        private async Task ExportViewAsync(ExportBookParameter parameter, ExportBookType bookType, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _terminated = false;

            await AppDispatcher.InvokeAsync(() => _operation.Control.MoveToFirst(this));

            using (var writer = ExportImageWriterFactory.Create(bookType, parameter.ExportBookPath, true))
            {
                while (await ExportViewPageAsync(writer, parameter, overwritePolicy, token))
                {
                    token.ThrowIfCancellationRequested();

                    await AppDispatcher.InvokeAsync(() => _operation.Control.MoveNext(this));
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

            var source = await AppDispatcher.InvokeAsync(() => ExportImageSourceFactory.Create());
            using var service = new ExportImageService(source, parameter);
            service.ThrowIfCannotExport();

            var pageSource = new ExportPageSource(service.Source.BookAddress, service.Source.Pages);
            using (var stream = await writer.OpenEntryAsync(pageSource, service, parameter, overwritePolicy, token))
            {
                var page = pages.First();
                var pageCount = _operation.Book?.Pages.Count ?? 1;
                _progress.Report(new ProgressInfo((double)(page.Index + 1) / pageCount, writer.CurrentName));

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
