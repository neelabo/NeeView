using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// UIを伴うPlaylist操作
    /// </summary>
    public static class PlaylistService
    {
        private static CancellationTokenSource? _removeUnlinkedCancellationTokenSource;

        /// <summary>
        /// 無効なプレイリスト削除を行う一連のUI
        /// </summary>
        /// <remarks>
        /// 項目のリンク切れを復元、できなかった無効項目にはリンク切れフラグを立てる。
        /// 無効項目の削除確認ダイアログを表示、削除を実行。
        /// </remarks>
        /// <param name="playlist"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async ValueTask DeleteInvalidItemsAsync(Playlist playlist, CancellationToken token)
        {
            var jobOperation = ProcessJobEngine.Current.AddJob(Resolve);
            int unlinkedCount = await jobOperation.WaitAsync(token);

            playlist.DelaySave();

            if (unlinkedCount > 0)
            {
                var dialog = new MessageDialog(TextResources.GetFormatString("DeleteItemsDialog.Message", unlinkedCount), TextResources.GetString("DeleteInvalidPlaylistItemDialog.Title"));
                dialog.Commands.AddRange(UICommands.OKCancel);
                var result = dialog.ShowDialog();
                if (result.IsPossible)
                {
                    var mementos = playlist.RemoveWithRecoverable(playlist.CollectUnlinked());
                    ShowRestoreToast(playlist, mementos);
                }
            }
            else
            {
                var dialog = new MessageDialog(TextResources.GetString("NoDeleteItemsDialog.Message"), TextResources.GetString("NoDeleteInvalidPlaylistItemDialog.Title"));
                dialog.ShowDialog();
            }

            async ValueTask<int> Resolve(IProgress<ProgressContext>? progress, CancellationToken token)
            {
                _removeUnlinkedCancellationTokenSource?.Cancel();
                _removeUnlinkedCancellationTokenSource = new CancellationTokenSource();

                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _removeUnlinkedCancellationTokenSource.Token);
                return await playlist.ResolveUnlinkedAsync(progress, tokenSource.Token);
            }
        }

        /// <summary>
        /// 削除復元トースト表示
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="mementos"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ShowRestoreToast(Playlist playlist, IEnumerable<PlaylistItemMemento> mementos)
        {
            if (mementos == null) throw new ArgumentNullException(nameof(mementos));
            if (!mementos.Any()) return;

            var toast = new Toast(TextResources.GetFormatString("Playlist.DeleteItemsMessage", mementos.Count()), null, ToastIcon.Information, TextResources.GetString("Word.Restore"),
                () =>
                {
                    foreach (var memento in mementos)
                    {
                        playlist.Restore(mementos);
                    }
                });

            ToastService.Current.Show("Playlist", toast);
        }
    }
}

