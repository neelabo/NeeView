using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#pragma warning disable CA1822

namespace NeeView
{
    public class CommandHost
    {
        private readonly CommandHostStaticResource _resource;
        private readonly ScriptAccessDiagnostics _accessDiagnostics;
        private readonly IHasScriptPath _scriptPath;
        private CommandAccessor? _command;
        private List<string> _args = new();
        private CancellationToken _cancellationToken;


        public CommandHost() : this(new DummyScriptPath())
        {
        }

        public CommandHost(IHasScriptPath scriptPath)
        {
            _resource = CommandHostStaticResource.Current;
            _accessDiagnostics = _resource.AccessDiagnostics;
            _scriptPath = scriptPath;

            Config = _resource.ConfigMap.Map;
            Command = _resource.CommandAccessMap;
            Environment = new EnvironmentAccessor();
            Book = new BookAccessor(_accessDiagnostics);
            Bookshelf = new BookshelfPanelAccessor();
            PageList = new PageListPanelAccessor();
            Bookmark = new BookmarkPanelAccessor();
            Playlist = new PlaylistPanelAccessor();
            History = new HistoryPanelAccessor();
            Information = new InformationPanelAccessor();
            Effect = new EffectPanelAccessor();
            Navigator = new NavigatorPanelAccessor();
            ExternalAppCollection = new ExternalAppCollectionAccessor();
            DestinationFolderCollection = new DestinationFolderCollectionAccessor();
            SusiePluginCollection = new SusiePluginCollectionAccessor();
            MainView = new MainViewPanelAccessor();
        }


        [WordNodeMember]
        public CommandAccessor? CurrentCommand => _command;

        [WordNodeMember]
        public string? ScriptPath => _scriptPath.ScriptPath;

        [WordNodeMember]
        public List<string> Args => _args;

        [WordNodeMember]
        public IDictionary<string, object> Values => _resource.Values;

        [WordNodeMember(IsAutoCollect = false)]
        public PropertyMap Config { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public CommandAccessorMap Command { get; }

        [WordNodeMember]
        public EnvironmentAccessor Environment { get; }

        [WordNodeMember]
        public BookAccessor Book { get; }

        [WordNodeMember]
        public BookshelfPanelAccessor Bookshelf { get; }

        [WordNodeMember]
        public PageListPanelAccessor PageList { get; }

        [WordNodeMember]
        public BookmarkPanelAccessor Bookmark { get; }

        [WordNodeMember]
        public PlaylistPanelAccessor Playlist { get; }

        [WordNodeMember]
        public HistoryPanelAccessor History { get; }

        [WordNodeMember]
        public InformationPanelAccessor Information { get; }

        [WordNodeMember]
        public EffectPanelAccessor Effect { get; }

        [WordNodeMember]
        public NavigatorPanelAccessor Navigator { get; }

        [WordNodeMember]
        public ExternalAppCollectionAccessor ExternalAppCollection { get; }

        [WordNodeMember]
        public DestinationFolderCollectionAccessor DestinationFolderCollection { get; }

        [WordNodeMember]
        public SusiePluginCollectionAccessor SusiePluginCollection { get; }

        [WordNodeMember]
        public MainViewPanelAccessor MainView { get; }

        [WordNodeMember]
        public WindowAccessor Window
        {
            get { return new WindowAccessor(new MainWindowProxy()); }
        }


        [WordNodeMember]
        [Obsolete("no used"), Alternative(nameof(Playlist), 39)] // ver.39
        public object? Pagemark
        {
            get
            {
                return _accessDiagnostics.Throw<object>(new NotSupportedException(RefrectionTools.CreatePropertyObsoleteMessage(this.GetType())));
            }
        }


        internal bool IsDirty => Command != _resource.CommandAccessMap;



        internal void SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            Book.SetCancellationToken(cancellationToken);
            Bookshelf.SetCancellationToken(cancellationToken);
        }

        internal void SetCommandName(string name)
        {
            _command = Command.TryGetCommand(name, out var command) ? command as CommandAccessor : null;
        }

        internal void SetArgs(List<string> args)
        {
            _args = args;
        }

        [WordNodeMember]
        public void ShowMessage(string message)
        {
            InfoMessage.Current.SetMessage(InfoMessageType.Notify, message);
        }

