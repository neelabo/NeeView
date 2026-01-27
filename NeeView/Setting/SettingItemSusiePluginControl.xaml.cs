using NeeLaboratory.Generators;
using NeeLaboratory.Windows.Input;
using NeeView.Susie;
using NeeView.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemSusiePluginControl.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class SettingItemSusiePluginControl : UserControl, INotifyPropertyChanged
    {
        private readonly SusiePluginType _pluginType;


        public SettingItemSusiePluginControl(SusiePluginType pluginType)
        {
            InitializeComponent();

            this.DragDataFormat = "SusiePlugin." + pluginType.ToString();

            this.Root.DataContext = this;

            _pluginType = pluginType;

            var binding = new Binding(pluginType == SusiePluginType.Image ? nameof(SusiePluginManager.INPlugins) : nameof(SusiePluginManager.AMPlugins)) { Source = SusiePluginManager.Current, Mode = BindingMode.OneWay };
            this.PluginList.SetBinding(ListBox.ItemsSourceProperty, binding);
            this.PluginList.SetBinding(ListBox.TagProperty, binding);
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public string DragDataFormat { get; private set; }


        #region Commands

        private RelayCommand? _configCommand;
        public RelayCommand ConfigCommand
        {
            get { return _configCommand = _configCommand ?? new RelayCommand(OpenConfigDialog_Executed, CanOpenConfigDialog); }
        }

        private bool CanOpenConfigDialog()
        {
            return this.PluginList.SelectedItem is SusiePluginInfo;
        }

        private void OpenConfigDialog_Executed()
        {
            if (this.PluginList.SelectedItem is not SusiePluginInfo item) return;

            OpenConfigDialog(item);
        }

        private void OpenConfigDialog(SusiePluginInfo spi)
        {
            if (spi == null) return;

            var dialog = new SusiePluginSettingWindow(spi);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();

            SusiePluginManager.Current.FlushSusiePlugin(spi.Name);
            SusiePluginManager.Current.UpdateSusiePlugin(spi.Name);
            UpdateExtensions();
        }


        private RelayCommand? _moveUpCommand;
        public RelayCommand MoveUpCommand
        {
            get { return _moveUpCommand = _moveUpCommand ?? new RelayCommand(MoveUpCommand_Executed); }
        }

        private void MoveUpCommand_Executed()
        {
            var index = this.PluginList.SelectedIndex;
            if (this.PluginList.Tag is not ObservableCollection<SusiePluginInfo> collection) return;

            if (index > 0)
            {
                collection.Move(index, index - 1);
                this.PluginList.ScrollIntoView(this.PluginList.SelectedItem);
            }
        }

        private RelayCommand? _moveDownCommand;
        public RelayCommand MoveDownCommand
        {
            get { return _moveDownCommand = _moveDownCommand ?? new RelayCommand(MoveDownCommand_Executed); }
        }

        private void MoveDownCommand_Executed()
        {
            var index = this.PluginList.SelectedIndex;
            if (this.PluginList.Tag is not ObservableCollection<SusiePluginInfo> collection) return;

            if (index >= 0 && index < collection.Count - 1)
            {
                collection.Move(index, index + 1);
                this.PluginList.ScrollIntoView(this.PluginList.SelectedItem);
            }
        }

        private RelayCommand? _switchAllCommand;
        public RelayCommand SwitchAllCommand
        {
            get { return _switchAllCommand = _switchAllCommand ?? new RelayCommand(SwitchAllCommand_Executed); }
        }

        private void SwitchAllCommand_Executed()
        {
            if (this.PluginList.Tag is ObservableCollection<SusiePluginInfo> collection)
            {
                var flag = collection.Any(e => !e.IsEnabled);
                foreach (var plugin in collection)
                {
                    plugin.IsEnabled = flag;
                }

                SusiePluginManager.Current.FlushSusiePlugin(collection.ToList());
                UpdateExtensions();

                this.PluginList.Items.Refresh();
            }
        }

        #endregion


        // プラグインリスト：ドロップ受付判定
        private void PluginListView_PreviewDragOver(object? sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e, DragDataFormat);
        }

        private void PluginListView_PreviewDragEnter(object? sender, DragEventArgs e)
        {
            PluginListView_PreviewDragOver(sender, e);
        }

        // プラグインリスト：ドロップ
        private void PluginListView_Drop(object? sender, DragEventArgs e)
        {
            if ((sender as ListBox)?.Tag is ObservableCollection<SusiePluginInfo> list)
            {
                ListBoxDragSortExtension.Drop<SusiePluginInfo>(sender, e, DragDataFormat, list);
            }
        }


        // 選択項目変更
        private void PluginList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ConfigCommand.RaiseCanExecuteChanged();
        }

        // 項目ダブルクリック
        private void ListBoxItem_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBoxItem)?.DataContext is not SusiePluginInfo item) return;
            OpenConfigDialog(item);
        }

        private void ListBoxItem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((sender as ListBoxItem)?.DataContext is not SusiePluginInfo item) return;

                OpenConfigDialog(item);
            }
        }

        // 有効/無効チェックボックス
        private void CheckBox_Changed(object? sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox)?.DataContext is not SusiePluginInfo item) return;

            SusiePluginManager.Current.FlushSusiePlugin(item.Name);
            UpdateExtensions();
        }

        private void UpdateExtensions()
        {
            if (_pluginType == SusiePluginType.Image)
            {
                SusiePluginManager.Current.UpdateImageExtensions();
            }
            else
            {
                SusiePluginManager.Current.UpdateArchiveExtensions();
            }
        }
    }
}
