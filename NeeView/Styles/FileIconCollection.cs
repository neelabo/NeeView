using NeeView.IO;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class FileIconCollection
    {
        static FileIconCollection() => Current = new FileIconCollection();
        public static FileIconCollection Current { get; }

        private struct Key : IEquatable<Key>
        {
            public Key(string filename, FileIconType iconType, bool allowJumbo)
            {
                FileName = filename;
                IconType = iconType;
                AllowJumbo = allowJumbo;
            }

            public string FileName { get; private set; }
            public FileIconType IconType { get; private set; }
            public bool AllowJumbo { get; private set; }

            #region IEquatable Support

            public override bool Equals(object? other)
            {
                if (other is Key key)
                {
                    return Equals(key);
                }
                return false;
            }

            public bool Equals(Key other)
            {
                return IconType == other.IconType && FileName == other.FileName && AllowJumbo == other.AllowJumbo;
            }

            public override int GetHashCode()
            {
                return FileName.GetHashCode(StringComparison.Ordinal) ^ IconType.GetHashCode();
            }

            public static bool operator ==(Key lhs, Key rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(Key lhs, Key rhs)
            {
                return !(lhs.Equals(rhs));
            }

            #endregion
        }

        private readonly Dictionary<Key, BitmapSourceCollection> _caches = new();


        public async ValueTask InitializeAsync()
        {
            await AppDispatcher.BeginInvoke(() => CreateDefaultFolderIcon());
        }


        public void Clear()
        {
            _caches.Clear();
        }

        public BitmapSourceCollection CreateDefaultFileIcon()
        {
            return CreateFileIcon("a.__dummy__", FileIconType.FileType, true, true);
        }

        public BitmapSourceCollection CreateDefaultFolderIcon()
        {
            return CreateFileIcon("__dummy__", FileIconType.DirectoryType, true, true);
        }

        public BitmapSourceCollection CreateFileIcon(string filename, FileIconType iconType, bool allowJumbo, bool useCache)
        {
            return CreateFileIconCollection(filename, iconType, allowJumbo, useCache);
        }

        private BitmapSourceCollection CreateFileIconCollection(string filename, FileIconType iconType, bool allowJumbo, bool useCache)
        {
            if (iconType == FileIconType.FileType)
            {
                filename = System.IO.Path.GetExtension(filename);
            }

            var key = new Key(filename, iconType, allowJumbo);
            if (useCache && _caches.TryGetValue(key, out BitmapSourceCollection? collection))
            {
                return collection;
            }

            ////var sw = Stopwatch.StartNew();
            try
            {
                var bitmaps = FileIcon.CreateIconCollection(filename, iconType, allowJumbo);
                collection = new BitmapSourceCollection(bitmaps);
                if (useCache && iconType == FileIconType.DirectoryType || iconType == FileIconType.FileType)
                {
                    _caches[key] = collection;
                }
                return collection;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new BitmapSourceCollection();
            }
            finally
            {
                ////sw.Stop();
                ////Debug.WriteLine($"FileIcon: {filename}: {sw.ElapsedMilliseconds}ms");
            }

        }
    }
}
