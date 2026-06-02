namespace NeeView
{
    public interface IViewWindowControl
    {
        bool IsFullScreen { get; }
        bool IsFullDesktop { get; }

        void StretchWindow();
        void ToggleTopmost(object? sender);
        void ToggleWindowFullDesktop(object? sender);
        void SetWindowFullDesktop(object? sender, bool isFullDesktop);
        void ToggleWindowFullScreen(object? sender);
        void SetWindowFullScreen(object? sender, bool isFullScreen);
        void ToggleWindowMaximize(object? sender);
        void ToggleWindowMinimize(object? sender);
    }
}
