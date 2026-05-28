namespace NeeView
{
    public enum ViewVerticalOrigin
    {
        Center,
        Top,
        Bottom,
        DirectionDependent,
        CenterOrTop,
        CenterOrBottom,
        CenterOrDirectionDependent,
    }

    public static class ViewVerticalOriginExtensions
    {
        extension(ViewVerticalOrigin origin)
        {
            public bool IsCenter => origin switch
            {
                ViewVerticalOrigin.Center => true,
                ViewVerticalOrigin.CenterOrTop => true,
                ViewVerticalOrigin.CenterOrBottom => true,
                ViewVerticalOrigin.CenterOrDirectionDependent => true,
                _ => false
            };
        }
    }
}
