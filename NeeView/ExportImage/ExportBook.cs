#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// [作業中]

namespace NeeView
{
    public class ExportBookParameter : ExportImageParameter
    {
        private ExportBookType _bookType;
        private string _bookName = "";

        [PropertyMember]
        public ExportBookType BookType
        {
            get => _bookType;
            set => SetProperty(ref _bookType, value);
        }

        [PropertyMember]
        public string BookName
        {
            get => _bookName;
            set => SetProperty(ref _bookName, value);
        }
    }


    [LocalDebug]
    public partial class ExportBook
    {
        private readonly BookOperation _operation;
        private readonly Book _book;
        private bool _terminated;
        private IProgress<ProgressInfo> _progress;


        public ExportBook(BookOperation operation, IProgress<ProgressInfo> progress)
        {
            _operation = operation;
            _book = operation.Book ?? throw new InvalidOperationException("Book is null");
            _progress = progress;
        }


        public async Task RunAsync(ExportBookParameter parameter, bool showToast, CancellationToken token)
        {
            LocalDebug.WriteLine("start...");

            // TODO: v ページ終端挙動を停止
            // TODO: v ページリスト、スライダーの挙動を停止
            // TODO: ブックをロック (すべての操作、ドロップなどによる変更、スライドショー)
            // TODO: 動画ブックどうする？ ... 当面禁止。原理的には10秒単位の画像出力が可能

            var currentPage = _operation.ViewPages.FirstOrDefault();
            var isPlayingSlideShow = SlideShow.Current.IsPlayingSlideShow;

            AppState.Instance.IsProcessingBook = true;

            try
            {
                var overwritePolicy = ExportImageOverwritePolicyFactory.Create(parameter.OverwriteMode);

                if (parameter.OverwriteMode == ExportImageOverwriteMode.Confirm)
                {
                    throw new InvalidOperationException("Overwrite confirm mode is not supported for book export.");
                }

                // TODO: ページが存在していないときの処理

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
                    var toast = new Toast(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ExportImage.Message.Success"), parameter.ExportFolder));
                    ToastService.Current.Show(toast);
                }
            }
            catch (Exception ex)
            {
                var toast = new Toast(ex.Message, TextResources.GetString("Word.Error"), ToastIcon.Error);
                ToastService.Current.Show(toast);
            }
            finally
            {
                // いろいろ復元
                // TODO: Transform の復元
                
                AppDispatcher.Invoke(() => _operation.JumpPage(this, currentPage));

                AppState.Instance.IsProcessingBook = false;
            }

            LocalDebug.WriteLine("done.");
        }

        private async Task ExportOriginalAsync(ExportBookParameter parameter, ExportBookType bookType, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            using (var writer = ExportImageWriterFactory.Create(bookType, parameter.ExportFolder, true))
            {
                var pages = _operation.Book?.Pages;
                if (pages is null || pages.Count == 0)
                {
                    throw new ApplicationException("No pages to export.");
                }


                foreach (var page in pages)
                {
                    _progress.Report(new ProgressInfo((double)(page.Index + 1) / pages.Count, page.EntryLastName));

                    var pageSource = new ExportPageSource(page);
                    using (var stream = await writer.OpenEntryAsync(pageSource, null, parameter, overwritePolicy, token))
                    {
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



        private async Task ExportViewAsync(ExportBookParameter parameter, ExportBookType bookType, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            _terminated = false;

            AppDispatcher.Invoke(() => _operation.Control.MoveToFirst(this));
            Report();

            using (var writer = ExportImageWriterFactory.Create(bookType, parameter.ExportFolder, true))
            {
                while (await ExportViewPageAsync(writer, parameter, overwritePolicy, token))
                {
                    AppDispatcher.Invoke(() => _operation.Control.MoveNext(this));
                    Report();
                }
                writer.Close();
            }

            void Report()
            {
                var page = _operation.ViewPages.First();
                var pageCount = _operation.Book?.Pages.Count ?? 1;
                _progress.Report(new ProgressInfo((double)(page.Index + 1) / pageCount, page.EntryLastName));
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

            var source = AppDispatcher.Invoke(() => ExportImageSourceFactory.Create());
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
