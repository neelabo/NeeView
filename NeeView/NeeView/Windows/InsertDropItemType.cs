namespace NeeView.Windows
{
    public enum InsertDropItemType
    {
        None = 0,
        Folder = (1 << 0),
        Item = (1 << 1),
        All = Folder | Item
    }
}
