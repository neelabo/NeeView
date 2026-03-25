using System;

namespace NeeView
{
    public static class QuickAccessCollectionValidator
    {
        public static QuickAccessCollectionMemento Validate(this QuickAccessCollectionMemento self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("QuickAccessCollection.Format must not be null.");

            return self;
        }
    }
}
