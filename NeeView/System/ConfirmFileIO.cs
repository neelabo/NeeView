using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;


namespace NeeView
{
    /// <summary>
    /// 確認ダイアログ経由でのファイル操作
    /// </summary>
    public static class ConfirmFileIO
    {
        /// <summary>
        /// ファイル削除、確認ダイアログ付き。１項目専用
        /// </summary>
        public static async ValueTask<bool> DeleteAsync(ArchiveEntry entry, string title, FrameworkElement? thumbnail)
        {
            return await DeleteAsync(new List<ArchiveEntry> { entry }, title, thumbnail);
        }

        /// <summary>
        /// ファイル削除、確認ダイアログ付き
        /// </summary>
        /// <param name="entries">削除ファイル</param>
        /// <param name="title">確認ダイアログタイトル。null で自動生成する</param>
        /// <param name="thumbnail">確認ダイアログサムネイル。null で自動生成する</param>
        /// <returns>削除成功/失敗</returns>
        public static async ValueTask<bool> DeleteAsync(List<ArchiveEntry> entries, string title, FrameworkElement? thumbnail)
        {
            if (entries.Count == 0) return false;

            var isCompletely = entries.Any(e => !e.IsFileSystem);
            if (Config.Current.System.IsRemoveConfirmed || isCompletely)
            {
                var dialog = CreateDeleteConfirmDialog(entries, title, thumbnail, isCompletely);
                var answer = dialog.ShowDialog();
                if (!answer.IsPossible)
                {
                    return false;
                }
            }

            try
            {
                return await ArchiveEntry.DeleteEntriesAsync(entries);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                new MessageDialog($"{Properties.TextResources.GetString("Word.Cause")}: {ex.Message}", Properties.TextResources.GetString("FileDeleteErrorDialog.Title")).ShowDialog();
                return false;
            }
        }


        /// <summary>
        /// 削除確認ダイアログを作成
        /// </summary>
        public static MessageDialog CreateDeleteConfirmDialog(List<ArchiveEntry> entries, string title, FrameworkElement? thumbnail, bool isCompletely)
        {
            Debug.Assert(entries.Any());

            isCompletely = isCompletely || entries.Any(e => !e.IsFileSystem);
            var content = CreateDeleteDialogContent(entries, thumbnail, isCompletely);
            title = title ?? GetDeleteDialogTitle(entries);
            var dialog = new MessageDialog(content, title);
            dialog.Commands.Add(UICommands.Delete);
            dialog.Commands.Add(UICommands.Cancel);
            return dialog;
        }

        /// <summary>
        /// 確認ダイアログ用コンテンツ作成
        /// </summary>
        private static FrameworkElement CreateDeleteDialogContent(List<ArchiveEntry> entries, FrameworkElement? thumbnail, bool isCompletely)
        {
            if (entries.Count == 1)
            {
                return CreateDeleteDialogContentSingle(entries[0], thumbnail, isCompletely);
            }
            else
            {
                return CreateDeleteDialogContentMulti(entries, isCompletely);
            }
        }

        /// <summary>
        /// 1ファイル用確認ダイアログコンテンツ
        /// </summary>
        private static FrameworkElement CreateDeleteDialogContentSingle(ArchiveEntry entry, FrameworkElement? thumbnail, bool isCompletely)
        {
            var dockPanel = new DockPanel();

            var message = new TextBlock();
            message.Inlines.Add(new Run(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("FileDeleteDialog.Message"), GetFilesTypeName(entry))));
            if (isCompletely)
            {
                message.Inlines.Add(new LineBreak());
                message.Inlines.Add(new Run(Properties.TextResources.GetString("FileDeleteMultiDialog.Message.Completely")) { FontWeight = FontWeights.Bold });
            }
            message.Margin = new Thickness(0, 0, 0, 10);
            DockPanel.SetDock(message, Dock.Top);
            dockPanel.Children.Add(message);

            if (thumbnail == null)
            {
                thumbnail = CreateFileVisual(entry.SystemPath);
            }

            thumbnail.Margin = new Thickness(0, 0, 10, 0);
            dockPanel.Children.Add(thumbnail);

            var textBlock = new TextBlock();
            textBlock.Text = entry.SystemPath;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = new Thickness(0, 0, 0, 2);
            dockPanel.Children.Add(textBlock);

            return dockPanel;
        }

        /// <summary>
        /// 複数ファイル用確認ダイアログコンテンツ
        /// </summary>
        private static FrameworkElement CreateDeleteDialogContentMulti(List<ArchiveEntry> entries, bool isCompletely)
        {
            var message = new TextBlock();
            message.Inlines.Add(new Run(Properties.TextResources.GetFormatString("FileDeleteMultiDialog.Message", entries.Count)));
            if (isCompletely)
            {
                message.Inlines.Add(new LineBreak());
                message.Inlines.Add(new Run(Properties.TextResources.GetString("FileDeleteMultiDialog.Message.Completely")) { FontWeight = FontWeights.Bold });
            }
            message.Margin = new Thickness(0, 10, 0, 10);
            DockPanel.SetDock(message, Dock.Top);

            return message;
        }

        /// <summary>
        /// ファイルからダイアログ用サムネイル作成
        /// </summary>
        private static Image CreateFileVisual(string path)
        {
            return new Image
            {
                SnapsToDevicePixels = true,
                Source = NeeLaboratory.IO.FileSystem.GetTypeIconSource(path, NeeLaboratory.IO.FileSystem.IconSize.Normal),
                Width = 32,
                Height = 32,
            };
        }

        /// <summary>
        /// ダイアログタイトル作成
        /// </summary>
        private static string GetDeleteDialogTitle(List<ArchiveEntry> entries)
        {
            return Properties.TextResources.GetString("FileDeleteDialog.Title");
        }

        private static string GetFilesTypeName(ArchiveEntry entry)
        {
            return entry.IsDirectory ? Properties.TextResources.GetString("Word.Folder") : Properties.TextResources.GetString("Word.File");
        }
    }
}
