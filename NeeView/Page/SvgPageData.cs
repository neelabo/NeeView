using System.Windows.Media;

namespace NeeView
{
    public class SvgPageData : IHasRawData
    {
        public SvgPageData(DrawingGroup? drawingGroup)
        {
            DrawingGroup = drawingGroup;
        }

        public DrawingGroup? DrawingGroup { get; }

        public object? RawData => DrawingGroup;
    }
}
