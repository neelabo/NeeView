using System.Windows;
using System.Windows.Input;

namespace NeeView.Windows
{
    public class WindowStateCommands
    {
        private readonly Window _window;

        public WindowStateCommands(Window window)
        {
            _window = window;
        }


        public void Bind()
        {
            _window.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindowCommand_Execute));
            _window.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindowCommand_Execute));
            _window.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindowCommand_Execute));
            _window.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindowCommand_Execute));
        }


        private void MinimizeWindowCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(_window);
        }

        private void RestoreWindowCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(_window);
        }

        private void MaximizeWindowCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(_window);
        }

        private void CloseWindowCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(_window);
        }
    }
}
