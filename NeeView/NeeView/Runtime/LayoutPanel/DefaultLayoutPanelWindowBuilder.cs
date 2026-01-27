namespace NeeView.Runtime.LayoutPanel
{
    public class DefaultLayoutPanelWindowBuilder : ILayoutPanelWindowBuilder
    {
        public LayoutPanelWindow CreateWindow(LayoutPanelWindowManager manager, LayoutPanel layoutPanel)
        {
            var window = new LayoutPanelWindow();
            window.LayoutPanelWindowManager = manager;
            window.LayoutPanel = layoutPanel;
            window.Title = layoutPanel.Title;
            window.Content = layoutPanel.Content.Value;
            return window;
        }
    }

}
