using NeeView.Native;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window, IHasRenameManager
    {
        /// <summary>
        /// このウィンドウが存在する間だけ設定されるインスタンス
        /// </summary>
        public static SettingWindow? Current { get; private set; }

        public SettingWindowViewModel? _vm;

        public SettingWindow()
        {
            NVInterop.NVFpReset();

            InitializeComponent();
        }

        public SettingWindow(SettingWindowModel model)
        {
            InitializeComponent();

            Current = this;

            DragDropHelper.AttachDragOverTerminator(this);

            this.Closing += SettingWindow_Closing;
            this.Closed += (s, e) => Current = null;
            this.KeyDown += SettingWindow_KeyDown;

            _vm = new SettingWindowViewModel(model);
            this.DataContext = _vm;

            _vm.AddPropertyChanged(nameof(SettingWindowViewModel.CurrentPage), UpdateIndexTreeSelected);

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.CloseAll();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.CloseAll();
        }


        /// <summary>
        /// 設定画面を閉じる時にデータ保存するフラグ
        /// </summary>
        public bool AllowSave { get; set; } = true;

        /// <summary>
        /// ファイル保存せずに終了
        /// </summary>
        public void Cancel()
        {
            AllowSave = false;
            Close();
        }


        private void SettingWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void SettingWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // 設定を閉じるとメインウィンドウが背後に隠れてしまう現象を抑制
            MainWindow.Current?.Activate();
        }

        private void SettingWindow_Closed(object? sender, EventArgs e)
        {
            if (this.AllowSave)
            {
                SaveDataSync.Current.SaveUserSetting(Config.Current.System.IsSyncUserSetting, true);
            }
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            this.PageContent.Focus();
        }

        private void IndexTree_SelectedItemChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_vm is null) return;

            if (this.IndexTree.SelectedItem is not SettingPage settingPage) return;

            _vm.SelectedItemChanged(settingPage);
        }

        private void UpdateIndexTreeSelected(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm is null) return;

            if (_vm.IsSearchPageSelected && this.IndexTree.SelectedItem is SettingPage settingPage)
            {
                settingPage.IsSelected = false;
            }
        }

        public RenameManager GetRenameManager()
        {
            return this.RenameManager;
        }
    }
}
