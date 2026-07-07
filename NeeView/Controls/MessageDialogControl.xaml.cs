using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeView.Properties;
using NeeView.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// DialogControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageDialogControl : UserControl
    {
        public readonly static RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(MessageDialogControl), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.C, ModifierKeys.Control) }));

        private readonly MessageDialogControlViewModel? _vm;


        public MessageDialogControl()
        {
            InitializeComponent();

            this.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Execute));

            this.Loaded += DialogControl_Loaded;
        }


        public MessageDialogControl(MessageDialogControlViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;

            InitializeButtons();
        }


        private void DialogControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            FocusDefaultButton();

            _vm.OnLoaded(sender, e);
        }


        private void FocusDefaultButton()
        {
            if (_vm is null) return;

            var context = _vm.Context;
            if (context.DefaultCommandIndex >= 0 && context.DefaultCommandIndex < this.ButtonPanel.Children.Count)
            {
                this.ButtonPanel.Children[context.DefaultCommandIndex].Focus();
            }
        }

        private void InitializeButtons()
        {
            if (_vm is null) return;

            this.ButtonPanel.Children.Clear();
            this.SubButtonPanel.Children.Clear();

            var context = _vm.Context;

            if (context.Commands.Any())
            {
                var defaultCommand = context.GetDefaultCommand();

                foreach (var command in context.Commands)
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
        }

        private Button CreateButton(UICommand command, bool isDefault)
        {
            Debug.Assert(_vm is not null);

            var button = new Button()
            {
                Style = App.Current.Resources[command.IsDanger ? "NVDialogDangerButton" : command.IsPossible ? "NVDialogAccentButton" : "NVDialogButton"] as Style,
                Content = TextResources.GetString(command.Label),
                Command = _vm.ButtonClickedCommand,
                CommandParameter = command,
            };

            return button;
        }



        private void Copy_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var caption = VisualTreeUtility.CollectElementText(this.Caption);
            var message = VisualTreeUtility.CollectElementText(this.Message);
            Clipboard.SetText(caption + message);
        }
    }





    public partial class MessageDialogControlViewModel : ObservableObject
    {
        private readonly MessageDialogContext _context;


        public MessageDialogControlViewModel(MessageDialogContext context)
        {
            _context = context;
        }


        public MessageDialogContext Context => _context;

        public string Caption => _context.Caption;

        public object? Content => _context.Content;

        public MessageDialogIcon Icon => _context.Icon;


        [RelayCommand]
        public void ButtonClicked(UICommand? command)
        {
            _context.Decide(command);
        }

        public void OnLoaded(object s, RoutedEventArgs e)
        {
            _context.OnLoaded(s, e);
        }

        public void Decide(bool possible)
        {
            _context.Decide(possible);
        }
    }

}
