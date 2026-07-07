using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Content DI
    /// </summary>
    public interface IMessageDialogContentComponent
    {
        event EventHandler Decide;

        object Content { get; }

        void OnLoaded(object sender, RoutedEventArgs e);
    }
}
