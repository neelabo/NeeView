namespace NeeView
{
    // 開始時の基準座標
    public enum DragViewOrigin
    {
        None,

        Center, // コンテンツの中心

        LeftTop, // コンテンツの左上
        RightTop, // コンテンツの右上
        LeftBottom,
        RightBottom,
    }

    public static class ViewOriginExtensions
    {
        public static DragViewOrigin Reverse(this DragViewOrigin origin)
        {
            return origin switch
            {
                DragViewOrigin.LeftTop => DragViewOrigin.RightTop,
                DragViewOrigin.RightTop => DragViewOrigin.LeftTop,
                DragViewOrigin.LeftBottom => DragViewOrigin.RightBottom,
                DragViewOrigin.RightBottom => DragViewOrigin.LeftBottom,
                _ => origin,
            };
        }
    }
}
