namespace NeeView
{
    public class FilePageData
    {
        public FilePageData(ArchiveEntry entry, FilePageIcon icon, string? message)
        {
            Entry = entry;
            Icon = icon;
            Message = message;
        }

        public ArchiveEntry Entry { get; }
        public FilePageIcon Icon { get; }
        public string? Message { get; }
    }

}
