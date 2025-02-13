using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
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

namespace NeeView
{
    /// <summary>
    /// HistoryListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryListBox : UserControl, IPageListPanel, IDisposable
    {
        private readonly HistoryListBoxViewModel _vm;
        private readonly ListBoxThumbnailLoader _thumbnailLoader;
        private readonly PageThumbnailJobClient _jobClient;
        private bool _focusRequest;
        private ValidTimeFlag _focusRequestTimestamp = ValidTimeFlag.Empty;
        private bool _disposedValue = false;
        private readonly DisposableCollection _disposables = new();


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

            _disposables.Add(_vm.SubscribePropertyChanged(nameof(_vm.Items),
                (s, e) => AppDispatcher.BeginInvoke(() => ViewModel_ItemsChanged(s, e))));
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

        public IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs) => objs.OfType<IHasPage>();

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

            var item = this.ListBox.SelectedItem as BookHistory;
            if (item?.Path == null) return;

            _vm.Load(item.Path);
            e.Handled = true;
        }

        public void Remove_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (_vm is null) return;

            var items = this.ListBox.SelectedItems?.Cast<BookHistory>().ToList();
            if (items == null || !items.Any()) return;

            _vm.Remove(items);
            FocusSelectedItem(true);
        }

        #endregion


        private void ViewModel_ItemsChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.ListBox.SetAnchorItem(null);

            // リストが更新されても選択項目のフォーカスを維持するための処理
            // NOTE: この一連の処理はかなり無理やりな実装になっているので再検討対象です
            if (this.ListBox.IsKeyboardFocusWithin)
            {
                FocusSelectedItem(true);
                _focusRequestTimestamp = ValidTimeFlag.Create();
            }
        }

        private void ListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.ListBox.IsFocused) return;

            // フォーカス予約処理
            // リストが更新されるとフォーカスはそのListBox自体に移るので、選択項目にフォーカスを戻す。
            if (_focusRequestTimestamp.Condition(100))
            {
                _focusRequestTimestamp = ValidTimeFlag.Empty;
                // このイベント内でフォーカス移動させるのは問題があるのでディスパッチ処理にする
                AppDispatcher.BeginInvoke(() => FocusSelectedItem(true));
            }
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

            var item = ((sender as ListBoxItem)?.Content as BookHistory);
            if (item?.Path is null) return;

            if (!Config.Current.Panels.OpenWithDoubleClick)
            {
                _vm.Load(item.Path);
            }
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
                KeyExGesture.AllowSingleKey = false;
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
        private async void HistoryListBox_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm is null) return;

            _vm.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;

            if (_vm.Visibility == Visibility.Visible)
            {
                await _vm.UpdateItemsAsync(CancellationToken.None);
                this.ListBox.UpdateLayout();

                await Task.Yield();

                if (this.ListBox.SelectedIndex < 0) this.ListBox.SelectedIndex = 0;
                FocusSelectedItem(_focusRequest);
                _focusRequest = false;
            }
        }

        private void HistoryListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
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

            _vm.UpdateItems(true, CancellationToken.None);
            return this.ListBox.Items?.Cast<BookHistory>().ToList();
        }

        public List<BookHistory> GetSelectedItems()
        {
            return this.ListBox.SelectedItems.Cast<BookHistory>().ToList();
        }

        public void SetSelectedItems(IEnumerable<BookHistory>? selectedItems)
        {
            var sources = GetItems();
            if (sources is null) return;

            var items = selectedItems?.Intersect(sources).ToList();
            this.ListBox.SetSelectedItems(items);
            this.ListBox.ScrollItemsIntoView(items);
        }

        #endregion UI Accessor
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
