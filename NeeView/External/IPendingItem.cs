namespace NeeView
{
    public interface IPendingItem
    {
        void IncrementPendingCount();
        void DecrementPendingCount();
    }
}
