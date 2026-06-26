namespace NeeView.Collections.ObjectModel
{
    public interface IEditCollection
    {
        bool CanDeleteItem(object? item);
        void DeleteItem(object? item);
        bool CanNewItem(object? item);
        void NewItem(object? item);
    }

    public interface IEditCollection<T>
    {
        bool CanDeleteItem(T? item);
        void DeleteItem(T? item);
        bool CanNewItem(T? item);
        void NewItem(T? item);
    }
}
