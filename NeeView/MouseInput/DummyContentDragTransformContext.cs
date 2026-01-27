using NeeView.PageFrames;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 表示座標系操作のリソース : 表示がない時用。ウィンドウ移動などを機能させるため。
    /// </summary>
    public class DummyContentDragTransformContext : ContentDragTransformContext
    {
        public DummyContentDragTransformContext(FrameworkElement sender, ITransformControl transform, ViewConfig viewConfig, MouseConfig mouseConfig)
            : base(sender, transform, viewConfig, mouseConfig)
        {
            ContentRect = new Rect(-sender.ActualWidth * 0.5, -sender.ActualHeight * 0.5, sender.ActualWidth, sender.ActualHeight);
        }
    }

}
