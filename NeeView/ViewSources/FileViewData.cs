using System.Windows.Media;

namespace NeeView
{
    public class FileViewData
    {
        public FileViewData(FilePageData pageData, ImageSource? imageSource)
            : this(pageData.Entry, pageData.Icon, pageData.Message, imageSource)
        {
        }

        public FileViewData(ArchiveEntry entry, FilePageIcon icon, string? message, ImageSource? imageSource)
        {
            Entry = entry;
            Icon = icon;
            Message = message;
            ImageSource = imageSource;
        }

        public ArchiveEntry Entry { get; }
        public FilePageIcon Icon { get; }
        public string? Message { get; }
        public ImageSource? ImageSource { get; }
    }

}
