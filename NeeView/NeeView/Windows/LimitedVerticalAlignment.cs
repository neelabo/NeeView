using System.Windows;

namespace NeeView.Windows
{
    public enum LimitedVerticalAlignment
    {
        [AliasName("VerticalAlignment.Top")]
        Top,
        
        [AliasName("VerticalAlignment.Center")]
        Center,

        [AliasName("VerticalAlignment.Bottom")]
        Bottom,
    }


    public static class LimitedVerticalAlignmentExtensions
    {
        public static VerticalAlignment ToVerticalAlignment(this LimitedVerticalAlignment self)
        {
            return self switch
            {
                LimitedVerticalAlignment.Top => VerticalAlignment.Top,
                LimitedVerticalAlignment.Bottom => VerticalAlignment.Bottom,
                _ => VerticalAlignment.Center
            };
        }

        public static LimitedVerticalAlignment ToLimitedVerticalAlignment(this VerticalAlignment self)
        {
            return self switch
            {
                VerticalAlignment.Top => LimitedVerticalAlignment.Top,
                VerticalAlignment.Bottom => LimitedVerticalAlignment.Bottom,
                _ => LimitedVerticalAlignment.Center
            };
        }
    }
}
