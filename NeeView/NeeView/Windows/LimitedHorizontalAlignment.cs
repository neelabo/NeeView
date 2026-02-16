using System.Windows;

namespace NeeView.Windows
{
    public enum LimitedHorizontalAlignment
    {
        [AliasName("HorizontalAlignment.Left")]
        Left,

        [AliasName("HorizontalAlignment.Center")]
        Center,

        [AliasName("HorizontalAlignment.Right")]
        Right,
    }


    public static class LimitedHorizontalAlignmentExtensions
    {
        public static HorizontalAlignment ToHorizontalAlignment(this LimitedHorizontalAlignment self)
        {
            return self switch
            {
                LimitedHorizontalAlignment.Left => HorizontalAlignment.Left,
                LimitedHorizontalAlignment.Right => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Center
            };
        }

        public static LimitedHorizontalAlignment ToLimitedHorizontalAlignment(this HorizontalAlignment self)
        {
            return self switch
            {
                HorizontalAlignment.Left => LimitedHorizontalAlignment.Left,
                HorizontalAlignment.Right => LimitedHorizontalAlignment.Right,
                _ => LimitedHorizontalAlignment.Center
            };
        }
    }

}
