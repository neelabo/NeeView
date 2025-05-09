using NeeLaboratory.Generators;
using NeeLaboratory.Windows.Input;
using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// ダイアログボタン配置
    /// </summary>
    public enum UICommandAlignment
    {
        Right,
        Left
    }

    /// <summary>
    /// UWP の UICommandモドキ。MessageDialog用
    /// </summary>
    public class UICommand
    {
        public string Label { get; set; }

        public UICommandAlignment Alignment { get; set; }

        public bool IsPossible { get; set; }

        public UICommand(string label)
        {
            this.Label = label;
        }
    }

    /// <summary>
    /// UICommand の既定値集
    /// </summary>
    public static class UICommands
    {
        public static UICommand OK { get; } = new UICommand("@Word.OK") { IsPossible = true };
        public static UICommand Yes { get; } = new UICommand("@Word.Yes") { IsPossible = true };
        public static UICommand No { get; } = new UICommand("@Word.No");
        public static UICommand Cancel { get; } = new UICommand("@Word.Cancel");
        public static UICommand Delete { get; } = new UICommand("@Word.Delete") { IsPossible = true };
        public static UICommand Retry { get; } = new UICommand("@Word.Retry") { IsPossible = true };

        // dialog.Commands.AddRange(...) のような使用を想定したセット
        public static readonly List<UICommand> YesNo = new() { Yes, No };
        public static readonly List<UICommand> OKCancel = new() { OK, Cancel };
    }

    /// <summary>
    /// ContentのDI
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

    /// <summary>
    /// UWP の MessageDialogモドキ
    /// </summary>
    [NotifyPropertyChanged]
    public partial class MessageDialog : Window, INotifyPropertyChanged
    {
        public readonly static RoutedCommand CopyCommand = new("CopyCommand", typeof(MessageDialog), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.C, ModifierKeys.Control) }));
        public static Window? OwnerWindow { get; set; }

        private UICommand? _resultCommand;


        public MessageDialog()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Owner = OwnerWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.ShowInTaskbar = IsShowInTaskBar || OwnerWindow is null;

            this.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Execute));
        }

        public MessageDialog(string message, string title) : this()
        {
            this.Caption.Text = title;
            this.Message.Content = CreateTextContent(message);
        }

        public MessageDialog(FrameworkElement content, string title) : this()
        {
            this.Caption.Text = title;
            this.Message.Content = content;
        }

        public MessageDialog(IMessageDialogContentComponent component, string title) : this()
        {
            this.Caption.Text = title;
            this.Message.Content = component.Content;

            component.Decide += (s, e) => Decide();
            this.Loaded += (s, e) => component.OnLoaded(s, e);
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public List<UICommand> Commands { get; private set; } = new List<UICommand>();

        public int DefaultCommandIndex { get; set; }

        public int CancelCommandIndex { get; set; } = -1;

        public static bool IsShowInTaskBar { get; set; } = true;


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
                Style = App.Current.Resources[isDefault ? "NVDialogAccentButton" : "NVDialogButton"] as Style,
                Content = ResourceService.GetString(command.Label),
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

        /// <summary>
        /// ButtonClickedCommand command.
        /// </summary>
        private RelayCommand<UICommand>? _buttonClickedCommand;
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
    }
}
