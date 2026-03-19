using System;
using System.Collections.Generic;

namespace NeeView
{
    public class DragActionCollection : Dictionary<string, DragActionMemento>, ICloneable
    {
        public object Clone()
        {
            var clone = new DragActionCollection();
            foreach (var pair in this)
            {
                clone.Add(pair.Key, pair.Value.Clone());
            }
            return clone;
        }
    }

}
