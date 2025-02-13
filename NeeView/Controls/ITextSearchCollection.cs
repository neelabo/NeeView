namespace NeeView
{
    public interface ITextSearchCollection
    {
        int SelectedIndex { get; }
        int ItemsCount { get; }
        string? GetPrimaryText(int index);
        void NavigateToItem(int index);
    }
}