        [WordNodeMember]
        public void ShowToast(string message)
        {
            ToastService.Current.Show(new Toast(message));
        }

        [WordNodeMember]
        public bool ShowDialog(string title, string message = "", int commands = 0)
        {
            return AppDispatcher.Invoke(() => ShowDialogInner(title, message, commands));
        }

        private bool ShowDialogInner(string title, string message, int commands)
        {
            var dialog = new MessageDialog(message, title);
            switch (commands)
            {
                default:
                    dialog.Commands.Add(UICommands.OK);
                    break;
                case 1:
                    dialog.Commands.Add(UICommands.OK);
                    dialog.Commands.Add(UICommands.Cancel);
                    break;
                case 2:
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    break;
            }
            var result = dialog.ShowDialog(App.Current.MainWindow);
            return result.IsPossible;
        }

        [WordNodeMember]
        public string? ShowInputDialog(string title)
        {
            return AppDispatcher.Invoke(() => ShowInputDialogInner(title, null, null));
        }

        [WordNodeMember]
        public string? ShowInputDialog(string title, string text)
        {
            return AppDispatcher.Invoke(() => ShowInputDialogInner(title, null, text));
        }

        [WordNodeMember]
        public string? ShowInputDialog(string title, string message, string text)
        {
            return AppDispatcher.Invoke(() => ShowInputDialogInner(title, message, text));
        }


        private static string? ShowInputDialogInner(string title, string? message, string? text)
        {
            var component = new InputDialogComponent(message, text);
            var dialog = new MessageDialog(component, title);
            dialog.Commands.Add(UICommands.OK);
            dialog.Commands.Add(UICommands.Cancel);
            var result = dialog.ShowDialog(App.Current.MainWindow);
            return result.IsPossible ? component.Text : null;
        }

        [WordNodeMember]
        public void CopyFile(string source, string destination)
        {
            Task.Run(async () =>
            {
                try
                {
                    var entry = await ArchiveEntryUtility.CreateAsync(source, ArchiveHint.None, true, _cancellationToken);
                    var path = await entry.RealizeAsync(_cancellationToken);
                    if (path is not null)
                    {
                        await FileIO.SHCopyAsync(path, destination, _cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    ToastService.Current.Show(new Toast(ex.Message, nameof(CopyFile), ToastIcon.Error));
                }
            });
        }

        [WordNodeMember]
        public void MoveFile(string source, string destination)
        {
            Task.Run(async () =>
            {
                try
                {
                    await FileIO.SHMoveAsync(source, destination, _cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    ToastService.Current.Show(new Toast(ex.Message, nameof(MoveFile), ToastIcon.Error));
                }
            });
        }

        [WordNodeMember]
        public void DeleteFile(string path)
        {
            Task.Run(async () =>
            {
                try
                {
                    await FileIO.DeleteAsync(path);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    ToastService.Current.Show(new Toast(ex.Message, nameof(DeleteFile), ToastIcon.Error));
                }
            });
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            if (node.Children is null) throw new InvalidOperationException();

            node.Children.Add(Config.CreateWordNode(nameof(Config)));
            node.Children.Add(Command.CreateWordNode(nameof(Command)));
            return node;
        }


        private class InputDialogComponent : IMessageDialogContentComponent
        {
            private readonly StackPanel _panel;
            private readonly TextBox _textBox;

            public InputDialogComponent(string? message, string? text)
            {
                _panel = new StackPanel();

                if (!string.IsNullOrEmpty(message))
                {
                    var messageText = new TextBlock()
                    {
                        Margin = new Thickness(0, 0, 0, 4),
                        Text = message,
                        TextWrapping = TextWrapping.Wrap
                    };
                    _panel.Children.Add(messageText);
                }

                _textBox = new TextBox() { Text = text ?? "", Padding = new Thickness(5.0) };
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                _panel.Children.Add(_textBox);
            }

            private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Return)
                {
                    Decide?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                }
            }

            public event EventHandler? Decide;

            public object Content => _panel;

            public string Text => _textBox.Text;

            public void OnLoaded(object sender, RoutedEventArgs e)
            {
                _textBox.Focus();
                _textBox.SelectAll();
            }
        }


        internal class MainWindowProxy : WindowProxy
        {
            public override Window? Window => App.Current.MainWindow;
        }
    }
}
