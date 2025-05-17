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

        public override async ValueTask ExtractAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            if (_archive.IsDisposed) return;

            IDisposable? lockSection = null;
            try
            {
                lockSection = await _archive.AsyncLock.LockAsync(token);
                _rawArchive = ZipFile.Open(_archive.Path, ZipArchiveMode.Read, _encoding);
                await base.ExtractAsync(entry, exportFileName, isOverwrite, token);
            }
            finally
            {
                _rawArchive?.Dispose();
                lockSection?.Dispose();
            }
        }

        protected override async ValueTask ExtractCore(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == _archive);
            Debug.Assert(_rawArchive is not null);
            if (entry.IsDirectory) throw new ApplicationException("This entry is directory: " + entry.EntryName);
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            var rawEntry = _rawArchive.FindEntry(entry);
            if (rawEntry is null) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);
            
            rawEntry.Export(exportFileName, isOverwrite);
            _archive.WriteZoneIdentifier(exportFileName);

            await Task.CompletedTask;
        }
    }

}
