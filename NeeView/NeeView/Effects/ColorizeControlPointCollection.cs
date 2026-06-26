using NeeView.Collections.ObjectModel;
using System.Collections.Generic;

namespace NeeView.Effects
{

    public class ColorizeControlPointCollection : ObservableCollectionEx<ColorizeControlPoint>, IEditCollection<ColorizeControlPoint>, IEditCollection
    {
        public ColorizeControlPointCollection()
        {
        }

        public ColorizeControlPointCollection(IEnumerable<ColorizeControlPoint> collection) : base(collection)
        {
        }

        public bool CanDeleteItem(ColorizeControlPoint? item)
        {
            return Count > 2;
        }

        public void DeleteItem(ColorizeControlPoint? item)
        {
            if (item is null) return;

            Remove(item);
        }

        public bool CanNewItem(ColorizeControlPoint? item)
        {
            return Count < 10;
        }

        public void NewItem(ColorizeControlPoint? item)
        {
            if (item is null) return;

            var index = IndexOf(item);
            if (index < 0) return;

            var newItem = new ColorizeControlPoint() { Color = item.Color };
            Insert(index + 1, newItem);
        }

        void IEditCollection.DeleteItem(object? item)
        {
            DeleteItem(item as ColorizeControlPoint);
        }

        bool IEditCollection.CanDeleteItem(object? item)
        {
            return CanDeleteItem(item as ColorizeControlPoint);
        }

        void IEditCollection.NewItem(object? item)
        {
            NewItem(item as ColorizeControlPoint);
        }

        bool IEditCollection.CanNewItem(object? item)
        {
            return CanNewItem(item as ColorizeControlPoint);
        }
    }

}
