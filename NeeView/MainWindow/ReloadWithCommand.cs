using System;
using System.Windows.Input;

namespace NeeView
{
    public class ReloadWithCommand : ICommand
    {
        public static ReloadWithCommand Command { get; } = new ReloadWithCommand();

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var archiver = parameter as ArchiverIdentifier;
            if (archiver == null) return;

            BookHub.Current.RequestReLoad(this, null, new ArchiveHint(archiver));
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

}
