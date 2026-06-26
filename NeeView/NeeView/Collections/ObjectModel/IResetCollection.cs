using System.Collections.Generic;

namespace NeeView.Collections.ObjectModel
{
    public interface IResetCollection
    {
        public void Reset(IEnumerable<object> newItems);
    }

    public interface IResetCollrction<T>
    {
        public void Reset(IEnumerable<T> newItems);
    }

}
