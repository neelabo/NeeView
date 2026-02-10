using NeeView.Properties;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public static class ClipboardUtility
    {
        /// <summary>
        /// 実体化してDataObjectに追加 (ページ)
        /// </summary>
        public static async ValueTask<bool> TrySetDataAsync(DataObject data, IEnumerable<Page> pages, CancellationToken token)
        {
            return await TrySetDataAsync(data, pages.Select(e => e.ArchiveEntry), Config.Current.System, token);
        }

        /// <summary>
        ///  実体化してDataObjectに追加
        /// </summary>
        public static async ValueTask<bool> TrySetDataAsync(DataObject data, IEnumerable<ArchiveEntry> entries, ICopyPolicy policy, CancellationToken token)
        {
            try
            {
                return await SetDataAsync(data, entries, policy, token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, null, ToastIcon.Error));
                return false;
            }
        }

        /// <summary>
        /// ページのコピーデータ―を DataObject に登録する
        /// </summary>
        /// <param name="data">登録先データオブジェクト</param>
        /// <param name="pages">登録ページ</param>
        /// <param name="policy">登録方針</param>
        /// <param name="token"></param>
        /// <returns>登録成功/失敗</returns>
        private static async ValueTask<bool> SetDataAsync(DataObject data, IEnumerable<ArchiveEntry> entries, ICopyPolicy policy, CancellationToken token)
        {
            if (!entries.Any()) return false;

            // query path
            data.SetQueryPathCollection(entries.Select(x => new QueryPath(x.EntryFullName)));

            // realize file path
            var files = await ArchiveEntryUtility.RealizeArchiveEntry(entries, policy.ArchiveCopyPolicy, token);

            if (files.Count > 0)
            {
                data.SetData(DataFormats.FileDrop, files.ToArray());
            }

            // file path text
            if (policy.TextCopyPolicy != TextCopyPolicy.None)
            {
                var paths = (policy.ArchiveCopyPolicy == ArchivePolicy.SendExtractFile && policy.TextCopyPolicy == TextCopyPolicy.OriginalPath)
                    ? await ArchiveEntryUtility.RealizeArchiveEntry(entries, policy.ArchiveCopyPolicy, token)
                    : files;
                if (paths.Count > 0)
                {
                    data.SetData(DataFormats.UnicodeText, string.Join(System.Environment.NewLine, paths));
                }
            }

            return true;
        }

        /// <summary>
        /// クリップボードに画像をコピー
        /// </summary>
        public static void CopyImage(System.Windows.Media.Imaging.BitmapSource image)
        {
            Clipboard.SetImage(image);
        }

        /// <summary>
        /// クリップボードからペースト(テスト)
        /// </summary>
        [Conditional("DEBUG")]
        public static void Paste()
        {
            var data = Clipboard.GetDataObject(); // クリップボードからオブジェクトを取得する。
            if (data is not null && data.GetDataPresent(DataFormats.FileDrop)) // テキストデータかどうか確認する。
            {
                var files = (string[])data.GetData(DataFormats.FileDrop); // オブジェクトからテキストを取得する。
                Debug.WriteLine("=> " + files[0]);
            }
        }

        /// <summary>
        /// 実体化してクリップボードにコピー (ページ、例外ハンドル済)
        /// </summary>
        public static async ValueTask<bool> TryCopyAsync(IEnumerable<Page> pages, FrameworkElement? element, CancellationToken token)
        {
            return await ExceptionHandling.WithDialogAsync((token) => CopyAsync(pages.Select(e => e.ArchiveEntry).ToList(), token), TextResources.GetString("Message.CopyFailed"), element, token);
        }

        /// <summary>
        /// 実体化してクリップボードにコピー
        /// </summary>
        public static async ValueTask<bool> TryCopyAsync(IEnumerable<ArchiveEntry> entries, CancellationToken token)
        {
            return await ExceptionHandling.WithDialogAsync((token) => CopyAsync(entries.ToList(), token), TextResources.GetString("Message.CopyFailed"), token);
        }

        /// <summary>
        /// 実体化してクリップボードにコピー
        /// </summary>
        public static async ValueTask CopyAsync(IEnumerable<ArchiveEntry> entries, CancellationToken token)
        {
            var data = new DataObject();
            if (await SetDataAsync(data, entries, Config.Current.System, token))
            {
                Clipboard.SetDataObject(data);
                GC.KeepAlive(entries);
            }
        }

        /// <summary>
        /// クリップボードに切り取り (ブック、例外ハンドル済)
        /// </summary>
        public static async ValueTask<bool> TryCutAsync(IPendingBook book, CancellationToken token)
        {
            return await ExceptionHandling.WithDialogAsync((token) => CutAsync(book, token), TextResources.GetString("Message.CopyFailed"), token);
        }

        /// <summary>
        /// クリップボードに切り取り (ブック)
        /// </summary>
        public static async ValueTask CutAsync(IPendingBook book, CancellationToken token)
        {
            var entry = await ArchiveEntryUtility.CreateAsync(book.Path, ArchiveHint.None, true, token);
            var guid = await CutAsync([entry], token);
            if (guid != Guid.Empty)
            {
                PendingItemManager.Current.AddRange(guid, [book]);
            }
        }

        /// <summary>
        /// クリップボードに切り取り (ページ、例外ハンドル済)
        /// </summary>
        public static async ValueTask<bool> TryCutAsync(IEnumerable<Page> pages, CancellationToken token)
        {
            return await ExceptionHandling.WithDialogAsync((token) => CutAsync(pages, token), TextResources.GetString("Message.CopyFailed"), token);
        }

        /// <summary>
        /// クリップボードに切り取り (ページ)
        /// </summary>
        public static async ValueTask CutAsync(IEnumerable<Page> pages, CancellationToken token)
        {
            var entries = pages.Select(e => e.ArchiveEntry);
            var guid = await CutAsync(entries, token);
            if (guid != Guid.Empty)
            {
                PendingItemManager.Current.AddRange(guid, pages);
            }
        }

        /// <summary>
        /// クリップボードに切り取り
        /// </summary>
        public static async ValueTask<Guid> CutAsync(IEnumerable<ArchiveEntry> entries, CancellationToken token)
        {
            var data = new DataObject();

            // 実在するファイルのみを対象とする。テキストは処理しない
            var copyPolicy = new CopyPolicy(ArchivePolicy.None, TextCopyPolicy.None);
            if (await SetDataAsync(data, entries, copyPolicy, token))
            {
                var guid = Guid.NewGuid();
                data.SetGuid(guid);
                data.SetPreferredDropEffect(DragDropEffects.Move);
                Clipboard.SetDataObject(data);
                return guid;
            }
            else
            {
                return default;
            }
        }
    }
}
