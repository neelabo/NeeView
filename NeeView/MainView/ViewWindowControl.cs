namespace NeeView
{
    public class ViewWindowControl : IViewWindowControl
    {
        private readonly MainViewComponent _viewComponent;
        private readonly WindowStateController _windowStateController;

        public bool IsFullScreen => _windowStateController.IsFullScreen;
        public bool IsFullDesktop => _windowStateController.IsFullDesktop;

        public ViewWindowControl(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
            _windowStateController = new WindowStateController(MainWindow.Current);
        }

        public void ToggleTopmost(object? sender)
        {
            _windowStateController.ToggleTopmost(sender);
        }

        public void ToggleWindowMinimize(object? sender)
        {
            _windowStateController.ToggleMinimize(sender);
        }

        public void ToggleWindowMaximize(object? sender)
        {
            _windowStateController.ToggleMaximize(sender);
        }

        public void ToggleWindowFullScreen(object? sender)
        {
            _windowStateController.ToggleFullScreen(sender);
        }
        
        public void ToggleWindowFullDesktop(object? sender)
        {
            _windowStateController.ToggleFullDesktop(sender);
        }

        public void SetWindowFullScreen(object? sender, bool isFullScreen)
        {
            _windowStateController.SetFullScreen(sender, isFullScreen);
        }

        public void SetWindowFullDesktop(object? sender, bool isFullDesktop)
        {
            _windowStateController.SetFullDesktop(sender, isFullDesktop);
        }

        public void StretchWindow()
        {
            _viewComponent.MainView.StretchWindow();
        }
    }
}
