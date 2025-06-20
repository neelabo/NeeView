using System;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// Represents a timestamp associated with a file, including its path and last write time.
    /// </summary>
    /// <remarks>This type is useful for tracking changes to a file by comparing its last write time.
    /// Instances of <see cref="FileStamp"/> are immutable, meaning their properties cannot be modified after
    /// initialization.</remarks>
    public record class FileStamp
    {
        public FileStamp() : this(string.Empty, default)
        {
        }

        public FileStamp(string path, DateTime lastWriteTime)
        {
            Path = path ?? "";
            LastWriteTime = lastWriteTime;
        }

        public string Path { get; init; }
        public DateTime LastWriteTime { get; init; }

        public static FileStamp Create(string path)
        {
            var lastWriteTime = File.Exists(path) ? File.GetLastWriteTime(path) : default;
            return new FileStamp(path, lastWriteTime);
        }

        public bool IsLatest()
        {
            if (string.IsNullOrEmpty(Path)) return true;
            return this == Create(Path);
        }
    }
}
