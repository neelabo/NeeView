using System.Windows;

namespace NeeView.PageFrames
{
    public interface ICenterTransform
    {
        Point GetSnapCenterPoint(Point pos);
    }
}
