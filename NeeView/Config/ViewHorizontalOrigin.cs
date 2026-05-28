namespace NeeView
{
    public enum ViewHorizontalOrigin
    {
        Center,
        Left,
        Right,
        DirectionDependent,
        CenterOrLeft,
        CenterOrRight,
        CenterOrDirectionDependent,
    }

    public static class ViewHorizontalOriginExtensions
    {
        extension(ViewHorizontalOrigin origin)
        {
            public bool IsCenter => origin switch
            {
                ViewHorizontalOrigin.Center => true,
                ViewHorizontalOrigin.CenterOrLeft => true,
                ViewHorizontalOrigin.CenterOrRight => true,
                ViewHorizontalOrigin.CenterOrDirectionDependent => true,
                _ => false
            };
        }
    }
}
