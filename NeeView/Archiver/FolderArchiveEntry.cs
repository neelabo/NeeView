using System.IO;

namespace NeeView
{
    public class FolderArchiveEntry : ArchiveEntry
    {
        public FolderArchiveEntry(Archive archiver) : base(archiver)
        {
        }

        public string? Link { get; set; }

        public override string PlacePath => SystemPath;

        public override string? EntityPath => Link ?? SystemPath;

        public override bool IsFileSystem => true;

        public override bool IsShortcut => Link is not null || base.IsShortcut;

        public bool HasReparsePoint { get; init; }


        public override FileSystemInfo? ResolveLinkTarget()
        {
            if (!HasReparsePoint) return null;

            if (IsDirectory)
            {
                return Directory.ResolveLinkTarget(TargetPath, true);
            }
            else
            {
                return File.ResolveLinkTarget(TargetPath, true);
            }
        }
    }
}
