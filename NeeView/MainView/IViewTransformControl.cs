namespace NeeView
{
    public interface IViewTransformControl
    {
        void ResetContentSizeAndTransform();

        void ScaleUp(ViewScaleCommandParameter parameter);
        void ScaleUp(ScaleType scaleType, ViewScaleCommandParameter parameter);
        void ScaleDown(ViewScaleCommandParameter parameter);
        void ScaleDown(ScaleType scaleType, ViewScaleCommandParameter parameter);
        void Stretch(bool ignoreViewOrigin);

        void ViewRotateLeft(ViewRotateCommandParameter parameter);
        void ViewRotateRight(ViewRotateCommandParameter parameter);

        void FlipHorizontal(bool isFlip);
        void FlipVertical(bool isFlip);
        void ToggleFlipHorizontal();
        void ToggleFlipVertical();

        void ScrollUp(ViewScrollCommandParameter parameter);
        void ScrollDown(ViewScrollCommandParameter parameter);
        void ScrollLeft(ViewScrollCommandParameter parameter);
        void ScrollRight(ViewScrollCommandParameter parameter);
        void ScrollToPreset(ViewPresetScrollCommandParameter parameter);

        void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter);
        void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter);

        void NextScrollPage(object? sender, ScrollPageCommandParameter parameter);
        void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter);
    }
}
