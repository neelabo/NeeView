namespace NeeView.PageFrames
{
    public interface IDragTransformContextFactory
    {
        ContentDragTransformContext? CreateContentDragTransformContext(bool isPointContainer);
        ContentDragTransformContext? CreateContentDragTransformContext(PageFrameContainer container);
        LoupeDragTransformContext? CreateLoupeDragTransformContext();
    }
}
