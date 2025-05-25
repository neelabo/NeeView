using NeeLaboratory.Linq;
using NeeView.Interop;
using NeeView.IO;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

// TODO: UI要素の除外

namespace NeeView
{
    /// <summary>
    /// File I/O
    /// </summary>
    public static partial class FileIO
    {
        [GeneratedRegex(@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(\.|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex _unavailableFileNameRegex { get; }

        [GeneratedRegex(@"^(.+)\((\d+)\)$")]
        private static partial Regex _fileNumberRegex { get; }

        [GeneratedRegex(@"[/\\]")]
        private static partial Regex _separateRegex { get; }

        [GeneratedRegex(@"^[a-zA-Z]:\\?$")]
        private static partial Regex _driveRegex { get; }

        [GeneratedRegex(@"^[a-z]:")]
        private static partial Regex _lowerDriveLetterRegex { get; }

        [GeneratedRegex(@":$")]
        private static partial Regex _colonTerminalRegex { get; }


        public static event EventHandler<FileReplaceEventHander>? Replacing;
        public static event EventHandler<FileReplaceEventHander>? Replaced;


        /// <summary>
        /// ファイルかディレクトリの存在チェック
        /// </summary>
        public static bool ExistsPath(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// FileSystemInfoを取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileSystemInfo CreateFileSystemInfo(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists) return directoryInfo;
            else return new FileInfo(path);
        }

        /// <summary>
        /// ファイル上書きチェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isOverwrite"></param>
        /// <exception cref="IOException"></exception>
        public static void CheckOverwrite(string path, bool isOverwrite)
        {
            if (!isOverwrite && File.Exists(path)) throw new IOException($"File already exists: {path}");
        }

        /// <summary>
        /// ファイル上書き前処理
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isOverwrite"></param>
        /// <exception cref="IOException"></exception>
        public static void ReadyOverwrite(string path, bool isOverwrite)
        {
            if (File.Exists(path))
            {
                if (isOverwrite)
                {
                    File.Delete(path);
                }
                else
                {
                    throw new IOException($"File already exists: {path}");
                }
            }
        }

        /// <summary>
        /// パス名の正規化
        /// </summary>
        /// <remarks>
        /// パスの存在チェックを行うので重い処理です
        /// </remarks>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetNormalizedPath(string? source)
        {
            if (source == null) return "";

            // 区切り文字修正
            source = _separateRegex.Replace(source, "\\").TrimEnd('\\');

            // Chop long-path prefix
            if (source.StartsWith(@"\\?\")) source = source[4..];

            // ドライブレター修正
            source = _lowerDriveLetterRegex.Replace(source, m => m.Value.ToUpperInvariant());
            source = _colonTerminalRegex.Replace(source, ":\\");

            // フルパス
            source = Path.GetFullPath(source);

            if (FileIO.ExistsPath(source))
            {
                // 大文字・小文字をファイルシステム情報にあわせる
                return GetLongPathName(source);
            }
            else
            {
                // アーカイブパスの可能性あり。有効なパス部分のみ正規化
                var path = "";
                var parts = LoosePath.Split(source);
                foreach (var part in parts)
                {
                    path = LoosePath.Combine(path, part);
                    if (File.Exists(path))
                    {
                        path = GetLongPathName(path);
                        path = LoosePath.Combine(path, source[path.Length..]);
                        break;
                    }
                }
                return path;
            }
        }

        /// <summary>
        /// ロングパス名を取得して大文字・小文字をファイルシステム情報にあわせる
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static string GetLongPathName(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";

            var longPath = new StringBuilder(1024); // 上限は1024文字
            if (0 == NativeMethods.GetLongPathName(source, longPath, longPath.Capacity))
            {
                return source;
            }
            return longPath.ToString();
        }

        /// <summary>
        /// パスの衝突を連番をつけて回避
        /// </summary>
        public static string CreateUniquePath(string source)
        {
            if (!ExistsPath(source))
            {
                return source;
            }

            var path = source;

            bool isFile = File.Exists(path);
            var directory = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Cannot get parent directory");
            var filename = isFile ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);
            var extension = isFile ? Path.GetExtension(path) : "";
            int count = 1;

            var match = _fileNumberRegex.Match(filename);
            if (match.Success)
            {
                filename = match.Groups[1].Value.Trim();
                count = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            }

            do
            {
                path = Path.Combine(directory, $"{filename} ({++count}){extension}");
            }
            while (ExistsPath(path));

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
                return path2.StartsWith(path1, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return path1.StartsWith(path2, StringComparison.OrdinalIgnoreCase);
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

        /// <summary>
        /// ファイルロックチェック
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsFileLocked(FileInfo file, FileShare share = FileShare.None)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, share))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ファイルが読み込み可能になるまで待機
        /// </summary>
        /// <param name="file"></param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async ValueTask WaitFileReadableAsync(FileInfo file, TimeSpan timeout, CancellationToken token)
        {
            var time = new TimeSpan();
            var interval = TimeSpan.FromMilliseconds(500);
            while (IsFileLocked(file, FileShare.Read))
            {
                if (time > timeout) throw new TimeoutException();
                await Task.Delay(interval, token);
                time += interval;
            }
        }

        /// <summary>
        /// ディレクトリ確保
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string EnsureDirectory(string path)
        {
            var directoryPath = LoosePath.TrimDirectoryEnd(path);
            var dir = new DirectoryInfo(LoosePath.TrimDirectoryEnd(directoryPath));
            if (!dir.Exists)
            {
                dir.Create();
            }
            return directoryPath;
        }

        /// <summary>
        /// ブックを閉じてアーカイブを開放する。
        /// </summary>
        /// <remarks>
        /// ファイル操作前の処理用。
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        private static async ValueTask<CloseBookResult> CloseBookAsync(string path)
        {
            return await CloseBookAsync([path]);
        }

        private static async ValueTask<CloseBookResult> CloseBookAsync(IEnumerable<string> paths)
        {
            // 開いている本であるならば閉じる
            var result = await BookHubTools.CloseBookAsync(paths);

            // 全てのファイルロックをはずす
            await ArchiveManager.Current.UnlockAllArchivesAsync();

            return result;
        }

        // 開いている本のページの削除処理
        private static void ValidateBookPages(IEnumerable<string> paths)
        {
            var book = BookOperation.Current.Book;
            if (book is null) return;
            var pages = paths.Select(e => book.Pages.GetPageWithEntryFullName(e)).WhereNotNull();
            if (!pages.Any()) return;
            BookOperation.Current.ValidatePages(pages);
        }

        /// <summary>
        /// ドライブ表示名を取得
        /// </summary>
        /// <param name="s"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string? GetDriveDisplayName(string s)
        {
            try
            {
                if (s is not null && _driveRegex.IsMatch(s))
                {
                    var driveInfo = new DriveInfo(s);
                    return GetDriveLabel(driveInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        private static string GetDriveLabel(DriveInfo driveInfo)
        {
            var driveName = driveInfo.Name.TrimEnd('\\');
            var volumeLabel = driveInfo.DriveType.ToDisplayString();
            var driveLabel = $"{volumeLabel} ({driveName})";

            try
            {
                // NOTE: ドライブによってはこのプロパティの取得に時間がかかる
                var IsReady = driveInfo.IsReady;
                if (driveInfo.IsReady)
                {
                    volumeLabel = string.IsNullOrEmpty(driveInfo.VolumeLabel) ? driveInfo.DriveType.ToDisplayString() : driveInfo.VolumeLabel;
                    driveLabel = $"{volumeLabel} ({driveName})";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return driveLabel;
        }

        /// <summary>
        /// ファイルを置き換える。
        /// </summary>
        /// <remarks>
        /// Replacing イベントと Replaced イベントを発行する。
        /// </remarks>
        /// <param name="sourceFileName"></param>
        /// <param name="destinationFileName"></param>
        /// <param name="destinationBackupFileName"></param>
        public static void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName)
        {
            var args = new FileReplaceEventHander(sourceFileName, destinationFileName, destinationBackupFileName);
            Replacing?.Invoke(null, args);
            try
            {
                File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
            }
            finally
            {
                Replaced?.Invoke(null, args);
            }
        }

        #region Copy

        /// <summary>
        /// 非同期ファイルコピー
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <param name="isOverwrite"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async ValueTask CopyFileAsync(string sourceFileName, string destFileName, bool isOverwrite, bool createDirectory, CancellationToken token)
        {
            await Task.Run(() =>
            {
                if (createDirectory)
                {
                    var outputDir = System.IO.Path.GetDirectoryName(destFileName) ?? throw new IOException($"Illegal path: {destFileName}");
                    Directory.CreateDirectory(outputDir);
                }

                File.Copy(sourceFileName, destFileName, isOverwrite);
            }, token);
        }

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーにコピーする
        /// </summary>
        public static async ValueTask SHCopyToFolderAsync(IEnumerable<string> paths, string toDirectory, CancellationToken token)
        {
            await SHCopyToFolderAsync(WindowTools.GetWindowHandle(), paths, toDirectory, token);
        }

        public static async ValueTask SHCopyToFolderAsync(IntPtr hwnd, IEnumerable<string> paths, string toDirectory, CancellationToken token)
        {
            var directoryPath = EnsureDirectory(toDirectory);
            await Task.Run(() => ShellFileOperation.Copy(hwnd, paths, directoryPath), token);
        }

        public static async ValueTask SHCopyAsync(string source, string destination, CancellationToken token)
        {
            await SHCopyAsync(WindowTools.GetWindowHandle(), source, destination, token);
        }

        public static async ValueTask SHCopyAsync(IntPtr hwnd, string source, string destination, CancellationToken token)
        {
            var dest = LoosePath.TrimEnd(destination);

            // destination の終端がセパレート記号があるときはディレクトリ確定
            if (LoosePath.IsDirectoryEnd(destination))
            {
                EnsureDirectory(dest);
            }

            await Task.Run(() => ShellFileOperation.Copy(hwnd, [source], dest), token);
        }

        #endregion Copy

        #region Move

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーに移動する
        /// </summary>
        public static async ValueTask SHMoveToFolderAsync(IEnumerable<string> paths, string toDirectory, CancellationToken token)
        {
            await SHMoveToFolderAsync(WindowTools.GetWindowHandle(), paths, toDirectory, token);
        }

        public static async ValueTask SHMoveToFolderAsync(IntPtr hwnd, IEnumerable<string> paths, string toDirectory, CancellationToken token)
        {
            await CloseBookAsync(paths);
            var directoryPath = EnsureDirectory(toDirectory);
            await Task.Run(() => ShellFileOperation.Move(hwnd, paths, directoryPath), token);
            ValidateBookPages(paths);
        }

        public static async ValueTask SHMoveAsync(string source, string destination, CancellationToken token)
        {
            await SHMoveAsync(WindowTools.GetWindowHandle(), source, destination, token);
        }

        public static async ValueTask SHMoveAsync(IntPtr hwnd, string source, string destination, CancellationToken token)
        {
            var paths = new string[] { source };
            await CloseBookAsync(paths);
            await Task.Run(() => ShellFileOperation.Move(hwnd, paths, destination), token);
            ValidateBookPages(paths);
        }

        #endregion Move

        #region Delete

        // ファイル削除 (Direct)
        public static void DeleteFile(string filename)
        {
            new FileInfo(filename).Delete();
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public static async ValueTask DeleteAsync(string path)
        {
            await DeleteAsync(new List<string>() { path });
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public static async ValueTask DeleteAsync(IEnumerable<string> paths)
        {
            await CloseBookAsync(paths);
            ShellFileOperation.Delete(WindowTools.GetWindowHandle(), paths, Config.Current.System.IsRemoveWantNukeWarning);
        }

        #endregion Delete

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
                    var dialog = new MessageDialog(Properties.TextResources.GetString("FileRenameWrongDialog.Message"), Properties.TextResources.GetString("FileRenameErrorDialog.Title"));
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
                    var dialog = new MessageDialog($"{Properties.TextResources.GetString("FileRenameInvalidDialog.Message")}\n\n{invalids}", Properties.TextResources.GetString("FileRenameErrorDialog.Title"));
                    dialog.ShowDialog();
                }
                return null;
            }

            // ファイル名に使用できない
            var match = _unavailableFileNameRegex.Match(newName);
            if (match.Success)
            {
                if (showConfirmDialog)
                {
                    var dialog = new MessageDialog($"{Properties.TextResources.GetString("FileRenameWrongDeviceDialog.Message")}\n\n{match.Groups[1].Value.ToUpperInvariant()}", Properties.TextResources.GetString("FileRenameErrorDialog.Title"));
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
                if (string.Compare(srcExt, dstExt, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (showConfirmDialog)
                    {
                        var dialog = new MessageDialog(Properties.TextResources.GetString("FileRenameExtensionDialog.Message"), Properties.TextResources.GetString("FileRenameExtensionDialog.Title"));
                        dialog.Commands.Add(UICommands.Yes);
                        dialog.Commands.Add(UICommands.No);
                        var answer = dialog.ShowDialog();
                        if (answer.Command != UICommands.Yes)
                        {
                            return null;
                        }
                    }
                }
            }

            // 大文字小文字の変換は正常
            if (string.Compare(src, dst, StringComparison.OrdinalIgnoreCase) == 0)
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
                    var dialog = new MessageDialog(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("FileRenameConflictDialog.Message"), Path.GetFileName(dstBase), Path.GetFileName(dst)), Properties.TextResources.GetString("FileRenameConflictDialog.Title"));
                    dialog.Commands.Add(new UICommand("@Word.Rename"));
                    dialog.Commands.Add(UICommands.Cancel);
                    var answer = dialog.ShowDialog();
                    if (answer.Command != dialog.Commands[0])
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
        public static async ValueTask<bool> RenameAsync(string src, string dst, bool restoreBook)
        {
            var closeBookResult = await CloseBookAsync(src);

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
                    MessageDialogResult? answer = null;
                    AppDispatcher.Invoke(() =>
                    {
                        var retryConfirm = new MessageDialog($"{Properties.TextResources.GetString("FileRenameFailedDialog.Message")}\n\n{ex.Message}", Properties.TextResources.GetString("FileRenameFailedDialog.Title"));
                        retryConfirm.Commands.Add(UICommands.Retry);
                        retryConfirm.Commands.Add(UICommands.Cancel);
                        answer = retryConfirm.ShowDialog();
                    });
                    if (answer?.Command == UICommands.Retry)
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
            catch (IOException) when (string.Compare(src, dst, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // 大文字小文字の違いだけである場合はWIN32APIで処理する
                // .NET6 では不要？
                NativeMethods.MoveFile(src, dst);
            }
        }

        #endregion Rename
    }


    public class FileReplaceEventHander : EventArgs
    {
        public string SourceFileName { get; }
        public string DestinationFileName { get; }
        public string? DestinationBackupFileName { get; }
        public FileReplaceEventHander(string sourceFileName, string destinationFileName, string? destinationBackupFileName)
        {
            SourceFileName = sourceFileName;
            DestinationFileName = destinationFileName;
            DestinationBackupFileName = destinationBackupFileName;
        }
    }
}
