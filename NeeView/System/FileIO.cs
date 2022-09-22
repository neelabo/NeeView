﻿using NeeView.IO;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace NeeView
{
    /// <summary>
    /// File I/O
    /// </summary>
    public static class FileIO
    {
        private class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool MoveFile(string lpExistingFileName, string lpNewFileName);
        }



        /// <summary>
        /// ファイルかディレクトリの存在チェック
        /// </summary>
        public static bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// パスの衝突を連番をつけて回避
        /// </summary>
        public static string CreateUniquePath(string source)
        {
            if (!Exists(source))
            {
                return source;
            }

            var path = source;

            bool isFile = File.Exists(path);
            var directory = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Cannot get parent directory");
            var filename = isFile ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);
            var extension = isFile ? Path.GetExtension(path) : "";
            int count = 1;

            var regex = new Regex(@"^(.+)\((\d+)\)$");
            var match = regex.Match(filename);
            if (match.Success)
            {
                filename = match.Groups[1].Value.Trim();
                count = int.Parse(match.Groups[2].Value);
            }

            do
            {
                path = Path.Combine(directory, $"{filename} ({++count}){extension}");
            }
            while (Exists(path));

            return path;
        }

        /// <summary>
        /// ディレクトリが親子関係にあるかをチェック
        /// </summary>
        /// <returns></returns>
        public static bool IsSubDirectoryRelationship(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            if (dir1 == dir2) return true;

            var path1 = LoosePath.TrimDirectoryEnd(LoosePath.NormalizeSeparator(dir1.FullName)).ToUpperInvariant();
            var path2 = LoosePath.TrimDirectoryEnd(LoosePath.NormalizeSeparator(dir2.FullName)).ToUpperInvariant();
            if (path1.Length < path2.Length)
            {
                return path2.StartsWith(path1);
            }
            else
            {
                return path1.StartsWith(path2);
            }
        }

        /// <summary>
        /// DirectoryInfoの等価判定
        /// </summary>
        public static bool DirectoryEquals(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            if (dir1 == null && dir2 == null) return true;
            if (dir1 == null || dir2 == null) return false;

            var path1 = LoosePath.NormalizeSeparator(dir1.FullName).TrimEnd(LoosePath.Separators).ToUpperInvariant();
            var path2 = LoosePath.NormalizeSeparator(dir2.FullName).TrimEnd(LoosePath.Separators).ToUpperInvariant();
            return path1 == path2;
        }

        #region Copy

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーにコピーする
        /// </summary>
        public static void CopyToFolder(IEnumerable<string> froms, string toDirectory)
        {
            var toDirPath = LoosePath.TrimDirectoryEnd(toDirectory);

            var dir = new DirectoryInfo(toDirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            ShellFileOperation.Copy(App.Current.MainWindow, froms, toDirPath);
        }

        #endregion Copy

        #region Move

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーに移動する
        /// </summary>
        public static void MoveToFolder(IEnumerable<string> froms, string toDirectory)
        {

            var toDirPath = LoosePath.TrimDirectoryEnd(toDirectory);

            var dir = new DirectoryInfo(toDirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            ShellFileOperation.Move(App.Current.MainWindow, froms, toDirPath);
        }

        #endregion Move

        #region Remove

        // ファイル削除 (Direct)
        public static void RemoveFile(string filename)
        {
            new FileInfo(filename).Delete();
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public static async Task<bool> RemoveFileAsync(string path, string title, FrameworkElement? thumbnail)
        {
            if (Config.Current.System.IsRemoveConfirmed)
            {
                var content = CreateRemoveDialogContent(path, thumbnail);
                if (!ConfirmRemove(content, title ?? GetRemoveDialogTitle(path)))
                {
                    return false;
                }
            }

            return await RemoveCoreAsync(new List<string>() { path });
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public static async Task<bool> RemoveFileAsync(List<string> paths, string title)
        {
            if (paths is null || !paths.Any())
            {
                return false;
            }
            if (paths.Count == 1)
            {
                return await RemoveFileAsync(paths.First(), title, null);
            }
            if (Config.Current.System.IsRemoveConfirmed)
            {
                var content = CreateRemoveDialogContent(paths);
                if (!ConfirmRemove(content, title ?? GetRemoveDialogTitle(paths)))
                {
                    return false;
                }
            }

            return await RemoveCoreAsync(paths);
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
        /// 1ファイル用確認ダイアログコンテンツ
        /// </summary>
        private static FrameworkElement CreateRemoveDialogContent(string path, FrameworkElement? thumbnail)
        {
            var dockPanel = new DockPanel();

            var message = new TextBlock();
            message.Text = string.Format(Resources.FileDeleteDialog_Message, GetRemoveFilesTypeName(path));
            message.Margin = new Thickness(0, 0, 0, 10);
            DockPanel.SetDock(message, Dock.Top);
            dockPanel.Children.Add(message);

            if (thumbnail == null)
            {
                thumbnail = CreateFileVisual(path);
            }

            thumbnail.Margin = new Thickness(0, 0, 10, 0);
            dockPanel.Children.Add(thumbnail);

            var textblock = new TextBlock();
            textblock.Text = path;
            textblock.VerticalAlignment = VerticalAlignment.Bottom;
            textblock.TextWrapping = TextWrapping.Wrap;
            textblock.Margin = new Thickness(0, 0, 0, 2);
            dockPanel.Children.Add(textblock);

            return dockPanel;
        }

        private static string GetRemoveDialogTitle(string path)
        {
            return string.Format(Resources.FileDeleteDialog_Title, GetRemoveFilesTypeName(path));
        }

        private static string GetRemoveDialogTitle(List<string> paths)
        {
            return string.Format(Resources.FileDeleteDialog_Title, GetRemoveFilesTypeName(paths));
        }

        private static string GetRemoveFilesTypeName(string path)
        {
            bool isDirectory = System.IO.Directory.Exists(path);
            return isDirectory ? Resources.Word_Folder : Resources.Word_File;
        }

        private static string GetRemoveFilesTypeName(List<string> paths)
        {
            if (paths.Count == 1)
            {
                return GetRemoveFilesTypeName(paths.First());
            }

            bool isDirectory = paths.All(e => System.IO.Directory.Exists(e));
            return isDirectory ? Resources.Word_Folders : Resources.Word_Files;
        }

        /// <summary>
        /// 複数ファイル用確認ダイアログコンテンツ
        /// </summary>
        private static FrameworkElement CreateRemoveDialogContent(List<string> paths)
        {
            var message = new TextBlock();
            message.Text = string.Format(Resources.FileDeleteMultiDialog_Message, paths.Count);
            message.Margin = new Thickness(0, 10, 0, 10);
            DockPanel.SetDock(message, Dock.Top);

            return message;
        }

        /// <summary>
        /// 削除確認
        /// </summary>
        private static bool ConfirmRemove(FrameworkElement content, string title)
        {
            var dialog = new MessageDialog(content, title);
            dialog.Commands.Add(UICommands.Delete);
            dialog.Commands.Add(UICommands.Cancel);
            var answer = dialog.ShowDialog();

            return (answer == UICommands.Delete);
        }

        /// <summary>
        /// 削除メイン
        /// </summary>
        private static async Task<bool> RemoveCoreAsync(List<string> paths)
        {
            try
            {
                // 開いている本であるならば閉じる
                await BookHubTools.CloseBookAsync(paths);

                // 全てのファイルロックをはずす
                await ArchiverManager.Current.UnlockAllArchivesAsync();

                ShellFileOperation.Delete(Application.Current.MainWindow, paths, Config.Current.System.IsRemoveWantNukeWarning);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"{Resources.Word_Cause}: {ex.Message}", Resources.FileDeleteErrorDialog_Title);
                dialog.ShowDialog();
                return false;
            }
        }

        #endregion Remove

        #region Rename

        /// <summary>
        /// ファイル名に無効な文字が含まれているか
        /// </summary>
        public static bool ContainsInvalidFileNameChars(string newName)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            return invalidCharsIndex >= 0;
        }

        /// <summary>
        /// Rename用変更後ファイル名を生成
        /// </summary>
        public static string? CreateRenameDst(string sourcePath, string newName, bool showConfirmDialog)
        {
            if (sourcePath is null) throw new ArgumentNullException(nameof(sourcePath));
            if (newName is null) throw new ArgumentNullException(nameof(newName));

            newName = newName.Trim().TrimEnd(' ', '.');

            // ファイル名に使用できない
            if (string.IsNullOrWhiteSpace(newName))
            {
                if (showConfirmDialog)
                {
                    var dialog = new MessageDialog(Resources.FileRenameWrongDialog_Message, Resources.FileRenameErrorDialog_Title);
                    dialog.ShowDialog();
                }
                return null;
            }

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                if (showConfirmDialog)
                {
                    var invalids = string.Join(" ", newName.Where(e => invalidChars.Contains(e)).Distinct());
                    var dialog = new MessageDialog($"{Resources.FileRenameInvalidDialog_Message}\n\n{invalids}", Resources.FileRenameErrorDialog_Title);
                    dialog.ShowDialog();
                }
                return null;
            }

            // ファイル名に使用できない
            var match = new Regex(@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(\.|$)", RegexOptions.IgnoreCase).Match(newName);
            if (match.Success)
            {
                if (showConfirmDialog)
                {
                    var dialog = new MessageDialog($"{Resources.FileRenameWrongDeviceDialog_Message}\n\n{match.Groups[1].Value.ToUpper()}", Resources.FileRenameErrorDialog_Title);
                    dialog.ShowDialog();
                }
                return null;
            }

            string src = sourcePath;
            string folder = System.IO.Path.GetDirectoryName(src) ?? throw new InvalidOperationException("Cannot get parent directory");
            string dst = System.IO.Path.Combine(folder, newName);

            // 全く同じ名前なら処理不要
            if (src == dst) return null;

            // 拡張子変更確認
            if (!Directory.Exists(sourcePath))
            {
                var srcExt = System.IO.Path.GetExtension(src);
                var dstExt = System.IO.Path.GetExtension(dst);
                if (string.Compare(srcExt, dstExt, true) != 0)
                {
                    if (showConfirmDialog)
                    {
                        var dialog = new MessageDialog(Resources.FileRenameExtensionDialog_Message, Resources.FileRenameExtensionDialog_Title);
                        dialog.Commands.Add(UICommands.Yes);
                        dialog.Commands.Add(UICommands.No);
                        var answer = dialog.ShowDialog();
                        if (answer != UICommands.Yes)
                        {
                            return null;
                        }
                    }
                }
            }

            // 大文字小文字の変換は正常
            if (string.Compare(src, dst, true) == 0)
            {
                // nop.
            }

            // 重複ファイル名回避
            else if (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst))
            {
                string dstBase = dst;
                string dir = System.IO.Path.GetDirectoryName(dst) ?? throw new InvalidOperationException("Cannot get parent directory");
                string name = System.IO.Path.GetFileNameWithoutExtension(dst);
                string ext = System.IO.Path.GetExtension(dst);
                int count = 1;

                do
                {
                    dst = $"{dir}\\{name} ({++count}){ext}";
                }
                while (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst));

                // 確認
                if (showConfirmDialog)
                {
                    var dialog = new MessageDialog(string.Format(Resources.FileRenameConfrictDialog_Message, Path.GetFileName(dstBase), Path.GetFileName(dst)), Resources.FileRenameConfrictDialog_Title);
                    dialog.Commands.Add(new UICommand(Resources.Word_Rename));
                    dialog.Commands.Add(UICommands.Cancel);
                    var answer = dialog.ShowDialog();
                    if (answer != dialog.Commands[0])
                    {
                        return null;
                    }
                }
            }

            return dst;
        }

        /// <summary>
        /// ファイル名前変更。現在ブックにも反映させる
        /// </summary>
        public static async Task<bool> RenameAsync(string src, string dst, bool restoreBook)
        {
            // 現在の本ならば閉じる
            var closeBookResult = await BookHubTools.CloseBookAsync(src);

            // 全てのファイルロックをはずす
            await ArchiverManager.Current.UnlockAllArchivesAsync();

            // rename main
            var isSuccess = RenameRetry(src, dst);
            if (!isSuccess) return false;

            // 本を開き直す
            if (restoreBook && closeBookResult.IsClosed)
            {
                BookHubTools.RestoreBook(dst, src, closeBookResult.RequestLoadCount);
            }

            return true;
        }

        private static bool RenameRetry(string src, string dst)
        {
            while (true)
            {
                try
                {
                    RenameCore(src, dst);
                    return true;
                }
                catch (Exception ex)
                {
                    UICommand? answer = null;
                    AppDispatcher.Invoke(() =>
                    {
                        var retryConfirm = new MessageDialog($"{Resources.FileRenameFailedDialog_Message}\n\n{ex.Message}", Resources.FileRenameFailedDialog_Title);
                        retryConfirm.Commands.Add(UICommands.Retry);
                        retryConfirm.Commands.Add(UICommands.Cancel);
                        answer = retryConfirm.ShowDialog();
                    });
                    if (answer == UICommands.Retry)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        /// <summary>
        /// ファイル名変更
        /// </summary>
        /// <param name="src">変更前のパス</param>
        /// <param name="dst">変更後のパス</param>
        /// <exception cref="FileNotFoundException">srcファイルが見つかりません</exception>
        private static void RenameCore(string src, string dst)
        {
            try
            {
                if (System.IO.Directory.Exists(src))
                {
                    System.IO.Directory.Move(src, dst);
                }
                else if (System.IO.File.Exists(src))
                {
                    System.IO.File.Move(src, dst);
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (IOException) when (string.Compare(src, dst, true) == 0)
            {
                // 大文字小文字の違いだけである場合はWIN32APIで処理する
                // .NET6 では不要？
                NativeMethods.MoveFile(src, dst);
            }
        }

        #endregion Rename
    }

}
