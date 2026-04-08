using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class ExternalAppCollection : List<ExternalApp>, IEquatable<ExternalAppCollection>
    {
        public ExternalAppCollection()
        {
        }

        public ExternalAppCollection(IEnumerable<ExternalApp> collection) : base(collection)
        {
        }

        public ExternalApp CreateNew()
        {
            var item = new ExternalApp();
            this.Add(item);
            return item;
        }

        public bool IsValidIndex(int index)
        {
            return 0 <= index && index < this.Count;
        }

        #region Equtable

        public override bool Equals(object? obj)
        {
            return Equals(obj as ExternalAppCollection);
        }

        public bool Equals(ExternalAppCollection? other)
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

        public void ValidatePlaceholder()
        {
            foreach (var item in this)
            {
                item.ValidatePlaceholder();
            }
        }
    }


}
