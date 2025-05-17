using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PlaylistArchiveEntry : ArchiveEntry
    {
        public PlaylistArchiveEntry(Archive archiver) : base(archiver)
        {
        }

        public required ArchiveEntry InnerEntry { get; init; }

        public override ArchiveEntry TargetArchiveEntry => InnerEntry;

        public override string PlacePath => InnerEntry.PlacePath;

        public override string SystemPath => InnerEntry.SystemPath;

        public override string? EntityPath => InnerEntry.EntityPath;

        public override bool IsArchive()
        {
            return InnerEntry.IsArchive();
        }

        public override async ValueTask<string?> RealizeAsync(ArchivePolicy archivePolicy, CancellationToken token)
        {
            return await InnerEntry.RealizeAsync(archivePolicy, token);
        }

    }
}

