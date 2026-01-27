using System.Windows.Controls;

namespace NeeView.PageFrames
{
    public class PageFrameContainerInitializer : IInitializable<PageFrameContainer>
    {
        private readonly Canvas _canvas;

        public PageFrameContainerInitializer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Initialize(PageFrameContainer item)
        {
            //item.Visibility = Visibility.Visible;
            _canvas.Children.Add(item);
        }

        public void Uninitialized(PageFrameContainer item)
        {
            _canvas.Children.Remove(item);
        }
    }
}


