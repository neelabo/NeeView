using NeeView.Properties;
using System;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class OpenPageExternalAppMenuFactory : ExternalAppMenuFactory
    {
        public OpenPageExternalAppMenuFactory(ICommandParameterFactory<ExternalApp> parameterFactory) : base(TextResources.GetString("OpenExternalAppAsCommand.Menu"), parameterFactory)
        {
        }

        protected override ICommand Command { get; } = new OpenExternalAppCommand();
        protected override Binding? EnabledBinding { get; } = new Binding(nameof(ViewPageBindingSource.AnyViewPages)) { Source = ViewPageBindingSource.Default };

        /// <summary>
        /// メインビュー用 外部アプリを開くコマンド
        /// </summary>
        private class OpenExternalAppCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (parameter is not ExternalAppParameter e) return false;
                return BookOperation.Current.Control.CanOpenApplication(e.ExternalApp, e.Option.MultiPagePolicy);
            }

            public void Execute(object? parameter)
            {
                if (parameter is not ExternalAppParameter e) return;
                BookOperation.Current.Control.OpenApplication(e.ExternalApp, e.Option.MultiPagePolicy);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
