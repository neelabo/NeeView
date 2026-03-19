using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class DestinationFolderCollection : List<DestinationFolder>, IEquatable<DestinationFolderCollection>
    {
        public DestinationFolderCollection()
        {
        }

        public DestinationFolderCollection(IEnumerable<DestinationFolder> collection) : base(collection)
        {
        }

        public DestinationFolder CreateNew()
        {
            var item = new DestinationFolder();
            this.Add(item);
            return item;
        }

        internal bool IsValidIndex(int index)
        {
            return (0 <= index && index < this.Count);
        }

        #region Equtable

        public override bool Equals(object? obj)
        {
            return Equals(obj as DestinationFolderCollection);
        }

        public bool Equals(DestinationFolderCollection? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == this.GetType()
                && this.SequenceEqual(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion Equtable
    }
}
