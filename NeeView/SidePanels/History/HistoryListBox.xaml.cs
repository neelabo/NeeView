//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// HistoryListBox.xaml の相互作用ロジック
    /// </summary>
    [LocalDebug]
    public partial class HistoryListBox : UserControl, IPageListPanel, IDisposable
    {
        private readonly HistoryListBoxViewModel _vm;
        private readonly ListBoxThumbnailLoader _thumbnailLoader;
        private readonly PageThumbnailJobClient _jobClient;
        private bool _focusRequest;
        private bool _disposedValue = false;
        private readonly DisposableCollection _disposables = new();
        private SelectorMemento _selectorMemento = new();
        private BookHistory? _clickItem;


        static HistoryListBox()
        {
            InitializeCommandStatic();
        }


        public HistoryListBox(HistoryListBoxViewModel vm)
        {
            InitializeComponent();

            _vm = vm;
            this.DataContext = vm;

            InitializeCommand();

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;

            this.ListBox.GotFocus += ListBox_GotFocus;

            _jobClient = new PageThumbnailJobClient("HistoryList", JobCategories.BookThumbnailCategory);
            _disposables.Add(_jobClient);

            _thumbnailLoader = new ListBoxThumbnailLoader(this, _jobClient);

            _disposables.Add(Config.Current.Panels.ContentItemProfile.SubscribePropertyChanged(PanelListItemProfile_PropertyChanged));
            _disposables.Add(Config.Current.Panels.BannerItemProfile.SubscribePropertyChanged(PanelListItemProfile_PropertyChanged));
            _disposables.Add(Config.Current.Panels.ThumbnailItemProfile.SubscribePropertyChanged(PanelListItemProfile_PropertyChanged));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #region IPageListBox support

        public ListBox PageCollectionListBox => this.ListBox;

        public bool IsThumbnailVisible => _vm is not null && _vm.IsThumbnailVisible;

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> collection) => collection.OfType<IHasPage>();

        #endregion

        #region Commands

        public static readonly RoutedCommand OpenBookCommand = new("OpenBookCommand", typeof(HistoryListBox));
        public static readonly RoutedCommand RemoveCommand = new("RemoveCommand", typeof(HistoryListBox));

        public static void InitializeCommandStatic()
        {
            OpenBookCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        public void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenBookCommand, OpenBook_Exec));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
        }

        public void OpenBook_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (_vm is null) return;

            if (sender is ListBox { SelectedItem: BookHistory item } && item.Path != null)
            {
                _vm.Load(item.Path);
                e.Handled = true;
            }
        }

        public void Remove_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (_vm is null) return;

            if (sender is ListBox { SelectedItems: System.Collections.IList items } && items.Count > 0)
            {
                _vm.Remove(items.Cast<BookHistory>().ToList());
                FocusSelectedItem(true);
            }
        }

        #endregion

        private void ListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.ListBox.IsFocused) return;

            LocalDebug.WriteLine($"ReFocus");
            AppDispatcher.BeginInvoke(() => FocusSelectedItem(true));
        }

        private void PanelListItemProfile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.ListBox.Items?.Refresh();
        }

        // フォーカス
        public bool FocusSelectedItem(bool focus)
        {
            if (this.ListBox.SelectedIndex < 0) return false;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (focus)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                return lbi?.Focus() ?? false;
            }
            else
            {
                return false;
            }
        }

        public void Refresh()
        {
            this.ListBox.Items.Refresh();
        }

        public void FocusAtOnce()
        {
            var focused = FocusSelectedItem(true);
            if (!focused)
            {
                _focusRequest = true;
            }
        }

        // 履歴項目決定
        private void HistoryListItem_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;
            
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if (sender is ListBoxItem { Content: BookHistory item } && item.Path is not null)
            {
                _clickItem = item;
            }
        }

        private void HistoryListItem_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if (sender is ListBoxItem { Content: BookHistory item } && item.Path is not null)
            {
                if (_clickItem == item && !Config.Current.Panels.OpenWithDoubleClick)
                {
                    _vm.Load(item.Path);
                }
            }
            _clickItem = null;
        }

        private void HistoryListItem_MouseDoubleClick(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            var item = ((sender as ListBoxItem)?.Content as BookHistory);
            if (item?.Path is null) return;

            if (Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Load(item.Path);
            }
        }

        // 履歴項目決定(キー)
        private void HistoryListItem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            if (this.ListBox.IsSimpleTextSearchEnabled)
            {
                KeyExGesture.AddFilter(KeyExGestureFilter.TextKey);
            }

            var item = ((sender as ListBoxItem)?.Content as BookHistory);
            if (item?.Path is null) return;

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Return)
                {
                    _vm.Load(item.Path);
                    e.Handled = true;
                }
            }
        }

        // 項目コンテキストメニュー
        private void HistoryListItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is not ListBoxItem container)
            {
                return;
            }

            if (container.Content is not BookHistory item)
            {
                return;
            }

            // コンテキストメニュー生成
            var contextMenu = container.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@HistoryItem.Menu.OpenBook"), Command = OpenBookCommand });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = ResourceService.GetString("@HistoryItem.Menu.Delete"), Command = RemoveCommand });
        }

        // リストのキ入力
        private void HistoryListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }

        // 表示/非表示イベント
        // TODO: 初回のフォーカスに失敗する。
        private void HistoryListBox_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm is null) return;
            if (sender is not ListBox listBox) return;

            if (listBox.IsVisible)
            {
                _vm.CollectionViewSource.View.Refresh();
                if (listBox.SelectedIndex < 0) listBox.SelectedIndex = 0;
                listBox.UpdateLayout();

                AppDispatcher.BeginInvoke(() =>
                {
                    FocusSelectedItem(_focusRequest);
                    _focusRequest = false;
                });
            }
        }

        private void HistoryListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;
            LocalDebug.WriteLine($"SelectedIndex={listBox.SelectedIndex}, SelectedItem={listBox.SelectedItem}");

            if (listBox.SelectedIndex >= 0)
            {
                LocalDebug.WriteLine("Store");
                _selectorMemento = new SelectorMemento(listBox);
            }
            else if (listBox.Items.Count > 0)
            {
                _selectorMemento.RestoreTo(listBox);
            }
        }

        // リスト全体が変化したときにサムネイルを更新する
        private void HistoryListBox_TargetUpdated(object? sender, DataTransferEventArgs e)
        {
            AppDispatcher.BeginInvoke(() => _thumbnailLoader?.Load());
        }

        #region UI Accessor

        public List<BookHistory>? GetItems()
        {
            if (_vm is null) return null;

            return _vm.GetViewItems();
        }

        public List<BookHistory> GetSelectedItems()
        {
            // ListBox 生成直後でプロパティが不定の場合、モデルデータの値を返す
            if (this.ListBox.SelectedItem is null)
            {
                if (_vm.SelectedItem is null)
                {
                    return new();
                }
                else
                {
                    return new() { _vm.SelectedItem };
                }
            }

            return this.ListBox.SelectedItems.Cast<BookHistory>().ToList();
        }

        public void SetSelectedItems(IEnumerable<BookHistory>? selectedItems)
        {
            var sources = GetItems();
            if (sources is null) return;

            var items = selectedItems?.Intersect(sources).ToList();
            this.ListBox.SetSelectedItems(items);
            this.ListBox.ScrollItemsIntoView(items);

            // ListBox 生成直後でプロパティが不定の場合、モデルデータにも反映
            // 個数 0 は未初期化とみなされるらしい
            if (items is null || items.Count == 0)
            {
                _vm.SelectedItem = null;
            }
        }

        #endregion UI Accessor

    }


    /// <summary>
    /// 選択項目の記憶
    /// </summary>
    /// <param name="Index"></param>
    /// <param name="Item"></param>
    [LocalDebug]
    internal partial record SelectorMemento(int Index, object? Item)
    {
        public SelectorMemento() : this(0, null)
        {
        }

        public SelectorMemento(Selector listBox) : this(listBox.SelectedIndex, listBox.SelectedItem)
        {
        }

        public void RestoreTo(Selector listBox)
        {
            listBox.SelectedItem = Item;
            if (listBox.SelectedItem is null)
            {
                LocalDebug.WriteLine($"Restore Selected item by index");
                listBox.SelectedIndex = Index;
            }
            else
            {
                LocalDebug.WriteLine($"Restore Selected item by object");
            }
        }
    }


    public class ArchiveEntryToDecoratePlaceNameConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ArchiveEntry entry)
            {
                var directory = entry.IsFileSystem ? System.IO.Path.GetDirectoryName(entry.SystemPath) ?? "" : entry.RootArchive.SystemPath;
                return SidePanelProfile.GetDecoratePlaceName(directory);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToFormatString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
