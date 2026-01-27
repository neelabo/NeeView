// from https://stackoverflow.com/questions/14948171/how-to-emulate-a-console-in-wpf
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ConsoleEmulator : UserControl, INotifyPropertyChanged, IDisposable
    {
        public readonly static RoutedCommand ClearScreenCommand = new("ClearScreen", typeof(ConsoleEmulator), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.L, ModifierKeys.Control) }));

        private string _consoleInput = "";
        private readonly List<string> _history = new();
        private int _historyIndex;
        private List<string>? _candidates;
        private int _candidatesIndex;
        private bool _isPromptEnabled = true;
        private bool _isInputEnabled = true;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposedValue;

        public ConsoleEmulator()
        {
            InitializeComponent();
            Scroller.DataContext = this;

            this.Loaded += ConsoleEmulator_Loaded;
            this.RootPanel.MouseDown += RootPanel_MouseDown;
            this.OutputBlock.PreviewKeyDown += OutputBlock_PreviewKeyDown;
            this.InputBlock.Loaded += InputBlock_Loaded;
            this.InputBlock.PreviewKeyDown += InputBlock_PreviewKeyDown;

            this.CommandBindings.Add(new CommandBinding(ClearScreenCommand, ClearScreen, (s, e) => e.CanExecute = true));
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public IConsoleHost ConsoleHost
        {
            get { return (IConsoleHost)GetValue(ConsoleHostProperty); }
            set { SetValue(ConsoleHostProperty, value); }
        }

        public static readonly DependencyProperty ConsoleHostProperty =
            DependencyProperty.Register("ConsoleHost", typeof(IConsoleHost), typeof(ConsoleEmulator), new PropertyMetadata(null, ConsoleHostChanged));

        private static void ConsoleHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConsoleEmulator control)
            {
                if (e.OldValue != null)
                {
                    ((IConsoleHost)e.OldValue).Output -= Log;
                }
                if (control.ConsoleHost != null)
                {
                    control.ConsoleHost.Output += Log;
                }

                void Log(object? sender, ConsoleHostOutputEventArgs args)
                {
                    control.WriteLine(args.Output ?? "");
                }
            }
        }

        public string Prompt
        {
            get { return (string)GetValue(PromptProperty); }
            set { SetValue(PromptProperty, value); }
        }

        public static readonly DependencyProperty PromptProperty =
            DependencyProperty.Register("Prompt", typeof(string), typeof(ConsoleEmulator), new PropertyMetadata("> "));


        public string FirstMessage
        {
            get { return (string)GetValue(FirstMessageProperty); }
            set { SetValue(FirstMessageProperty, value); }
        }

        public static readonly DependencyProperty FirstMessageProperty =
            DependencyProperty.Register("FirstMessage", typeof(string), typeof(ConsoleEmulator), new PropertyMetadata(null));


        public bool IsPromptEnabled
        {
            get { return _isPromptEnabled; }
            set { SetProperty(ref _isPromptEnabled, value); }
        }

        public string ConsoleInput
        {
            get => _consoleInput;
            set => SetProperty(ref _consoleInput, value);
        }

        private void ConsoleEmulator_Loaded(object sender, RoutedEventArgs e)
        {
            if (FirstMessage != null)
            {
                WriteLine(FirstMessage);
            }
        }

        private void RootPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_disposedValue) return;

            this.InputBlock.Focus();
            e.Handled = true;
        }


        private void OutputBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_disposedValue) return;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                FocusToInputBlock();
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                FocusToInputBlock();
            }
        }

        // NOTE: 未使用
        private void OutputBlock_PreviewMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_disposedValue) return;

            if (this.OutputBlock.SelectionLength == 0)
            {
                FocusToInputBlock();
            }
        }

        private void InputBlock_Loaded(object sender, RoutedEventArgs e)
        {
            if (_disposedValue) return;

            this.InputBlock.Focus();
        }


        private void InputBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_disposedValue) return;

            // Ctrl+C
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                CancelScript();
            }

            if (!_isInputEnabled)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.System)
            {
                return;
            }

            // TAB interpolation
            if (e.Key == Key.Tab && ConsoleHost.WordTree != null)
            {
                if (_candidates == null)
                {
                    _candidates = ConsoleHost.WordTree.Interpolate(ConsoleInput).OrderBy(x => x).ToList();
                    _candidatesIndex = 0;
                }
                else
                {
                    var direction = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1;
                    _candidatesIndex = (_candidatesIndex + _candidates.Count + direction) % _candidates.Count;
                }
                ConsoleInput = _candidates[_candidatesIndex];
                FocusToInputBlock();
                e.Handled = true;
                return;
            }

            _candidates = null;

            if (e.Key == Key.Enter)
            {
                ConsoleInput = InputBlock.Text;
                RunCommand();
                FocusToInputBlock();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                PreviewHistory();
                FocusToInputBlock();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                NextHistory();
                FocusToInputBlock();
                e.Handled = true;
            }
        }

        private void FocusToInputBlock()
        {
            this.InputBlock.Focus();
            ScrollToBottom();
            this.InputBlock.Select(InputBlock.Text.Length, 0);
        }

        private void ScrollToBottom()
        {
            this.Scroller.ScrollToBottom();
            this.Scroller.ScrollToLeftEnd();
        }

        private void ClearScreen(object? sender, ExecutedRoutedEventArgs e)
        {
            ClearScreen();
        }

        private void ClearScreen()
        {
            this.OutputBlock.Clear();
            this.OutputBlock.Visibility = Visibility.Collapsed;
            this.InputBlock.Text = ConsoleInput = "";
            this.InputBlock.Focus();
        }

        public void WriteLine(string? text)
        {
            if (_disposedValue) return;

            //if (string.IsNullOrEmpty(text)) return;

            var maxLength = 256 * 256;
            if (text is not null && text.Length > maxLength)
            {
                text = text[..maxLength] + "...";
            }

            this.Dispatcher.Invoke((Action)(() =>
            {
                if (string.IsNullOrEmpty(this.OutputBlock.Text))
                {
                    this.OutputBlock.AppendText(text);
                }
                else
                {
                    this.OutputBlock.AppendText("\r\n" + text);
                }

                this.OutputBlock.Visibility = Visibility.Visible;
                ScrollToBottom();
            }));
        }

        private void PreviewHistory()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
            }

            if (_history.Count > 0)
            {
                ConsoleInput = _history[_historyIndex];
            }
        }

        private void NextHistory()
        {
            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
            }

            if (_historyIndex < _history.Count)
            {
                ConsoleInput = _history[_historyIndex];
            }
        }

        private void RunCommand()
        {
            var input = ConsoleInput;

            WriteLine(Prompt + ConsoleInput);
            ConsoleInput = "";

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            switch (input.Trim())
            {
                case "cls":
                    ClearScreen();
                    break;

                default:
                    RunScript(input);
                    break;
            }

            _history.Add(input);
            _historyIndex = _history.Count;
        }

        private void RunScript(string input)
        {
            if (_disposedValue) return;

            var consoleHost = ConsoleHost;
            if (consoleHost is null) return;

            DisableInput();
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                try
                {
                    var result = consoleHost.Execute(input, _cancellationTokenSource.Token);
                    WriteLine(result);
                }
                finally
                {
                    EnableInput();
                }
            });
        }

        private void CancelScript()
        {
            if (_disposedValue) return;

            ////Debug.WriteLine($"CancelScript");
            _cancellationTokenSource?.Cancel();
        }


        private void EnableInput()
        {
            if (_disposedValue) return;

            _isInputEnabled = true;
            IsPromptEnabled = true;
        }

        private void DisableInput()
        {
            if (_disposedValue) return;

            _isInputEnabled = false;
            IsPromptEnabled = false;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }



    public class ConsoleHostOutputEventArgs : EventArgs
    {
        public ConsoleHostOutputEventArgs(string output)
        {
            Output = output;
        }

        public string Output { get; set; }
    }

    public interface IConsoleHost
    {
        event EventHandler<ConsoleHostOutputEventArgs>? Output;

        WordTree WordTree { get; }

        string? Execute(string input, CancellationToken token);
    }
}
