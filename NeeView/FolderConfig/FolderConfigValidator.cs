//#define LOCAL_DEBUG

using System;

namespace NeeView
{
    public static class FolderConfigValidator
    {
        public static FolderConfig.Memento Validate(this FolderConfig.Memento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("FolderConfig.Format must not be null.");

#if false
            // folder config ver 2.0.0
            if (self.Format.CompareTo(new FormatVersion(FolderConfig.FormatName, 2, 0, 0)) < 0)
            {
                throw new NotImplementedException();
            }
#endif

            self.Format = FolderConfig.Memento.FormatVersion;
            return self;
        }
    }

}
