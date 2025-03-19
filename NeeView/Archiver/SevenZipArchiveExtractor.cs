using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class SevenZipArchiveExtractor : ArchiveExtractor
    {
        private readonly SevenZipArchive _archive;

        public SevenZipArchiveExtractor(SevenZipArchive archive) : base(archive)
        {
            _archive = archive;
        }

        public override async Task ExtractAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == _archive);
            Debug.Assert(_archive.Initialized());
            Debug.Assert(!_archive.CanPreExtract(), "Pre-extract, so no direct extract.");

            if (_archive.IsDisposed) return;

            await base.ExtractAsync(entry, exportFileName, isOverwrite, token);
        }

        protected override async Task ExtractCore(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == _archive);

            _archive.Extract(entry, exportFileName, isOverwrite, token);

            await Task.CompletedTask;
        }
    }

}
