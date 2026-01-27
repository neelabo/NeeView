using System.Windows.Controls;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrameContainer を配置する Canvas
    /// </summary>
    public class PageFrameContainerCanvas : Canvas
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameContainerCollection _containers;


        public PageFrameContainerCanvas(PageFrameContext context, PageFrameContainerCollection containers)
        {
            _context = context;
            _containers = containers;

            var containerInitializer = new PageFrameContainerInitializer(this);
            _containers.SetContainerInitializer(containerInitializer);

            // NOTE: 開発用フレームマーカー
#if false
            var rectangle = new Rectangle()
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.Red,
            };
            Canvas.SetZIndex(rectangle, -1);

            Children.Add(rectangle);

#endif

            // NOTE: 開発用に背景をチェック模様にして領域を可視化
#if false
            var grid = new Grid()
            {
                Width = 4096,
                Height = 4096,
                Background = CheckBackgroundBrush,
                Opacity = 0.75,
            };
            Canvas.SetLeft(grid, -2048);
            Canvas.SetTop(grid, -2048);
            this.Children.Insert(0, grid);
#endif
        }

    }
}


