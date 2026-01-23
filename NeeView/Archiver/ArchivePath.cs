
using System;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// アーカイブパス
    /// </summary>
    public record class ArchivePath
    {
        public ArchivePath(string path, int systemPathLength)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));
            Debug.Assert(0 <= systemPathLength && systemPathLength < path.Length - 1);

            Path = path;
            SystemPathLength = systemPathLength == path.Length ? 0 : systemPathLength;

            if (IsArchivePath)
            {
                Debug.Assert(Path[SystemPathLength] == '\\');
            }
            else
            {
                Debug.Assert(SystemPathLength == 0);
            }
        }

        public string Path { get; }
        public int SystemPathLength { get; }
        public int EntryPathLength => Path.Length - SystemPathLength;
        public string SystemPath => SystemPathLength > 0 ? Path.Substring(0, SystemPathLength) : Path;
        public string EntryPath => SystemPathLength > 0 ? Path.Substring(SystemPathLength) : "";
        public ReadOnlySpan<char> SystemPathSpan => SystemPathLength > 0 ? Path.AsSpan(0, SystemPathLength) : Path.AsSpan();
        public ReadOnlySpan<char> EntryPathSpan => SystemPathLength > 0 ? Path.AsSpan(SystemPathLength) : default;
        public bool IsArchivePath => SystemPathLength > 0;


        public static ArchivePath Create(string path)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            if (File.Exists(path) || Directory.Exists(path))
            {
                return new ArchivePath(path, 0);
            }
            else
            {
                return new ArchivePath(path, GetSystemPathLength(path));
            }
        }

        public static int GetSystemPathLength(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return 0;

            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            ReadOnlySpan<char> pathSpan = path.AsSpan();

            // ルート部分（C:\ や \\Server\Share）を特定
            var root = System.IO.Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return 0;

            int lastValidEnd = root.Length;
            int currentPos = lastValidEnd;

            while (currentPos < pathSpan.Length)
            {
                int nextSeparatorOffset = pathSpan[currentPos..].IndexOf('\\');

                int currentSegmentEnd;
                if (nextSeparatorOffset == -1)
                {
                    currentSegmentEnd = pathSpan.Length;
                    currentPos = pathSpan.Length;
                }
                else
                {
                    currentSegmentEnd = currentPos + nextSeparatorOffset;
                    currentPos = currentSegmentEnd + 1;
                }

                ReadOnlySpan<char> currentSlice = pathSpan[..currentSegmentEnd];
                if (currentSlice.Length <= lastValidEnd) continue;

                string checkPath = currentSlice.ToString();

                if (Directory.Exists(checkPath))
                {
                    lastValidEnd = currentSegmentEnd;
                }
                else if (File.Exists(checkPath))
                {
                    return currentSegmentEnd;
                }
                else
                {
                    break;
                }
            }

            return 0;
        }

        /// <summary>
        /// アーカイブパスからシステムパスを取得する
        /// </summary>
        /// <param name="path">アーカイブパス</param>
        /// <param name="excludeUnc">UNCパスの場合は null を返す</param>
        /// <returns></returns>
        public static string? GetSystemPath(string path, bool excludeUnc = false)
        {
            if (excludeUnc && LoosePath.IsUnc(path))
            {
                return null;
            }

            return Create(path).SystemPath;
        }

    }
}

