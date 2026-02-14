using System;
using System.Collections.Generic;

namespace NeeLaboratory.IO
{
    public class FileTypeNameCache
    {
        private readonly static Lazy<FileTypeNameCache> _current = new();
        public static FileTypeNameCache Current => _current.Value;

        private readonly Dictionary<string, string> _cache = new();
        private string _directoryTypeName = "";

        public string GetExtensionTypeName(string extension)
        {
            var ext = extension.ToLowerInvariant().Trim();
            if (_cache.TryGetValue(ext, out var fileTypeName))
            {
                return fileTypeName; 
            }

            fileTypeName = FileSystem.GetExtensionTypeName(ext);
            if (string.IsNullOrEmpty(fileTypeName))
            {
                fileTypeName = (ext.ToUpperInvariant().TrimStart(['.']) + " File").Trim();
            }
            _cache[ext] = fileTypeName;
            return fileTypeName;
        }

        public string GetDirectoryTypeName()
        {
            if (string.IsNullOrEmpty(_directoryTypeName))
            {
                _directoryTypeName = FileSystem.GetDirectoryTypeName() ?? "Folder";
            }
            return _directoryTypeName;
        }
    }
}
