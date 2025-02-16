using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    // コピー設定
    public static class ClipboardUtility
    {
        public static async Task CopyAsync(List<Page> pages, CancellationToken token)
        {
            var data = new DataObject();

            if (await SetDataAsync(data, pages, token))
            {
                Clipboard.SetDataObject(data);
            }
        }

        public static async Task<bool> SetDataAsync(DataObject data, List<Page> pages, CancellationToken token)
        {
            try
            {
                return await SetDataAsync(data, pages, Config.Current.System, token);
            }
            catch(OperationCanceledException)
            {
                return false;
            }
            catch(Exception ex)
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
        private static async Task<bool> SetDataAsync(DataObject data, List<Page> pages, ICopyPolicy policy, CancellationToken token)
        {
            if (pages.Count == 0) return false;

            // query path
            data.SetQueryPathCollection(pages.Select(x => new QueryPath(x.EntryFullName)));

            // realize file path
            var files = await PageUtility.CreateRealizedFilePathListAsync(pages, policy.ArchiveCopyPolicy, token);
            if (files.Count > 0)
            {
                data.SetData(DataFormats.FileDrop, files.ToArray());
            }

            // file path text
            if (policy.TextCopyPolicy != TextCopyPolicy.None)
            {
                var paths = (policy.ArchiveCopyPolicy == ArchivePolicy.SendExtractFile && policy.TextCopyPolicy == TextCopyPolicy.OriginalPath)
                    ? await PageUtility.CreateRealizedFilePathListAsync(pages, ArchivePolicy.SendArchivePath, token)
                    : files;
                if (paths.Count > 0)
                {
                    data.SetData(DataFormats.UnicodeText, string.Join(System.Environment.NewLine, paths));
                }
            }

            return true;
        }

        // クリップボードに画像をコピー
        public static void CopyImage(System.Windows.Media.Imaging.BitmapSource image)
        {
            Clipboard.SetImage(image);
        }

        // クリップボードからペースト(テスト)
        [Conditional("DEBUG")]
        public static void Paste()
        {
            var data = Clipboard.GetDataObject(); // クリップボードからオブジェクトを取得する。
            if (data.GetDataPresent(DataFormats.FileDrop)) // テキストデータかどうか確認する。
            {
                var files = (string[])data.GetData(DataFormats.FileDrop); // オブジェクトからテキストを取得する。
                Debug.WriteLine("=> " + files[0]);
            }
        }
    }
}
