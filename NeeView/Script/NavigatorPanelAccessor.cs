namespace NeeView
{
    [WordNodeMember]
    public class NavigatorPanelAccessor : LayoutPanelAccessor
    {
        private readonly NavigatePanel _panel;


        public NavigatorPanelAccessor() : base(nameof(NavigatePanel))
        {
            _panel = (NavigatePanel)CustomLayoutPanelManager.Current.GetPanel(nameof(NavigatePanel));
        }

    }

}
