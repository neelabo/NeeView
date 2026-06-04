//#define LOCAL_DEBUG

using System;
using System.Linq;

namespace NeeView
{
    public static class FolderConfigCollectionValidator
    {
        public static FolderConfigCollectionMemento Validate(this FolderConfigCollectionMemento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("FolderConfigCollection.Format must not be null.");

            // v46.0
            if (self.Format.CompareTo(new FormatVersion(FolderConfigCollectionMemento.FormatName, VersionNumber.Ver46_Alpha1)) <= 0)
            {
                foreach (var folder in self.Folders)
                {
                    folder.Validate();
                }
            }

            // before v46.0-Alpha.5 
            if (self.Format.CompareTo(new FormatVersion(FolderConfigCollectionMemento.FormatName, VersionNumber.Ver46_Alpha5)) <= 0)
            {
                self.ValidateIsFolderRecursive();
            }

            return self;
        }

        /// <summary>
        /// IsFolderRecursive をサブフォルダーに継承する。
        /// この仕様変更の変換に限り IsFolderRecursive==false は無効とする。
        /// </summary>
        /// <param name="self"></param>
        private static void ValidateIsFolderRecursive(this FolderConfigCollectionMemento self)
        {
            foreach (var folder in self.Folders)
            {
                var currentRecursive = folder.Parameter?.IsFolderRecursive ?? false;
                var defaultRecursive = GetDefaultRecursive(folder.Place);

                if (folder.Parameter is null)
                {
                    continue;
                }

                if (currentRecursive == defaultRecursive || currentRecursive == false)
                {
                    folder.Parameter = folder.Parameter with { IsFolderRecursive = null };
                }
            }

            bool GetDefaultRecursive(string path)
            {
                var query = new QueryPath(path);

                while (true)
                {
                    query = query.GetParent();
                    if (string.IsNullOrEmpty(query.Path))
                    {
                        return false;
                    }

                    var place = query.SimplePath;
                    var config = self.Folders.FirstOrDefault(e => e.Place == place);
                    if (config?.Parameter is not null && config.Parameter.IsFolderRecursive == true)
                    {
                        return true;
                    }
                }
            }
        }

    }
}
