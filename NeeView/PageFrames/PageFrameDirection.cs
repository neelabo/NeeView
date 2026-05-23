using System;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame 配置方向
    /// </summary>
    public enum PageFrameDirection
    {
        None,
        Left,
        Up,
        Right,
        Down,
    }


    public static class PageFrameDirectionExtension
    {
        public static PageFrameDirection Reverse(this PageFrameDirection self)
        {
            return self switch
            {
                PageFrameDirection.None => PageFrameDirection.None,
                PageFrameDirection.Left => PageFrameDirection.Right,
                PageFrameDirection.Up => PageFrameDirection.Down,
                PageFrameDirection.Right => PageFrameDirection.Left,
                PageFrameDirection.Down => PageFrameDirection.Up,
                _ => throw new NotSupportedException()
            };
        }

        public static PageFrameDirection Transpose(this PageFrameDirection self)
        {
            return self switch
            {
                PageFrameDirection.None => PageFrameDirection.None,
                PageFrameDirection.Left => PageFrameDirection.Up,
                PageFrameDirection.Up => PageFrameDirection.Left,
                PageFrameDirection.Right => PageFrameDirection.Down,
                PageFrameDirection.Down => PageFrameDirection.Right,
                _ => throw new NotSupportedException()
            };
        }
    }
}
