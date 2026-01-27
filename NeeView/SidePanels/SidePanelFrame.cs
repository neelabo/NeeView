using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace NeeView
{
    public class VisibleAtOnceRequestEventArgs : EventArgs
    {
        public VisibleAtOnceRequestEventArgs(string key)
            : this(key, true)
        {
        }

        public VisibleAtOnceRequestEventArgs(string key, bool isVisible)
        {
            Key = key;
            IsVisible = isVisible;
        }

        public string Key { get; init; }
        public bool IsVisible { get; init; }
    }

    /// <summary>
    /// NeeView用 サイドパネル管理
    /// </summary>
    public class SidePanelFrame : BindableBase
    {
        static SidePanelFrame() => Current = new SidePanelFrame();
        public static SidePanelFrame Current { get; }


        private bool _isVisibleLocked;


        private SidePanelFrame()
        {
            CustomLayoutPanelMessenger.CollectionChanged += (s, e) => RaisePanelPropertyChanged();
        }



        // パネル表示要求
        public event EventHandler<VisibleAtOnceRequestEventArgs>? VisibleAtOnceRequest;


        // サイドバー表示ロック。自動非表示にならないようにする
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        ///  タッチスクロール終端挙動汎用
        /// </summary>
        public void ScrollViewer_ManipulationBoundaryFeedback(object? sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            if (!Config.Current.Panels.IsManipulationBoundaryFeedbackEnabled)
            {
                e.Handled = true;
            }
        }


        #region Panels Visibility

        public void RaisePanelPropertyChanged()
        {
            RaisePropertyChanged(nameof(IsVisibleFolderList));
            RaisePropertyChanged(nameof(IsVisibleHistoryList));
            RaisePropertyChanged(nameof(IsVisibleBookmarkList));
            RaisePropertyChanged(nameof(IsVisiblePageList));
            RaisePropertyChanged(nameof(IsVisibleFileInfo));
            RaisePropertyChanged(nameof(IsVisibleEffectInfo));
            RaisePropertyChanged(nameof(IsVisibleNavigator));
            RaisePropertyChanged(nameof(IsVisiblePlaylist));
        }


        public void VisibleAtOnce(string key, bool isVisible = true)
        {
            VisibleAtOnceRequest?.Invoke(this, new VisibleAtOnceRequestEventArgs(key, isVisible));
        }

        private static bool IsVisiblePanel(string key)
        {
            return CustomLayoutPanelManager.Current.IsPanelSelected(key);
        }

        private void SetVisiblePanel(string key, bool isVisible, bool isFocus = true)
        {
            CustomLayoutPanelManager.Current.SelectPanel(key, isVisible, isFocus);
            RaisePanelPropertyChanged();
        }

        private bool ToggleVisiblePanel(string key, bool byMenu)
        {
            bool isVisible = !CustomLayoutPanelManager.Current.IsPanelSelected(key) || (!byMenu && !CustomLayoutPanelManager.Current.IsPanelVisible(key));
            SetVisiblePanel(key, isVisible);
            return isVisible;
        }



        // ファイル情報表示ON/OFF
        public bool IsVisibleFileInfo
        {
            get { return IsVisiblePanel(nameof(FileInformationPanel)); }
            set { SetVisiblePanel(nameof(FileInformationPanel), value); }
        }

        // TODO: flushって？
        public void SetVisibleFileInfo(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(FileInformationPanel), isVisible);
        }

        public bool ToggleVisibleFileInfo(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(FileInformationPanel), byMenu);
        }



        // エフェクト情報表示ON/OFF
        public bool IsVisibleEffectInfo
        {
            get { return IsVisiblePanel(nameof(ImageEffectPanel)); }
            set { SetVisiblePanel(nameof(ImageEffectPanel), value); }
        }

        public void SetVisibleEffectInfo(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(ImageEffectPanel), isVisible);
        }

        public bool ToggleVisibleEffectInfo(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(ImageEffectPanel), byMenu);
        }



        // ナビゲートパネル情報表示ON/OFF
        public bool IsVisibleNavigator
        {
            get { return IsVisiblePanel(nameof(NavigatePanel)); }
            set { SetVisiblePanel(nameof(NavigatePanel), value); }
        }

        public void SetVisibleNavigator(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(NavigatePanel), isVisible);
        }

        public bool ToggleVisibleNavigator(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(NavigatePanel), byMenu);
        }


        // フォルダーリスト表示ON/OFF
        public bool IsVisibleFolderList
        {
            get { return IsVisiblePanel(nameof(FolderPanel)); }
            set { SetVisiblePanel(nameof(FolderPanel), value); }
        }

        public void SetVisibleFolderList(bool isVisible, bool flush, bool isFocus)
        {
            SetVisiblePanel(nameof(FolderPanel), isVisible, isFocus);
        }

        public bool ToggleVisibleFolderList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(FolderPanel), byMenu);
        }


        // ページリスト
        public bool IsVisiblePageList
        {
            get { return IsVisiblePanel(nameof(PageListPanel)); }
            set { SetVisiblePanel(nameof(PageListPanel), value); }
        }

        public void SetVisiblePageList(bool isVisible, bool flush, bool isFocus)
        {
            SetVisiblePanel(nameof(PageListPanel), isVisible, isFocus);
        }

        public bool ToggleVisiblePageList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(PageListPanel), byMenu);
        }


        // 履歴リスト表示ON/OFF
        public bool IsVisibleHistoryList
        {
            get { return IsVisiblePanel(nameof(HistoryPanel)); }
            set { SetVisiblePanel(nameof(HistoryPanel), value); }
        }

        public void SetVisibleHistoryList(bool isVisible, bool flush, bool isFocus)
        {
            SetVisiblePanel(nameof(HistoryPanel), isVisible, isFocus);
        }

        public bool ToggleVisibleHistoryList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(HistoryPanel), byMenu);
        }


        // ブックマークリスト表示ON/OFF
        public bool IsVisibleBookmarkList
        {
            get { return IsVisiblePanel(nameof(BookmarkPanel)); }
            set { SetVisiblePanel(nameof(BookmarkPanel), value); }
        }

        public void SetVisibleBookmarkList(bool isVisible, bool flush, bool isFocus)
        {
            SetVisiblePanel(nameof(BookmarkPanel), isVisible, isFocus);
        }

        public bool ToggleVisibleBookmarkList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(BookmarkPanel), byMenu);
        }

        // プレイリスト表示ON/OFF
        public bool IsVisiblePlaylist
        {
            get { return IsVisiblePanel(nameof(PlaylistPanel)); }
            set { SetVisiblePanel(nameof(PlaylistPanel), value); }
        }

        public void SetVisiblePlaylist(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(PlaylistPanel), isVisible);
        }

        public bool ToggleVisiblePlaylist(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(PlaylistPanel), byMenu);
        }

        #endregion Panels Visibility

        #region Bookshelf parts

        /// <summary>
        /// 本棚のブックマークグループを表示
        /// </summary>
        public bool FocusBookshelfBookmarkList(bool byMenu)
        {
            // フォルダーツリーは「ブックマークリスト」を選択した状態にする
            FolderPanel.Current.FolderTreeModel?.SelectRootBookmarkFolder();
            BookshelfFolderList.Current.RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.Refresh);

            // フォルダーリスト選択
            SetVisiblePanel(nameof(FolderPanel), true);
            RaisePanelPropertyChanged();

            // フォルダーリストにフォーカスをあわせる
            if (!byMenu && IsVisibleFolderList)
            {
                BookshelfFolderList.Current.FocusAtOnce();
            }

            return IsVisibleFolderList;
        }

        /// <summary>
        /// 本棚の検索ボックスにフォーカスを移す
        /// </summary>
        public void FocusBookshelfSearchBox(bool byMenu)
        {
            SetVisibleFolderList(true, true, false);
            BookshelfFolderList.Current.RaiseSearchBoxFocus();
        }

        /// <summary>
        /// ブックマークの検索ボックスにフォーカスを移す
        /// </summary>
        public void FocusBookmarkSearchBox(bool byMenu)
        {
            SetVisibleBookmarkList(true, true, false);
            BookmarkFolderList.Current.RaiseSearchBoxFocus();
        }

        /// <summary>
        /// ページリストの検索ボックスにフォーカスを移す
        /// </summary>
        public void FocusPageListSearchBox(bool byMenu)
        {
            SetVisiblePageList(true, true, false);
            PageList.Current.RaiseSearchBoxFocus();
        }

        /// <summary>
        /// 履歴の検索ボックスにフォーカスを移す
        /// </summary>
        public void FocusHistorySearchBox(bool byMenu)
        {
            SetVisibleHistoryList(true, true, false);
            HistoryList.Current.RaiseSearchBoxFocus();
        }

        // TODO: 本棚専用コマンドなのにサイドパネルに定義されているのはよろしくない
        /// <summary>
        /// フォルダーツリー表示状態
        /// </summary>
        public bool IsVisibleBookshelfFolderTree
        {
            get { return Config.Current.Bookshelf.IsFolderTreeVisible && IsVisibleFolderList; }
            set { SetVisibleBookshelfFolderTree(false, value); }
        }

        /// <summary>
        /// フォルダーツリー表示状態切替
        /// </summary>
        public bool ToggleVisibleBookshelfFolderTree(bool byMenu)
        {
            return SetVisibleBookshelfFolderTree(byMenu, !IsVisibleBookshelfFolderTree || !IsVisiblePanel(nameof(FolderPanel)));
        }

        /// <summary>
        /// フォルダーツリー表示状設定
        /// </summary>
        public bool SetVisibleBookshelfFolderTree(bool byMenu, bool isVisible)
        {
            Debug.WriteLine($"{isVisible}, {IsVisiblePanel(nameof(FolderPanel))}");

            // フォーカス要求。表示前に要求する
            if (!byMenu && isVisible)
            {
                FolderPanel.Current.FolderTreeModel?.FocusAtOnce();
            }

            Config.Current.Bookshelf.IsFolderTreeVisible = isVisible;

            SetVisiblePanel(nameof(FolderPanel), true);
            RaisePanelPropertyChanged();

            return Config.Current.Bookshelf.IsFolderTreeVisible;
        }

        #endregion Bookshelf parts

        #region Bookmark parts


        /// <summary>
        /// フォルダーツリー表示状設定
        /// </summary>
        public bool SetVisibleBookmarkFolderTree(bool byMenu, bool isVisible)
        {
            Debug.WriteLine($"{isVisible}, {IsVisiblePanel(nameof(BookmarkPanel))}");

            // フォーカス要求。表示前に要求する
            if (!byMenu && isVisible)
            {
                //BookmarkFolderTreeModel.Current?.FocusAtOnce();
                BookmarkPanel.Current.FolderTreeModel.FocusAtOnce();
            }

            Config.Current.Bookmark.IsFolderTreeVisible = isVisible;

            SetVisiblePanel(nameof(BookmarkPanel), true);
            RaisePanelPropertyChanged();

            return Config.Current.Bookmark.IsFolderTreeVisible;
        }

        #endregion

    }
}
