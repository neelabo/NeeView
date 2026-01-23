//#define LOCAL_DEBUG

using System;

namespace NeeView
{
    public static class FolderConfigCollectionValidator
    {
        public static FolderConfigCollection.Memento Validate(this FolderConfigCollection.Memento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("FolderConfigCollection.Format must not be null.");

            return self;
        }
    }
}
