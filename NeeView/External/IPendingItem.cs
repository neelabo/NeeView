namespace NeeView
{
    public interface IPendingItem
    {
        int PendingCount { get; }
        void IncrementPendingCount();
        void DecrementPendingCount();
    }
}
