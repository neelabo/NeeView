using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ZipArchiveExtractor : ArchiveExtractor
    {
        private readonly ZipArchive _archive;
        private readonly Encoding? _encoding;
        private System.IO.Compression.ZipArchive? _rawArchive;

        public ZipArchiveExtractor(ZipArchive archive, Encoding? encoding) : base(archive)
        {
            _archive = archive;
            _encoding = encoding;
        }

        public override async Task ExtractAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            if (_archive.IsDisposed) return;

            try
            {
                _rawArchive = ZipFile.Open(_archive.Path, ZipArchiveMode.Read, _encoding);
                await base.ExtractAsync(entry, exportFileName, isOverwrite, token);
            }
            finally
            {
                _rawArchive?.Dispose();
            }
        }

        protected override async Task ExtractCore(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == _archive);
            Debug.Assert(_rawArchive is not null);
            if (entry.IsDirectory) throw new ApplicationException("This entry is directory: " + entry.EntryName);
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            var rawEntry = _rawArchive.Entries[entry.Id];
            Debug.Assert(ZipArchive.IsValidEntry(entry, rawEntry));
            rawEntry.Export(exportFileName, isOverwrite);

            await Task.CompletedTask;
        }
    }

}
