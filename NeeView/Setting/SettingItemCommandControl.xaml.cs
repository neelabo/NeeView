using NeeLaboratory.Collection;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;
using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    // TODO: 整備
    public class GestureElement
    {
        public GestureElement()
        {
        }

        public string? Gesture { get; set; }
        public bool IsConflict { get; set; }
        public string? Splitter { get; set; }
        public string? Note { get; set; }
    }


    /// <summary>
    /// SettingItemCommandControl.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class SettingItemCommandControl : UserControl, INotifyPropertyChanged
    {
        // コマンド項目
        public class CommandItem : BindableBase
        {
            public CommandItem(string key, CommandElement command)
            {
                Key = key;
                Command = command;
            }

            public string Key { get; set; }
            public CommandElement Command { get; set; }
            public string? ShortCutNote { get; set; }
            public ObservableCollection<GestureElement> ShortCuts { get; set; } = new ObservableCollection<GestureElement>();
            public GestureElement? MouseGestureElement { get; set; }
            public string? TouchGestureNote { get; set; }
            public ObservableCollection<GestureElement> TouchGestures { get; set; } = new ObservableCollection<GestureElement>();
            public bool HasParameter { get; set; }
            public string? ParameterShareCommandName { get; set; }
            public bool IsShareParameter => !string.IsNullOrEmpty(ParameterShareCommandName);
            public string? ShareTips => ParameterShareCommandName != null ? string.Format(CultureInfo.InvariantCulture, TextResources.GetString("CommandListItem.Message.ShareParameter"), CommandTable.Current.GetElement(ParameterShareCommandName).Text) : null;
        }

        private int _commandTableChangeCount;
        private readonly ObservableCollection<CommandItem> _commandItems;
        private readonly Searcher _search = new Searcher(new SearchContext());
        private string _searchKeyword = "";

        public SettingItemCommandControl()
        {
            InitializeComponent();
            InitializeCommand();

            this.Root.DataContext = this;

            // 初期化
            _commandItems = new ObservableCollection<CommandItem>();
            UpdateCommandList();

            ItemsViewSource = new CollectionViewSource() { Source = _commandItems };
            ItemsViewSource.Filter += ItemsViewSource_Filter;

            this.Loaded += SettingItemCommandControl_Loaded;
            this.Unloaded += SettingItemCommandControl_Unloaded;

            this.SearchBoxModel = new SearchBoxModel(new CommandSearchBoxComponent(this));
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        #region Commands

        public readonly static RoutedCommand SettingCommand = new(nameof(SettingCommand), typeof(SettingItemCommandControl));
        public readonly static RoutedCommand EditCommand = new(nameof(EditCommand), typeof(SettingItemCommandControl));
        public readonly static RoutedCommand CloneCommand = new(nameof(CloneCommand), typeof(SettingItemCommandControl));
        public readonly static RoutedCommand RemoveCommand = new(nameof(RemoveCommand), typeof(SettingItemCommandControl));

        private void InitializeCommand()
        {
            this.CommandListView.CommandBindings.Add(new CommandBinding(SettingCommand, SettingCommand_Execute, SettingCommand_CanExecute));
            this.CommandListView.CommandBindings.Add(new CommandBinding(EditCommand, EditCommand_Execute, EditCommand_CanExecute));
            this.CommandListView.CommandBindings.Add(new CommandBinding(CloneCommand, CloneCommand_Execute, CloneCommand_CanExecute));
            this.CommandListView.CommandBindings.Add(new CommandBinding(RemoveCommand, RemoveCommand_Execute, RemoveCommand_CanExecute));
        }

        private void SettingCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void SettingCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem item)
            {
                OpenEditCommandWindow(item.Key, EditCommandWindowTab.Default);
            }
        }

        private void EditCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem item)
            {
                e.CanExecute = item.Command is ScriptCommand;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void EditCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem item && item.Command is ScriptCommand command)
            {
                command.OpenFile();
            }
        }

        private void CloneCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem item)
            {
                e.CanExecute = item.Command.IsCloneable;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void CloneCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem item)
            {
                var command = CommandTable.Current.CreateCloneCommand(item.Command);
                this.CommandListView.SelectedItem = _commandItems.FirstOrDefault(x => x.Command == command);
                FocusSelectedItem();
            }
        }

        private void RemoveCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem item)
            {
                e.CanExecute = item.Command.IsCloneCommand();
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void RemoveCommand_Execute(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.CommandListView.SelectedItem is CommandItem item)
            {
                CommandTable.Current.RemoveCloneCommand(item.Command);
            }
        }

        #endregion Commands


        public SearchBoxModel SearchBoxModel { get; }

        public CollectionViewSource ItemsViewSource { get; set; }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    Search();
                }
            }
        }


        private void FocusSelectedItem()
        {
            var selectedItem = this.CommandListView.SelectedItem;
            if (selectedItem is null) return;

            this.CommandListView.ScrollIntoView(selectedItem);
            this.CommandListView.UpdateLayout();

            var listViewItem = (ListViewItem)this.CommandListView
                .ItemContainerGenerator
                .ContainerFromItem(selectedItem);

            listViewItem?.Focus();
        }

        private void ItemsViewSource_Filter(object? sender, FilterEventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(_searchKeyword)) return;

            try
            {
                var item = (CommandItem)eventArgs.Item;
                var commands = new SingleEnumerableValue<CommandElement>(item.Command);
                var result = _search.Search(_searchKeyword, commands, CancellationToken.None);
                eventArgs.Accepted = result.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                eventArgs.Accepted = false;
            }
        }

        private void SettingItemCommandControl_Loaded(object? sender, RoutedEventArgs e)
        {
            CommandTable.Current.Changed += CommandTable_Changed;

            if (_commandTableChangeCount != CommandTable.Current.ChangeCount)
            {
                UpdateCommandList();
            }

            this.SearchKeyword = "";
            Search();
        }

        private void SettingItemCommandControl_Unloaded(object? sender, RoutedEventArgs e)
        {
            CommandTable.Current.Changed -= CommandTable_Changed;
        }

        private void CommandTable_Changed(object? sender, CommandChangedEventArgs e)
        {
            UpdateCommandList();
        }


        // 全コマンド初期化ボタン処理
        private void ResetGestureSettingButton_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new CommandResetWindow();
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                ScriptManager.Current.UpdateScriptCommands(isForce: true, isReplace: true);
                CommandTable.Current.RestoreCommandCollection(dialog.InputScheme);
            }
        }

        // コマンド一覧 更新
        private void UpdateCommandList()
        {
            _commandTableChangeCount = CommandTable.Current.ChangeCount;

            _commandItems.Clear();
            foreach (var element in CommandTable.Current.OrderBy(e => e.Value.Order))
            {
                var command = element.Value;

                var item = new CommandItem(element.Key, command);

                if (command.ParameterSource != null)
                {
                    item.HasParameter = true;

                    if (command.Share != null)
                    {
                        item.ParameterShareCommandName = command.Share.Name;
                    }
                }

                _commandItems.Add(item);
            }

            UpdateCommandListShortCut();
            UpdateCommandListMouseGesture();
            UpdateCommandListTouchGesture();

            this.CommandListView.Items.Refresh();
            //this.CommandListView.UpdateLayout();

            if (CommandListView.SelectedItem is CommandItem selectedItem)
            {
                var item = _commandItems.FirstOrDefault(x => x.Key == selectedItem.Key);
                if (item != null)
                {
                    this.CommandListView.SelectedItem = item;
                    FocusSelectedItem();
                }
            }
        }

        // コマンド一覧 ショートカット更新
        private void UpdateCommandListShortCut()
        {
            foreach (var item in _commandItems)
            {
                item.ShortCutNote = null;

                if (!item.Command.ShortCutKey.IsEmpty)
                {
                    var shortcuts = new ObservableCollection<GestureElement>();
                    foreach (var key in item.Command.ShortCutKey.Gestures)
                    {
                        var overlaps = _commandItems
                            .Where(e => !e.Command.ShortCutKey.IsEmpty && e.Key != item.Key && e.Command.ShortCutKey.Gestures.Contains(key))
                            .Select(e => CommandTable.Current.GetElement(e.Key).Text)
                            .ToList();

                        if (overlaps.Count > 0)
                        {
                            if (item.ShortCutNote != null) item.ShortCutNote += "\n";
                            item.ShortCutNote += string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.ConflictWith"), key.GetDisplayString(), TextResources.Join(overlaps));
                        }

                        var element = new GestureElement();
                        element.Gesture = key.GetDisplayString();
                        element.IsConflict = overlaps.Count > 0;
                        element.Splitter = ",";

                        shortcuts.Add(element);
                    }

                    if (shortcuts.Count > 0)
                    {
                        shortcuts.Last().Splitter = null;
                    }

                    item.ShortCuts = shortcuts;
                }
                else
                {
                    item.ShortCuts = new ObservableCollection<GestureElement>() { new GestureElement() };
                }
            }
        }

        // コマンド一覧 マウスジェスチャー更新
        private void UpdateCommandListMouseGesture()
        {
            foreach (var item in _commandItems)
            {
                if (!item.Command.MouseGesture.IsEmpty)
                {
                    var overlaps = _commandItems
                        .Where(e => e.Key != item.Key && e.Command.MouseGesture == item.Command.MouseGesture)
                        .Select(e => CommandTable.Current.GetElement(e.Key).Text)
                        .ToList();

                    var element = new GestureElement();
                    element.Gesture = item.Command.MouseGesture.GetDisplayString();
                    element.IsConflict = overlaps.Count > 0;
                    if (overlaps.Count > 0)
                    {
                        element.Note = string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.Conflict"), TextResources.Join(overlaps));
                    }

                    item.MouseGestureElement = element;
                }
                else
                {
                    item.MouseGestureElement = new GestureElement();
                }
            }
        }

        // コマンド一覧 タッチ更新
        private void UpdateCommandListTouchGesture()
        {
            foreach (var item in _commandItems)
            {
                item.TouchGestureNote = null;

                if (!item.Command.TouchGesture.IsEmpty)
                {
                    var elements = new ObservableCollection<GestureElement>();
                    foreach (var area in item.Command.TouchGesture.Areas)
                    {
                        var overlaps = _commandItems
                            .Where(e => !e.Command.TouchGesture.IsEmpty && e.Key != item.Key && e.Command.TouchGesture.Areas.Contains(area))
                            .Select(e => CommandTable.Current.GetElement(e.Key).Text)
                            .ToList();

                        if (overlaps.Count > 0)
                        {
                            if (item.TouchGestureNote != null) item.TouchGestureNote += "\n";
                            item.TouchGestureNote += string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.ConflictWith"), area.GetDisplayString(), TextResources.Join(overlaps));
                        }

                        var element = new GestureElement();
                        element.Gesture = area.GetDisplayString();
                        element.IsConflict = overlaps.Count > 0;
                        element.Splitter = ",";

                        elements.Add(element);
                    }

                    if (elements.Count > 0)
                    {
                        elements.Last().Splitter = null;
                    }

                    item.TouchGestures = elements;
                }
                else
                {
                    item.TouchGestures = new ObservableCollection<GestureElement>() { new GestureElement() };
                }
            }
        }


        private void EditCommandParameterButton_Clock(object? sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not CommandItem command) return;

            this.CommandListView.SelectedItem = command;
            OpenEditCommandWindow(command.Key, EditCommandWindowTab.Parameter);
        }

        private void OpenEditCommandWindow(string key, EditCommandWindowTab tab)
        {
            var dialog = new EditCommandWindow(key, tab);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }

        private void ListViewItem_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            if (sender is not ListViewItem listViewItem) return;

            if (listViewItem.Content is not CommandItem item) return;

            // カーソル位置から初期TABを選択
            var hitResult = VisualTreeHelper.HitTest(listViewItem, e.GetPosition(listViewItem));
            var tag = GetAncestorTag(hitResult.VisualHit, "@");
            var tab = tag switch
            {
                "@shortcut" => EditCommandWindowTab.InputGesture,
                "@gesture" => EditCommandWindowTab.MouseGesture,
                "@touch" => EditCommandWindowTab.InputTouch,
                _ => EditCommandWindowTab.Default,
            };
            OpenEditCommandWindow(item.Key, tab);
        }

        private void ListViewItem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not ListViewItem listViewItem) return;

            if (listViewItem.Content is not CommandItem item) return;

            if (e.Key == Key.Enter)
            {
                OpenEditCommandWindow(item.Key, EditCommandWindowTab.Default);
                e.Handled = true;
            }
        }

        private void ListViewItem_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            ContextMenu? contextMenu = (sender as ListViewItem)?.ContextMenu;
            if (contextMenu is null) return;

            var editMenu = contextMenu.Items.OfType<MenuItem>().FirstOrDefault(x => x.Name == "EditMenu");
            if (editMenu is null) return;

            if (this.CommandListView.SelectedItem is CommandItem item)
            {
                editMenu.Visibility = (item.Command is ScriptCommand) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// ビジュアルツリーの親に定義されている文字列タグを取得。
        /// </summary>
        /// <param name="obj">検索開始要素</param>
        /// <param name="prefix">文字列のプレフィックス</param>
        /// <returns></returns>
        private static string? GetAncestorTag(DependencyObject obj, string prefix)
        {
            while (obj != null)
            {
                if ((obj as FrameworkElement)?.Tag is string tag && tag.StartsWith(prefix, StringComparison.Ordinal)) return tag;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        private void Search()
        {
            ItemsViewSource.View.Refresh();
        }

        private SearchKeywordAnalyzeResult SearchKeywordAnalyze(string keyword)
        {
            try
            {
                return new SearchKeywordAnalyzeResult(_search.Analyze(keyword));
            }
            catch (Exception ex)
            {
                return new SearchKeywordAnalyzeResult(ex);
            }
        }

        /// <summary>
        /// 検索ボックスコンポーネント
        /// </summary>
        public class CommandSearchBoxComponent : ISearchBoxComponent
        {
            private readonly SettingItemCommandControl _self;

            public CommandSearchBoxComponent(SettingItemCommandControl self)
            {
                _self = self;
            }

            public HistoryStringCollection? History { get; } = new HistoryStringCollection();

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public SearchKeywordAnalyzeResult Analyze(string keyword) => _self.SearchKeywordAnalyze(keyword);

            public void Search(string keyword) => _self.SearchKeyword = keyword;
        }

    }


}
