using System.Windows;

namespace NeeView.PageFrames
{
    public interface ICenterTransform
    {
        Point GetSnapPoint(Point pos);
    }
}
