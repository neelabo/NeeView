using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace NeeView
{
    public class FileSystemBreadcrumbProfile : IBreadcrumbProfile
    {
        public List<string> GetDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path).Select(e => System.IO.Path.GetFileName(e)).ToList();
            }
            catch
            {
                return new();
            }
        }
    }

}
