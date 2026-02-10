using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class DestinationFolder : ICloneable, IEquatable<DestinationFolder>
    {
        private string _name = "";
        private string _path = "";

        public DestinationFolder()
        {
        }

        public DestinationFolder(string name, string path)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }


        public string Name
        {
            get { return string.IsNullOrWhiteSpace(_name) ? LoosePath.GetFileName(_path) : _name; }
            set { _name = value; }
        }

        public string Path
        {
            get => _path;
            set => _path = value;
        }


        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(_path);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Equals(DestinationFolder? other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name && this.Path == other.Path)
                return true;
            else
                return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DestinationFolder);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.Ordinal) ^ Path.GetHashCode(StringComparison.Ordinal);
        }

        public async ValueTask CopyRawAsync(IEnumerable<string> paths, CancellationToken token)
        {
            if (!paths.Any()) return;

            if (!Directory.Exists(this.Path))
            {
                throw new DirectoryNotFoundException();
            }

            await FileIO.SHCopyToFolderAsync(paths, this.Path, token);
        }

        public async ValueTask MoveRawAsync(IEnumerable<string> paths, CancellationToken token)
        {
            if (!paths.Any()) return;

            if (!Directory.Exists(this.Path))
            {
                throw new DirectoryNotFoundException();
            }

            await FileIO.SHMoveToFolderAsync(paths, this.Path, token);
        }

        public async ValueTask CopyAsync(IEnumerable<string> paths, CancellationToken token)
        {
            var entries = await PathToArchiveEntry(paths, token);
            await CopyAsync(entries, token);
        }

        public async ValueTask CopyAsync(IEnumerable<ArchiveEntry> entries, CancellationToken token)
        {
            var items = await ArchiveEntryUtility.RealizeArchiveEntry(entries, token);
            await CopyRawAsync(items, token);
            GC.KeepAlive(entries);
        }

        public async ValueTask<bool> TryCopyAsync(IEnumerable<string> paths, CancellationToken token)
        {
            return await ExceptionHandling.WithToastAsync((token) => CopyAsync(paths.ToList(), token), TextResources.GetString("Message.CopyFailed"), token);
        }

        public async ValueTask<bool> TryCopyAsync(IEnumerable<ArchiveEntry> entries, CancellationToken token)
        {
            return await ExceptionHandling.WithToastAsync((token) => CopyAsync(entries.ToList(), token), TextResources.GetString("Message.CopyFailed"), token);
        }

        public async ValueTask<bool> TryMoveAsync(IEnumerable<string> paths, CancellationToken token)
        {
            return await ExceptionHandling.WithToastAsync((token) => MoveRawAsync(paths.ToList(), token), TextResources.GetString("Message.MoveFailed"), token);
        }

        private static async ValueTask<List<ArchiveEntry>> PathToArchiveEntry(IEnumerable<string> paths, CancellationToken token)
        {
            var entries = new List<ArchiveEntry>();
            foreach (var path in paths)
            {
                entries.Add(await ArchiveEntryUtility.CreateAsync(path, ArchiveHint.None, true, token));
            }
            return entries;
        }
    }
}
