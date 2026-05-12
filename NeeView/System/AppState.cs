using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Threading;
using NeeView.Properties;
using NeeView.Setting;
using System;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NeeView
{
    public partial class AppState : ObservableObject, IDisposable
    {
        private static readonly Lazy<AppState> _current = new();
        public static AppState Current => _current.Value;


        private readonly TaskTray _taskTray = new();
        private readonly AsyncLock _asyncLock = new();
        private bool _disposedValue;

        public AppState()
        {
        }

        public bool IsTaskTrayEnabled
        {
            get { return _taskTray.IsEnabled; }
            set { _taskTray.IsEnabled = value; }
        }

        [ObservableProperty]
        public partial bool IsProcessingBook { get; set; }

        [ObservableProperty]
        public partial bool IsHideWindow { get; private set; }

        [ObservableProperty]
        public partial bool IsSuspended { get; set; }


        public void Initialize()
        {
            Config.Current.System.SubscribePropertyChanged(nameof(SystemConfig.IsTaskTrayEnabled),
                (s, e) => UpdateTaskTray());

            UpdateTaskTray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _taskTray.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// タスクトレイ更新
        /// </summary>
        private void UpdateTaskTray()
        {
            IsTaskTrayEnabled = Config.Current.System.IsTaskTrayEnabled || App.Current.Option.IsTray == SwitchOption.on;
        }

        /// <summary>
        /// アプリの中断
        /// </summary>
        /// <remarks>
        /// タスクトレイに格納してウィンドウを非表示にする
        /// </remarks>
        public async Task SuspendAsync()
        {
            if (IsHideWindow) return;

            using var scope = _asyncLock.Lock();

            // すべての Window を Hide
            IsHideWindow = true;

            // WndProc や Rendering イベント購読休止
            IsSuspended = true;

            if (!App.Current.Sequence.Initialized)
            {
                return;
            }

            // 設定ウィンドウを閉じる
            SettingWindow.Current?.Cancel();

            // スライドショー停止
            SlideShow.Current.IsPlayingSlideShow = false;

            // 設定ファイル監視の一時停止
            SaveDataSync.Current.Suspend();

            // 設定保存
            AppSaveData.SaveAll();

            // ブックを閉じる
            BookHub.Current.RequestUnload(this, true);

            // Script Engine 初期化
            ScriptManager.Current.CancelAll();
            CommandHostStaticResource.Current.Clear();

            // パネル解放
            BookshelfFolderList.Current.RequestPlaceClear();
            BookmarkFolderList.Current.RequestPlaceClear();
            PlaylistHub.Current?.IsHide = true;
            HistoryList.Current.IsHide = true;

            // 非同期処理もあるのでちょっと待つ
            await Task.Delay(500);

            // 各種メモリキャッシュのクリア
            BookMementoCollection.Current.ClearCache();
            ArchiveManager.Current.ClearCache();
            BookThumbnailPool.Current.Clear();
            ThumbnailLifetimeManagement.Current.Clear();

            // キャッシュDBのクリーンナップ
            ThumbnailCache.Current.Cleanup();

            // テンポラリファイル破棄？
            //Temporary.Current.RemoveTempFolder();

            // 非同期処理もあるのでちょっと待つ
            await Task.Delay(500);

            // GC
            await GarbageCollectAsync();
        }

        /// <summary>
        /// アプリ中断時のGC
        /// </summary>
        private async Task GarbageCollectAsync()
        {
            await App.Current.Dispatcher.BeginInvoke(() =>
            {
                // LOH 圧縮が必要なときだけ
                if (ShouldCompactLOH())
                {
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            },
            DispatcherPriority.ApplicationIdle);
        }

        private bool ShouldCompactLOH()
        {
            var info = GC.GetGCMemoryInfo();
            return info.FragmentedBytes > 50 * 1024 * 1024; // 例：50MB 以上断片化していたら圧縮
        }

        /// <summary>
        /// アプリ表示
        /// </summary>
        /// <remarks>
        /// タスクトレイからの要求。
        /// ウィンドウあるNeeViewプロセスがあればそれをアクティブにする。なければ自身を再開する。
        /// </remarks>
        public async Task ShowWindow()
        {
            var activeProcess = MultiBootService.GetLatestActiveProcess();
            if (activeProcess is not null)
            {
                ProcessActivator.AppActivate(activeProcess);
                return;
            }

            if (IsHideWindow)
            {
                await ResumeAsync([]);
            }
            else
            {
                MainWindow.Current.Activate();
            }
        }

        /// <summary>
        /// アプリ再開
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        public async Task ResumeAsync(string[] args)
        {
            var options = App.Current.ParseCommandLineOption(args);

            if (!IsHideWindow)
            {
                // ウィンドウがアクティブな場合はブックのロード要求のみ
                if (options.Values.Count > 0)
                {
                    BookHubTools.RequestLoad(this, options.Values, BookLoadOption.FocusOnLoaded, true, null);
                }
                return;
            }
            else
            {
                using var scope = _asyncLock.Lock();

                if (IsCriticalSettingChanged(options))
                {
                    // 致命的な設定の変更がある場合は再起動する
                    App.Current.Reboot(args);
                    return;
                }
                else
                {
                    // 最新の設定でアプリを再開する

                    // コマンドラインオプションを適用
                    App.Current.SetCommandLineOption(options);

                    // UserSettings 更新
                    AppSaveData.Current.LoadUserSetting();

                    // WinProc イベント購読再開
                    IsSuspended = false;

                    // Window 座標を復元
                    MainWindow.InitializeWindowShapeSnap();
                    MainWindow.Current.RestoreWindowPlacement();
                    IsHideWindow = false;

                    await App.Current.Sequence.ResumeAsync();
                }
            }
        }

        /// <summary>
        /// 致命的な設定の変更？
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static bool IsCriticalSettingChanged(CommandLineOption options)
        {
            if (options.SettingFilename != App.Current.Option.SettingFilename)
            {
                return true;
            }

            var culture = TextResources.ValidateCultureInfo(options.Language ?? Config.Current.System.Language);
            if (culture != TextResources.Culture)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// アプリ終了コマンド
        /// </summary>
        /// <remarks>
        /// タスクトレイを解除して完全終了する。
        /// </remarks>
        public void Shutdown(bool runShutdown = true)
        {
            IsTaskTrayEnabled = false;

            if (IsHideWindow)
            {
                // 非表示状態からの終了ではデータ保存しない
                SaveDataSync.Current.Dispose();
                SaveData.Current.DisableSave();
            }

            if (runShutdown)
            {
                App.Current.Shutdown();
            }
        }
    }
}
