namespace NeeView
{
    public interface INavigateControl
    {
        object SelectedItem { get; }
        void NavigateToItem(object item);
    }
}
