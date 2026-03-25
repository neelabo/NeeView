//#define LOCAL_DEBUG

using System;

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

            return self;
        }
    }
}
