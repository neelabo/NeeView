using NeeView.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.IO
{
    /// <summary>
    /// ファイルショートカット
    /// </summary>
    public class FileShortcut
    {
        private FileInfo _source;
        private FileSystemInfo? _target;


        public FileShortcut(string path)
        {
            Open(new FileInfo(path));
        }

        public FileShortcut(FileInfo source)
        {
            Open(source);
        }


        // リンク元ファイル
        public FileInfo Source => _source;
        public string SourcePath => _source.FullName;

        // リンク先ファイル
        public FileSystemInfo? Target => _target;
        public string? TargetPath => _target?.FullName;

        // 有効？
        [MemberNotNullWhen(true, nameof(Target), nameof(TargetPath))]
        public bool IsValid => _target != null && _target.Exists;



        public bool TryGetTarget([NotNullWhen(true)] out FileSystemInfo? target)
        {
            target = Target;
            return IsValid;
        }

        public bool TryGetTargetPath([NotNullWhen(true)] out string? targetPath)
        {
            targetPath = TargetPath;
            return IsValid;
        }


        public static bool IsShortcut(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (QuerySchemeExtensions.GetScheme(path) != QueryScheme.File)
            {
                return false;
            }

            try
            {
                return Path.GetExtension(path).ToLowerInvariant() == ".lnk";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public static string ResolveShortcutPath(string path)
        {
            if (IsShortcut(path))
            {
                var shortcut = new FileShortcut(path);
                if (shortcut.TryGetTargetPath(out var targetPath))
                {
                    return targetPath;
                }
            }
            return path;
        }

        [MemberNotNull(nameof(_source))]
        public void Open(FileInfo source)
        {
            if (!IsShortcut(source.FullName)) throw new NotSupportedException($"{source.FullName} is not link file.");

            _source = source;

            try
            {
                var targetPath = GetLinkTargetPath(source);
                if (string.IsNullOrEmpty(targetPath)) throw new FileNotFoundException($"Shortcut link target not found: {source.FullName}");

                var directoryInfo = new DirectoryInfo(targetPath);
                if (directoryInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    _target = directoryInfo;
                }
                else
                {
                    _target = new FileInfo(targetPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShortcutFileName: {source.FullName}\n{ex.Message}");
                _target = null;
            }
        }

        /// <summary>
        /// ショートカットのターゲットパスを取得
        /// </summary>
        /// <param name="linkFile">ショートカットファイル</param>
        /// <returns>ターゲットパス</returns>
        private static string GetLinkTargetPath(FileInfo linkFile)
        {
            if (linkFile is null)
            {
                throw new ArgumentNullException(nameof(linkFile));
            }
            if (!linkFile.Exists)
            {
                throw new FileNotFoundException();
            }

            var targetPath = new StringBuilder(1024);
            var isSuccess = NVInterop.NVGetFullPathFromShortcut(linkFile.FullName, targetPath);
            if (!isSuccess)
            {
                throw new IOException("IShellLink error.");
            }

            return targetPath.ToString();
        }

    }
}
