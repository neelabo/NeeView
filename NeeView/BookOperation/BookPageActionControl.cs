﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using NeeView.PageFrames;

namespace NeeView
{
    /// <summary>
    /// 現在ページに対する操作
    /// </summary>
    public class BookPageActionControl : IBookPageActionControl
    {
        private readonly Book _book;
        private readonly IBookControl _bookControl;
        private readonly PageFrameBox _box;

        public BookPageActionControl(PageFrameBox box, IBookControl bookControl)
        {
            _box = box;
            _book = _box.Book;
            _bookControl = bookControl;
        }

        #region ページ削除

        // 現在表示しているページのファイル削除可能？
        public bool CanDeleteFile()
        {
            var page = _book?.CurrentPage;
            if (page is null) return false;
            return CanDeleteFile(new List<Page>() { page });
        }

        // 現在表示しているページのファイルを削除する
        public async Task DeleteFileAsync()
        {
            var page = _book?.CurrentPage;
            if (page is null) return;
            await DeleteFileAsync(new List<Page>() { page });
        }

        // 指定ページのファル削除可能？
        public bool CanDeleteFile(List<Page> pages)
        {
            if (!pages.Any()) return false;

            return Config.Current.System.IsFileWriteAccessEnabled && PageFileIO.CanDeletePage(pages);
        }

        // 指定ページのファイルを削除する
        public async Task DeleteFileAsync(List<Page> pages)
        {
            var isCompletely = pages.Any(e => !e.ArchiveEntry.Archiver.IsFileSystem);
            if (Config.Current.System.IsRemoveConfirmed || isCompletely)
            {
                var dialog = await PageFileIO.CreateDeleteConfirmDialog(pages, isCompletely);
                if (!dialog.ShowDialog().IsPossible)
                {
                    return;
                }
            }

            // ページ表示を停止
            _bookControl.DisposeViewContent(pages);
            await Task.Delay(16);

            try
            {
                await PageFileIO.DeletePageAsync(pages);
            }
            catch (Exception ex)
            {
                new MessageDialog($"{Properties.TextResources.GetString("Word.Cause")}: {ex.Message}", Properties.TextResources.GetString("FileDeleteErrorDialog.Title")).ShowDialog();
            }

            _bookControl.ReLoad();
        }

        #endregion ページ削除

        #region ページ出力

        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return _book?.CurrentPage != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            if (CanOpenFilePlace())
            {
                string? place = _book?.CurrentPage?.GetFolderOpenPlace();
                if (!string.IsNullOrWhiteSpace(place))
                {
                    ExternalProcess.OpenWithFileManager(place);
                }
            }
        }

        // 外部アプリで開く
        public void OpenApplication(OpenExternalAppCommandParameter parameter)
        {
            _ = OpenApplicationAsync(parameter, CancellationToken.None);
        }

        // 外部アプリで開く
        public async Task OpenApplicationAsync(OpenExternalAppCommandParameter parameter, CancellationToken token)
        {
            var book = this._book;
            if (book is null) return;

            if (CanOpenFilePlace())
            {
                try
                {
                    var external = new ExternalAppUtility();
                    var pages = CollectPages(book, parameter.MultiPagePolicy);
                    await external.CallAsync(pages, parameter, token);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    new MessageDialog(ex.Message, Properties.TextResources.GetString("OpenApplicationErrorDialog.Title")).ShowDialog();
                }
            }
        }

        private static List<Page> CollectPages(Book book, MultiPagePolicy policy)
        {
            if (book is null)
            {
                return new List<Page>();
            }

            var pages = book.CurrentPages.Distinct();

            switch (policy)
            {
                case MultiPagePolicy.Once:
                    pages = pages.Take(1);
                    break;

                case MultiPagePolicy.AllLeftToRight:
                    if (book.Setting.BookReadOrder == PageReadOrder.RightToLeft)
                    {
                        pages = pages.Reverse();
                    }
                    break;
            }

            return pages.ToList();
        }


        // クリップボードにコピー
        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            _ = CopyToClipboardAsync(parameter, CancellationToken.None);
        }

        // クリップボードにコピー
        public async Task CopyToClipboardAsync(CopyFileCommandParameter parameter, CancellationToken token)
        {
            var book = this._book;
            if (book is null) return;

            if (CanOpenFilePlace())
            {
                try
                {
                    var pages = CollectPages(book, parameter.MultiPagePolicy);
                    await ClipboardUtility.CopyAsync(pages, parameter, token);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Properties.TextResources.GetString("Word.Cause")}: {e.Message}", Properties.TextResources.GetString("CopyErrorDialog.Title")).ShowDialog();
                }
            }
        }

        /// <summary>
        /// ファイル保存可否
        /// </summary>
        /// <returns></returns>
        public bool CanExport()
        {
            return _box.GetSelectedPageFrameContent()?.ViewContents.FirstOrDefault() is IHasImageSource;
        }


        // ファイルに保存する (ダイアログ)
        // TODO: OutOfMemory対策
        // TODO: ダイアログにリソースを直接渡すようにする
        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    var exportImageProceduralDialog = new ExportImageProceduralDialog();
                    exportImageProceduralDialog.Owner = MainViewComponent.Current.GetWindow();
                    exportImageProceduralDialog.Show(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Properties.TextResources.GetString("ImageExportErrorDialog.Message")}\n{Properties.TextResources.GetString("Word.Cause")}: {e.Message}", Properties.TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
                    return;
                }
            }
        }

        // ファイルに保存する
        public void Export(ExportImageCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    ExportImageProcedure.Execute(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Properties.TextResources.GetString("ImageExportErrorDialog.Message")}\n{Properties.TextResources.GetString("Word.Cause")}: {e.Message}", Properties.TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
                    return;
                }
            }
        }

        #endregion ページ出力
    }
}
