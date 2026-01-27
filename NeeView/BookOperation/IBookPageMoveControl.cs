namespace NeeView
{
    /// <summary>
    /// ブックのページ移動操作
    /// </summary>
    public interface IBookPageMoveControl
    {
        void MovePrev(object? sender);
        void MoveNext(object? sender);

        void MovePrevOne(object? sender);
        void MoveNextOne(object? sender);

        void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter);
        void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter);

        void MoveTo(object? sender, int index);
        void MoveToRandom(object? sender);

        void MovePrevSize(object? sender, int size);
        void MoveNextSize(object? sender, int size);

        void MovePrevFolder(object? sender, bool isShowMessage);
        void MoveNextFolder(object? sender, bool isShowMessage);

        void MoveToFirst(object? sender);
        void MoveToLast(object? sender);
    }
}
