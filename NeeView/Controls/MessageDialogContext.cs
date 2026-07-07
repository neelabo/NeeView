using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public partial class MessageDialogContext : ObservableObject
    {
        public MessageDialogContext(string caption, string message, MessageDialogIcon icon = MessageDialogIcon.None)
        {
            Caption = caption;
            Icon = icon;
            Content = new TextBlock() { Text = message, TextWrapping = TextWrapping.Wrap };
        }

        public MessageDialogContext(string caption, FrameworkElement content, MessageDialogIcon icon = MessageDialogIcon.None)
        {
            Caption = caption;
            Icon = icon;
            Content = content;
        }

        public MessageDialogContext(string caption, IMessageDialogContentComponent component, MessageDialogIcon icon = MessageDialogIcon.None)
        {
            Caption = caption;
            Icon = icon;
            Content = component.Content;
            DialogComponent = component;
        }


        public event EventHandler<DialogContextDecidedEventArgs>? Decided;


        public string Caption { get; set; } = "";

        public object? Content { get; set; }

        public IMessageDialogContentComponent? DialogComponent { get; set; }

        public MessageDialogIcon Icon { get; set; } = MessageDialogIcon.None;

        public List<UICommand> Commands { get; private set; } = new();

        public int DefaultCommandIndex { get; set; }

        public int CancelCommandIndex { get; set; } = -1;

        public UICommand? ResultCommand { get; set; }


        public UICommand? GetDefaultCommand()
        {
            return (DefaultCommandIndex >= 0 && DefaultCommandIndex < Commands.Count) ? Commands[DefaultCommandIndex] : null;
        }

        public UICommand? GetCancelCommand()
        {
            return (CancelCommandIndex >= 0 && CancelCommandIndex < Commands.Count) ? Commands[CancelCommandIndex] : null;
        }

        public void BeginShow()
        {
            DialogComponent?.Decide += DialogComponent_Decide;
        }

        public void EndShow()
        {
            DialogComponent?.Decide -= DialogComponent_Decide;
        }

        private void DialogComponent_Decide(object? sender, EventArgs e)
        {
            Decide(true);
        }

        public void OnLoaded(object s, RoutedEventArgs e)
        {
            DialogComponent?.OnLoaded(s, e);
        }

        public void Decide(bool possible)
        {
            Decide(possible ? Commands.FirstOrDefault(e => e.IsPossible) : null);
        }

        public void Decide(UICommand? command)
        {
            ResultCommand = command;
            Decided?.Invoke(this, new(ResultCommand));
        }
    }


    public class DialogContextDecidedEventArgs : EventArgs
    {
        public DialogContextDecidedEventArgs(UICommand? command)
        {
            ResultCommand = command;
        }
        public UICommand? ResultCommand { get; }
    }
}
