using NeeLaboratory.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 初期化シーケンス
    /// </summary>
    public class AppSequence
    {
        private bool _initialized = false;
        private bool _initializeMinimized = false;

        public bool Initialized => _initialized;


        /// <summary>
        /// 復帰処理
        /// </summary>
        /// <returns></returns>
        public async Task ResumeAsync()
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }
            else
            {
                await ReinitializeAsync();
            }
        }

        /// <summary>
        /// 最小限の初期化
        /// </summary>
        public void InitializeMinimize()
        {
            if (_initializeMinimized) return;
            _initializeMinimized = true;

            // サイドパネル復元
            Trace.WriteLine("Restore layout panels");
            CustomLayoutPanelManager.Current.Restore();

            // Susie起動
            // TODO: 非同期化できないか？
            Trace.WriteLine("Initialize Susie");
            SusiePluginManager.Current.Initialize();

            // データ読み込み
            LoadData();
        }

        /// <summary>
        /// 起動時初期化
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;

            InitializeMinimize();

            // SaveDataSync活動開始
            Trace.WriteLine("Initialize SaveDataSync");
            SaveDataSync.Current.Initialize();

            await StartAsync();

#if DEBUG
            // [開発用] デバッグアクションの実行
            DebugCommand.Execute(App.Current.Option.DebugCommand);
#endif
        }

        /// <summary>
        /// 復帰時初期化
        /// </summary>
        public async Task ReinitializeAsync()
        {
            // SaveDataSync活動再開
            SaveDataSync.Current.Resume();

            // Playlist
            PlaylistHub.Current?.IsHide = false;

            // History
            SaveData.Current.LoadHistory();
            HistoryList.Current.SetSearchKeyword("");
            HistoryList.Current.IsHide = false;

            await StartAsync();
        }

        /// <summary>
        /// 初期状態を設定
        /// </summary>
        /// <returns></returns>
        private async Task StartAsync()
        {
            // 非同期初期化処理を待機
            Trace.WriteLine("Wait Process..");
            await ProcessJobEngine.Current.WaitPropertyAsync(nameof(ProcessJobEngine.IsBusy), x => x.IsBusy == false);

            // ブック初期化
            Trace.WriteLine("Load first book");
            LoadFirstBook();

            // スライドショーの自動再生
            PlaySlideshow();

            // パネル初期化待機
            Trace.WriteLine("Wait bookmark panel");
            await BookmarkFolderList.Current.WaitAsync(CancellationToken.None);
            
            Trace.WriteLine("Wait bookshelf panel");
            await BookshelfFolderList.Current.WaitAsync(CancellationToken.None);

            // 最初のスクリプト実行
            Trace.WriteLine("Run start script");
            RunStartupScript();
        }

        /// <summary>
        /// データのロード
        /// </summary>
        private static void LoadData()
        {
            // 履歴読み込み
            Trace.WriteLine("Load History");
            SaveData.Current.LoadHistory();

            // フォルダー設定読み込み
            Trace.WriteLine("Load FolderConfig");
            SaveData.Current.LoadFolderConfig();

            // ブックマーク読み込み
            Trace.WriteLine("Load Bookmark");
            SaveData.Current.LoadBookmark();

            // クイックアクセス読み込み
            Trace.WriteLine("Load QuickAccess");
            SaveData.Current.LoadQuickAccess();

            // プレイリスト読み込み
            Trace.WriteLine("Initialize PlaylistHub");
            PlaylistHub.Current.Initialize();
        }

        /// <summary>
        /// 最初のブック、本棚、ブックマークを開く
        /// </summary>
        private static void LoadFirstBook()
        {
            // 最初のブック、フォルダを開く
            new FirstLoader().Load();

            // オプション指定があればフォルダーリスト表示
            if (App.Current.Option.FolderListQuery is not null)
            {
                SidePanelFrame.Current.IsVisibleFolderList = true;
            }
        }

        /// <summary>
        /// スライドショーの自動再生
        /// </summary>
        private static void PlaySlideshow()
        {
            if (App.Current.Option.IsSlideShow != null ? App.Current.Option.IsSlideShow == SwitchOption.on : Config.Current.StartUp.IsAutoPlaySlideShow)
            {
                Trace.WriteLine("Play slideshow");
                SlideShow.Current.Play();
            }
        }

        /// <summary>
        /// 起動時スクリプトの実行
        /// </summary>
        public void RunStartupScript()
        {
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
        }
    }
}
