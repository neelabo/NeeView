using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;


namespace NeeView
{
    public class FileSystemBreadcrumbProfile : IBreadcrumbProfile
    {
        public List<BreadcrumbToken> GetChildren(string path, int index, CancellationToken token)
        {
            return Directory.GetDirectories(path).Select(e => System.IO.Path.GetFileName(e)).Select(e => new FileBreadcrumbToken(path, e, null)).ToList<BreadcrumbToken>();
        }

        public bool CanHasChild(string path, int index)
        {
            return true;
        }
    }
}
