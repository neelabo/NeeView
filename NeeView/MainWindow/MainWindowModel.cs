using Microsoft.Win32;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO.Search;
using NeeView.Properties;
using NeeView.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// MainWindow : Model
    /// </summary>
    public class MainWindowModel : BindableBase
    {
        private static MainWindowModel? _current;
        public static MainWindowModel Current => _current ?? throw new InvalidOperationException();


        // パネル表示ロック
        private bool _isPanelVisibleLocked;

        // 古いパネル表示ロック。コマンドでロックのトグルをできるようにするため
        private bool _isPanelVisibleLockedOld;

        private bool _canHidePageSlider;
        private bool _canHideLeftPanel;
        private bool _canHideRightPanel;
        private bool _canHideMenu;

        private volatile EditCommandWindow? _editCommandWindow;

        private readonly MainWindowController _windowController;


        public static void Initialize(MainWindowController windowController)
        {
            if (_current is not null) throw new InvalidOperationException();
            _current = new MainWindowModel(windowController);
        }

        private MainWindowModel(MainWindowController windowController)
        {
            if (_current is not null) throw new InvalidOperationException();
            _current = this;

            _windowController = windowController;

            _windowController.SubscribePropertyChanged(nameof(_windowController.AutoHideMode),
                (s, e) =>
                {
                    RefreshCanHideLeftPanel();
                    RefreshCanHideRightPanel();
                    RefreshCanHidePageSlider();
                    RefreshCanHideMenu();
                });

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenuInAutoHideMode), (s, e) =>
            {
                RefreshCanHideMenu();
            });

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenu), (s, e) =>
            {
                RefreshCanHideMenu();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsEnabled), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsHidePageSliderInAutoHideMode), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsHidePageSlider), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHideLeftPanelInAutoHideMode), (s, e) =>
            {
                RefreshCanHideLeftPanel();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHideRightPanelInAutoHideMode), (s, e) =>
            {
                RefreshCanHideRightPanel();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHideLeftPanel), (s, e) =>
            {
                RefreshCanHideLeftPanel();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHideRightPanel), (s, e) =>
            {
                RefreshCanHideRightPanel();
            });

            RefreshCanHideLeftPanel();
            RefreshCanHideRightPanel();
            RefreshCanHidePageSlider();

            PageViewRecorder.Initialize();

            // 検索で使用する日時フォーマット指定
            SearchDateTimeTools.SetDateTimeFormatInfo(new LocalDateTimeFormatInfo());
        }

        private class LocalDateTimeFormatInfo : IDateTimeFormatInfo
        {
            public string GetPattern() => DateTimeTools.DateTimePattern;
        }


        public event EventHandler? FocusMainViewCall;

        public event EventHandler<VisibleAtOnceRequestEventArgs>? VisibleAtOnceRequest;


        public MainWindowController WindowController => _windowController;


        /// <summary>
        /// メニューを自動非表示するか
        /// </summary>
        public bool CanHideMenu
        {
            get { return _canHideMenu; }
            set { SetProperty(ref _canHideMenu, value); }
        }

        /// <summary>
        /// スライダーを自動非表示するか
        /// </summary>
        public bool CanHidePageSlider
        {
            get { return _canHidePageSlider; }
            set { SetProperty(ref _canHidePageSlider, value); }
        }

        /// <summary>
        /// 左パネルを自動非表示するか
        /// </summary>
        public bool CanHideLeftPanel
        {
            get { return _canHideLeftPanel; }
            set { SetProperty(ref _canHideLeftPanel, value); }
        }

        /// <summary>
        /// 右パネルを自動非表示するか
        /// </summary>
        public bool CanHideRightPanel
        {
            get { return _canHideRightPanel; }
            set { SetProperty(ref _canHideRightPanel, value); }
        }

        /// <summary>
        /// パネル表示状態をロックする
        /// </summary>
        public bool IsPanelVisibleLocked
        {
            get { return _isPanelVisibleLocked; }
            set
            {
                if (_isPanelVisibleLocked != value)
                {
                    _isPanelVisibleLocked = value;
                    RaisePropertyChanged();
                    SidePanelFrame.Current.IsVisibleLocked = _isPanelVisibleLocked;
                }
            }
        }


        private void RefreshCanHideMenu()
        {
            CanHideMenu = Config.Current.MenuBar.IsHideMenu || (Config.Current.MenuBar.IsHideMenuInAutoHideMode && _windowController.AutoHideMode);
        }

        private void RefreshCanHidePageSlider()
        {
            CanHidePageSlider = Config.Current.Slider.IsEnabled && (Config.Current.Slider.IsHidePageSlider || (Config.Current.Slider.IsHidePageSliderInAutoHideMode && _windowController.AutoHideMode));
        }

        public void RefreshCanHideLeftPanel()
        {
            CanHideLeftPanel = Config.Current.Panels.IsHideLeftPanel || (Config.Current.Panels.IsHideLeftPanelInAutoHideMode && _windowController.AutoHideMode);
        }

        public void RefreshCanHideRightPanel()
        {
            CanHideRightPanel = Config.Current.Panels.IsHideRightPanel || (Config.Current.Panels.IsHideRightPanelInAutoHideMode && _windowController.AutoHideMode);
        }

        public bool ToggleHideMenu()
        {
            Config.Current.MenuBar.IsHideMenu = !Config.Current.MenuBar.IsHideMenu;
            return Config.Current.MenuBar.IsHideMenu;
        }

        public bool ToggleHidePageSlider()
        {
            Config.Current.Slider.IsHidePageSlider = !Config.Current.Slider.IsHidePageSlider;
            return Config.Current.Slider.IsHidePageSlider;
        }

        public bool ToggleVisibleAddressBar()
        {
            Config.Current.MenuBar.IsAddressBarEnabled = !Config.Current.MenuBar.IsAddressBarEnabled;
            return Config.Current.MenuBar.IsAddressBarEnabled;
        }

        // 起動時処理
        public void Loaded()
        {
            // NOTE: 必要であればエクスプローラーメニューを更新
            // NOTE: 都度追従させるほどではないので、無効化して様子を見る
#if false
            if (App.Current.Option.Language is null)
            {
                ExplorerContextMenu.Current.Update();
            }
#endif

            // サイドパネル復元
            CustomLayoutPanelManager.Current.Restore();

            // Susie起動
            // TODO: 非同期化できないか？
            SusiePluginManager.Current.Initialize();

            // 履歴読み込み
            SaveData.Current.LoadHistory();

            // ブックマーク読み込み
            SaveData.Current.LoadBookmark();

            // プレイリスト読み込み
            PlaylistHub.Current.Initialize();

            // SaveDataSync活動開始
            SaveDataSync.Current.Initialize();

            // 最初のブック、フォルダを開く
            new FirstLoader().Load();

            // 最初のブックマークを開く
            BookmarkFolderList.Current.UpdateItems();

            // オプション指定があればフォルダーリスト表示
            if (App.Current.Option.FolderListQuery is not null)
            {
                SidePanelFrame.Current.IsVisibleFolderList = true;
            }

            // スライドショーの自動再生
            if (App.Current.Option.IsSlideShow != null ? App.Current.Option.IsSlideShow == SwitchOption.on : Config.Current.StartUp.IsAutoPlaySlideShow)
            {
                SlideShow.Current.Play();
            }

            AppDispatcher.BeginInvoke(async () =>
            {
                // パネル初期化待機
                await BookmarkFolderList.Current.WaitAsync(CancellationToken.None);
                await BookshelfFolderList.Current.WaitAsync(CancellationToken.None);

                // 起動時スクリプトの実行
                if (App.Current.Option.ScriptQuery is not null)
                {
                    var path = App.Current.Option.ScriptQuery.ResolvePath().SimplePath;
                    if (!string.IsNullOrEmpty(path))
                    {
                        ScriptManager.Current.Execute(this, path, null, null);
                    }
                }

                // Script: OnStartup
                CommandTable.Current.TryExecute(this, ScriptCommand.EventOnStartup, null, CommandOption.None);
            });

#if DEBUG
            // [開発用] デバッグアクションの実行
            DebugCommand.Execute(App.Current.Option.DebugCommand);
#endif
        }

        public void ContentRendered()
        {
            if (Config.Current.History.IsAutoCleanupEnabled)
            {
                Task.Run(() => BookHistoryCollection.Current.RemoveUnlinkedAsync(CancellationToken.None));
            }
        }

        // ダイアログでファイル選択して画像を読み込む
        public void LoadAs()
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultDirectory = GetDefaultFolder();

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                LoadAs(dialog.FileName);
            }
        }

        public void LoadAs(string path)
        {
            BookHub.Current.RequestLoad(this, path, null, BookLoadOption.None, true);
        }

        // ファイルを開く基準となるフォルダーを取得
        private string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            var book = BookHub.Current.GetCurrentBook();
            if (Config.Current.System.IsOpenBookAtCurrentPlace && book != null)
            {
                return System.IO.Path.GetDirectoryName(book.Path) ?? "";
            }
            else
            {
                return "";
            }
        }


        // 設定ウィンドウを開く
        public void OpenSettingWindow()
        {
            if (Setting.SettingWindow.Current != null)
            {
                if (Setting.SettingWindow.Current.WindowState == WindowState.Minimized)
                {
                    Setting.SettingWindow.Current.WindowState = WindowState.Normal;
                }
                Setting.SettingWindow.Current.Activate();
                return;
            }

            var dialog = new Setting.SettingWindow(new Setting.SettingWindowModel());
            dialog.Owner = App.Current.MainWindow;
            dialog.Width = MathUtility.Clamp(App.Current.MainWindow.ActualWidth - 100, 640, 1280);
            dialog.Height = MathUtility.Clamp(App.Current.MainWindow.ActualHeight - 100, 480, 2048);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Show();
        }

        // 設定ウィンドウを閉じる
        public bool CloseSettingWindow()
        {
            if (Setting.SettingWindow.Current != null)
            {
                Setting.SettingWindow.Current.AllowSave = false;
                Setting.SettingWindow.Current.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        // 設定ウィンドウの表示/非表示を切り替える
        public void ToggleSettingWindow()
        {
            if (Setting.SettingWindow.Current is null || Setting.SettingWindow.Current.WindowState == WindowState.Minimized)
            {
                OpenSettingWindow();
            }
            else
            {
                CloseSettingWindow();
            }
        }

        // コマンド設定を開く
        public void OpenCommandParameterDialog(string command)
        {
            var dialog = new EditCommandWindow(command, EditCommandWindowTab.Default);
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            try
            {
                _editCommandWindow = dialog;
                if (_editCommandWindow.ShowDialog() == true)
                {
                    // 設定の同期
                    SaveDataSync.Current.SaveUserSetting(Config.Current.System.IsSyncUserSetting, true);
                }
            }
            finally
            {
                _editCommandWindow = null;
            }
        }

        public void CloseCommandParameterDialog()
        {
            _editCommandWindow?.Close();
            _editCommandWindow = null;
        }


        // バージョン情報を表示する
        public void OpenVersionWindow()
        {
            var dialog = new VersionWindow();
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }


        // 設定ファイルの場所を開く
        public void OpenSettingFilesFolder()
        {
            if (Environment.IsAppxPackage)
            {
                new MessageDialog(TextResources.GetString("OpenSettingFolderErrorDialog.Message"), TextResources.GetString("OpenSettingFolderErrorDialog.Title")).ShowDialog();
                return;
            }

            ExternalProcess.OpenWithFileManager(Environment.LocalApplicationDataPath, true);
        }


        /// <summary>
        /// パネル表示ロック開始
        /// コマンドから呼ばれる
        /// </summary>
        public void EnterVisibleLocked()
        {
            this.IsPanelVisibleLocked = !_isPanelVisibleLockedOld;
            _isPanelVisibleLockedOld = _isPanelVisibleLocked;
        }

        /// <summary>
        /// パネル表示ロック解除
        /// 他の操作をした場所から呼ばれる
        /// </summary>
        public void LeaveVisibleLocked()
        {
            if (_isPanelVisibleLocked)
            {
                _isPanelVisibleLockedOld = true;
                this.IsPanelVisibleLocked = false;
            }
            else
            {
                _isPanelVisibleLockedOld = false;
            }
        }

        /// <summary>
        /// 現在のスライダー方向を取得
        /// </summary>
        /// <returns></returns>
        public bool IsLeftToRightSlider()
        {
            if (PageFrameBoxPresenter.Current.IsMedia)
            {
                return true;
            }
            else
            {
                return !PageSlider.Current.IsSliderDirectionReversed;
            }
        }

        /// <summary>
        /// メインビューにフォーカスを移す
        /// </summary>
        public void FocusMainView()
        {
            FocusMainViewCall?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// パネルの自動非表示の一度の状態設定
        /// </summary>
        /// <param name="key">Menu or Status or ThumbnailList. "" ですべてのパネル</param>
        /// <param name="isVisible">表示/非表示</param>
        public void VisibleAtOnce(string key, bool isVisible)
        {
            VisibleAtOnceRequest?.Invoke(this, new VisibleAtOnceRequestEventArgs(key, isVisible));
        }

        /// <summary>
        /// すべての自動パネルをすぐ閉じる
        /// </summary>
        public void AllPanelHideAtOnce()
        {
            VisibleAtOnce("", false);
            SidePanelFrame.Current.VisibleAtOnce("", false);
        }
    }

}
