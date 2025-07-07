namespace NeeView
{
    [WordNodeMember]
    public class EffectPanelAccessor : LayoutPanelAccessor
    {
        private readonly ImageEffectPanel _panel;


        public EffectPanelAccessor() : base(nameof(ImageEffectPanel))
        {
            _panel = (ImageEffectPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(ImageEffectPanel));
        }

    }

}
