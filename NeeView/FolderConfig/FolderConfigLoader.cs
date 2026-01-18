//#define LOCAL_DEBUG

namespace NeeView
{
    public abstract class FolderConfigLoader
    {
        public abstract string? GetPlaceFromBookPath(string bookPath);
        public abstract FolderConfig LoadFolderConfig(string place);
        public abstract void SaveFolderConfig(FolderConfig config);
        public abstract void RenameRecursive(string src, string dst);
        public virtual void ClearCache() { }
    }

}
