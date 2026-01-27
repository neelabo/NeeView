namespace NeeView
{
    public class ViewLoupeControl : IViewLoupeControl
    {
        private readonly MainViewComponent _viewComponent;

        public ViewLoupeControl(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
        }

        public void SetLoupeMode(bool isLoupeMode)
        {
            // NOTE: 有効にするのはマウスのルーペモードのみ
            if (isLoupeMode)
            {
                _viewComponent.MouseInput.SetState(MouseInputState.Loupe, false);
            }
            else
            {
                _viewComponent.LoupeContext.IsEnabled = false;
            }
        }

        public bool GetLoupeMode()
        {
            return _viewComponent.LoupeContext.IsEnabled;
        }

        public void ToggleLoupeMode()
        {
            SetLoupeMode(!GetLoupeMode());
        }

        public void LoupeZoomIn()
        {
            _viewComponent.LoupeContext.ZoomIn();
        }

        public void LoupeZoomOut()
        {
            _viewComponent.LoupeContext.ZoomOut();
        }

    }


}
