using System.Windows;

namespace NeeView.Windows
{
    /// <summary>
    /// 各種ウィンドウシステムパラメータをまとめたもの
    /// </summary>
    public static class WindowParameters
    {
        // TODO: SystemVisualParameter との関係
        // TODO: Windows7Tools とか Windows10Tools との関係

        private static TabletModeWatcher? _tabletModeWatcher;


        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="window"></param>
        public static void Initialize(Window window)
        {
            _tabletModeWatcher = new TabletModeWatcher(window);
        }

        /// <summary>
        /// タブレットモード判定
        /// </summary>
        public static bool IsTabletMode
        {
            get
            {
                if (_tabletModeWatcher is null) return false;
                return _tabletModeWatcher.IsTabletMode;
            }
        }
    }
}
