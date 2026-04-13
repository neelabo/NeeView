using NeeLaboratory.Generators;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// Standard MessageDialog
    /// </summary>
    [NotifyPropertyChanged]
    public partial class MessageDialog : Window, INotifyPropertyChanged
    {
        public readonly static RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(MessageDialog), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.C, ModifierKeys.Control) }));
        public static Window? OwnerWindow { get; set; }

        public static bool IsShowInTaskBar { get; set; } = true;


        private RelayCommand<UICommand>? _buttonClickedCommand;

        private UICommand? _resultCommand;

        private bool _isClosing;


        public MessageDialog()
        {
            InitializeComponent();

            this.DataContext = this;

            this.Owner = OwnerWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.ShowInTaskbar = IsShowInTaskBar || OwnerWindow is null;

            this.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Execute));
        }

        public MessageDialog(string caption, MessageDialogIcon icon = MessageDialogIcon.None) : this()
        {
            this.Caption.Text = caption;
            SetIcon(icon);
        }

        public MessageDialog(string caption, string message, MessageDialogIcon icon = MessageDialogIcon.None) : this(caption, icon)
        {
            this.Message.Content = CreateTextContent(message);
        }

        public MessageDialog(string caption, FrameworkElement content, MessageDialogIcon icon = MessageDialogIcon.None) : this(caption, icon)
        {
            this.Message.Content = content;
        }

        public MessageDialog(string caption, IMessageDialogContentComponent component, MessageDialogIcon icon = MessageDialogIcon.None) : this(caption, icon)
        {
            this.Message.Content = component.Content;
            component.Decide += (s, e) => Decide();
            this.Loaded += (s, e) => component.OnLoaded(s, e);
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public List<UICommand> Commands { get; private set; } = new List<UICommand>();

        public int DefaultCommandIndex { get; set; }

        public int CancelCommandIndex { get; set; } = -1;

        public bool CloseWhenDeactivated { get; set; }

        public bool IsStretchWindow { get; set; } = true;


        public RelayCommand<UICommand> ButtonClickedCommand
        {
            get
            {
                return _buttonClickedCommand = _buttonClickedCommand ?? new RelayCommand<UICommand>(Execute);

                void Execute(UICommand? command)
                {
                    _resultCommand = command;
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private static FrameworkElement CreateTextContent(string content)
        {
            return new TextBlock()
            {
                Text = content,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private UICommand? GetDefaultCommand()
        {
            return (DefaultCommandIndex >= 0 && DefaultCommandIndex < Commands.Count) ? Commands[DefaultCommandIndex] : null;
        }

        private void Copy_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var caption = VisualTreeUtility.CollectElementText(this.Caption);
            var message = VisualTreeUtility.CollectElementText(this.Message);
            Clipboard.SetText(caption + message);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (CloseWhenDeactivated && !_isClosing)
            {
                this.Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _isClosing = true;
            base.OnClosing(e);
        }

        public MessageDialogResult ShowDialog(Window? owner)
        {
            _resultCommand = null;

            InitializeButtons();

            if (owner != null)
            {
                this.Owner = owner;
            }

            var command = (base.ShowDialog() != null)
                ? _resultCommand
                : (CancelCommandIndex >= 0 && CancelCommandIndex < Commands.Count) ? Commands[CancelCommandIndex] : null;

            return new MessageDialogResult(command);
        }

        private void Decide()
        {
            _resultCommand = Commands.FirstOrDefault(e => e.IsPossible);
            this.DialogResult = true;
            this.Close();
        }

        public new MessageDialogResult ShowDialog()
        {
            return ShowDialog(null);
        }

        private void InitializeButtons()
        {
            this.ButtonPanel.Children.Clear();
            this.SubButtonPanel.Children.Clear();

            if (Commands.Any())
            {
                var defaultCommand = GetDefaultCommand();

                foreach (var command in Commands)
                {
                    var button = CreateButton(command, command == defaultCommand);
                    if (command.Alignment == UICommandAlignment.Left)
                    {
                        this.SubButtonPanel.Children.Add(button);
                    }
                    else
                    {
                        this.ButtonPanel.Children.Add(button);
                    }
                }
            }
            else
            {
                var button = CreateButton(UICommands.OK, true);
                button.CommandParameter = null; // 設定されていなボタンなので結果が null になるようにする
                this.ButtonPanel.Children.Add(button);
            }

            // Focus
            if (DefaultCommandIndex >= 0 && DefaultCommandIndex < this.ButtonPanel.Children.Count)
            {
                this.ButtonPanel.Children[DefaultCommandIndex].Focus();
            }
        }

        private Button CreateButton(UICommand command, bool isDefault)
        {
            var button = new Button()
            {
                Style = App.Current.Resources[command.IsDanger ? "NVDialogDangerButton" : command.IsPossible ? "NVDialogAccentButton" : "NVDialogButton"] as Style,
                Content = TextResources.GetString(command.Label),
                Command = ButtonClickedCommand,
                CommandParameter = command,
            };

            return button;
        }

        private void MessageDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void SetIcon(MessageDialogIcon icon)
        {
            SetIcon(icon switch
            {
                MessageDialogIcon.Error
                    => "\uE783",
                MessageDialogIcon.Warning
                    => "\uE7BA",
                MessageDialogIcon.Information
                    => "\uE946",
                MessageDialogIcon.Question
                    => "\uE897",
                _
                    => ""
            });
        }

        private void SetIcon(string iconText)
        {
            this.FontIcon.Text = iconText;
            this.FontIcon.Visibility = string.IsNullOrEmpty(iconText) ? Visibility.Collapsed : Visibility.Visible;
        }
    }


    /// <summary>
    /// Dialog button alignment
    /// </summary>
    public enum UICommandAlignment
    {
        Right,
        Left
    }

    /// <summary>
    /// Dialog icon
    /// </summary>
    public enum MessageDialogIcon
    {
        None,
        Error,
        Warning,
        Information,
        Question
    }

    /// <summary>
    /// MessageDialog UICommand
    /// </summary>
    public class UICommand
    {
        public UICommand(string label)
        {
            this.Label = label;
        }

        public string Label { get; set; }

        public UICommandAlignment Alignment { get; set; }

        public bool IsPossible { get; set; }

        public bool IsDanger { get; set; }
    }

    /// <summary>
    /// Default UICommands
    /// </summary>
    public static class UICommands
    {
        public static UICommand OK { get; } = new UICommand("Word.OK") { IsPossible = true };
        public static UICommand Yes { get; } = new UICommand("Word.Yes") { IsPossible = true };
        public static UICommand No { get; } = new UICommand("Word.No");
        public static UICommand Cancel { get; } = new UICommand("Word.Cancel");
        public static UICommand Delete { get; } = new UICommand("Word.Delete") { IsPossible = true };
        public static UICommand Retry { get; } = new UICommand("Word.Retry") { IsPossible = true };

        // Usage: dialog.Commands.AddRange(...) 
        public static readonly List<UICommand> YesNo = new() { Yes, No };
        public static readonly List<UICommand> OKCancel = new() { OK, Cancel };
    }

    /// <summary>
    /// Content DI
    /// </summary>
    public interface IMessageDialogContentComponent
    {
        event EventHandler Decide;

        object Content { get; }

        void OnLoaded(object sender, RoutedEventArgs e);
    }

    /// <summary>
    /// MessageDialog Result
    /// </summary>
    public class MessageDialogResult
    {
        public MessageDialogResult(UICommand? command)
        {
            Command = command;
        }

        public UICommand? Command { get; }
        public bool IsPossible => Command != null && Command.IsPossible;
    }
}
