#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


// [作業中]

namespace NeeView
{
    [LocalDebug]
    public partial class BookExporter
    {
        private readonly BookOperation _operation;
        private readonly Book _book;
        private bool _terminated;

        public BookExporter(BookOperation operation)
        {
            _operation = operation;
            _book = operation.Book ?? throw new InvalidOperationException("Book is null");
        }

        public async Task ProcessAsync(CancellationToken token)
        {
            LocalDebug.WriteLine("start...");

            // ブックをロック
            // ページ終端挙動を停止
            // :


            var parameter = new ExportImageParameter()
            {
                Mode = ExportImageMode.View,
                IsOriginalSize = true,
                OverwriteMode = ExportImageOverwriteMode.AddNumber,
                FileNameMode = ExportImageFileNameMode.BookPageNumber,
                FileFormat = BitmapImageFormat.Jpeg,
                ExportFolder = @"F:\Temp\Export"
            };

            var overwritePolicy = ExportImageOverwritePolicyFactory.Create(parameter.OverwriteMode);

            _terminated = false;
            _operation.Control.MoveToFirst(this);

            while (await ProcessPage(parameter, overwritePolicy, token))
            {
                _operation.Control.MoveNext(this);
            }

            // 復元
            // :

            LocalDebug.WriteLine("done.");
        }

        private async Task<bool> ProcessPage(ExportImageParameter parameter, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            if (_terminated) return false;

            // TODO: 出力設定ダイアログ表示
            // 画像出力ダイアログ + 出力先フォルダー or ZIPファイル
            // プレビューはどうしよう。先頭ページのみ？


            // 表示確定
            await _operation.WaitAsync(token);

            // 表示ページの取得
            var pages = _operation.ViewPages;
            LocalDebug.WriteLine("ViewPage:" + string.Join(',', pages.Select(e => e.Index.ToString())));

            // TODO: ページの処理
            // PageFrameBox? 

            // CreateViewExporter(PageFrameBoxPresenter.Current); ... PageFrameBoxPresenter にもたせてもいいな
            var source = ExportImageSourceFactory.Create();
            using var service = new ExportImageService(source, parameter);
            service.ThrowIfCannotExport();

            // TODO: BookExport でのファイル名前生成は自動でなければいけない
            var filename = service.CreateFileName(parameter.FileNameMode, parameter.FileFormat);
            var file = overwritePolicy.Resolve(service, filename);
            //return overwritePolicy.Resolve(exporter, filename);
            //var file = ExportImageProcedure.ValidateFileName(service, filename, ExportOverwritePolicyFactory.Create(parameter.OverwriteMode));
            FileMode fileMode = file.AllowOverwrite ? FileMode.Create : FileMode.CreateNew;

            // TODO: フォルダー or ZIP
            using (var stream = new FileStream(file.FilePath, fileMode, FileAccess.Write))
            {
                await service.ExportStreamAsync(stream, token);
                //await ExportImageProcedure.ExecuteAsync(stream, service, parameter, token);
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
