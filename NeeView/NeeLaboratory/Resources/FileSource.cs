using NeeView;
using System.IO;

namespace NeeLaboratory.Resources
{
    public class FileSource : IFileSource
    {
        private readonly string _path;

        public FileSource(string path)
        {
            _path = path;
        }

        public string Name => _path;

        public Stream Open()
        {
            return FileIO.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
