namespace NeeView
{
    public enum PageType
    {
        Folder,
        Archive,
        File,
        Empty,
    }

    public static class PageTypeExtensions
    {
        public static bool IsFolder(this PageType pageType)
        {
            return pageType == PageType.Folder || pageType == PageType.Archive;
        }
    }
}
