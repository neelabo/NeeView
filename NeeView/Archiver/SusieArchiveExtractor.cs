using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class SusieArchiveExtractor : ArchiveExtractor
    {
        private readonly SusieArchive _archive;

        public SusieArchiveExtractor(SusieArchive archive) : base(archive)
        {
            _archive = archive;
        }

        protected override async ValueTask ExtractCore(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == _archive);
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);
            if (entry.IsDirectory) throw new ApplicationException("This entry is directory: " + entry.EntryName);

            await _archive.ExtractAsync(entry, exportFileName, isOverwrite, token);
        }
    }

}
