namespace NeeView
{
    [WordNodeMember]
    public class InformationPanelAccessor : LayoutPanelAccessor
    {
        private readonly FileInformationPanel _panel;
        private readonly FileInformation _model;


        public InformationPanelAccessor() : base(nameof(FileInformationPanel))
        {
            _panel = (FileInformationPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(FileInformationPanel));
            _model = _panel.FileInformation;
        }

    }

}
