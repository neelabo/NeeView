using NeeView.ComponentModel;
using NeeView.Native;
using NeeView.Properties;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    // NOTE: Panels生成でインスタンスを参照しているため例外処理をCustomLayoutPanelManagerから分離させている
    // PanelsSource -> SidePaneFactory > FolderPanel -> FolderListView -> FolderTreeView -> SidePanelFrame:42
    public class CustomLayoutPanelMessenger
    {
        public static event EventHandler? CollectionChanged;

        public static void RaiseCollectionChanged(object? sender, EventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }
    }

    public class CustomLayoutPanelManager : LayoutPanelManager
    {
        private static CustomLayoutPanelManager? _current;
        public static CustomLayoutPanelManager Current => _current ?? throw new InvalidOperationException();

        public static void Initialize()
        {
            if (_current is not null) return;
            _current = new CustomLayoutPanelManager();

            // TODO: Panels生成でインスタンスを参照しているため処理をコンストラクタから分離させているがよろしくない
            // PanelsSource -> SidePaneFactory > FolderPanel -> FolderListView -> FolderTreeView -> SidePanelFrame:42
            //_current.InitializePanels();
        }

        // NTOE: 初期化前に復元を呼ばれる可能性があるため static メソッドを用意している
        public static void RestoreMaybe()
        {
            if (_current is null) return;
            _current.Restore();
        }

        // TODO: この変数は不要だと思う
        private readonly bool _initialized;

        private bool _isStoreEnabled = true;
        private readonly SidePanelProfile _sidePanelProfile;


        public CustomLayoutPanelManager()
        {
            _initialized = true;

            // NOTE: To be on the safe side, initialize the floating point processor.
            NVInterop.NVFpReset();

            _sidePanelProfile = new SidePanelProfile();
            _sidePanelProfile.Initialize();

            Resources["Floating"] = TextResources.GetString("LayoutPanel.Menu.Floating");
            Resources["Docking"] = TextResources.GetString("LayoutPanel.Menu.Docking");
            Resources["Close"] = TextResources.GetString("LayoutPanel.Menu.Close");

            WindowBuilder = new LayoutPanelWindowBuilder();

            var panelKeys = new[] {
                nameof(FolderPanel),
                nameof(PageListPanel),
                nameof(HistoryPanel),
                nameof(FileInformationPanel),
                nameof(NavigatePanel),
                nameof(ImageEffectPanel),
                nameof(BookmarkPanel),
                nameof(PlaylistPanel),
            };

            var panelLeftKeys = new[] { nameof(FolderPanel), nameof(PageListPanel), nameof(HistoryPanel) };
            var panelRightKeys = panelKeys.Except(panelLeftKeys).ToArray();

            PanelsSource = SidePanelFactory.CreatePanels(panelKeys).ToDictionary(e => e.TypeCode, e => e);
            Panels = LayoutPanelFactory.CreatePanels(PanelsSource.Values).ToDictionary(e => e.Key, e => e);

            LeftDock = new LayoutDockPanelContent(this);
            LeftDock.AddPanelRange(panelLeftKeys.Select(e => Panels[e]));

            RightDock = new LayoutDockPanelContent(this);
            RightDock.AddPanelRange(panelRightKeys.Select(e => Panels[e]));

            Docks = new Dictionary<string, LayoutDockPanelContent>()
            {
                ["Left"] = LeftDock,
                ["Right"] = RightDock,
            };

            Windows.Owner = App.Current.MainWindow;

            LeftDock.CollectionChanged += (s, e) => RaiseCollectionChanged(s, e);
            RightDock.CollectionChanged += (s, e) => RaiseCollectionChanged(s, e);
            Windows.CollectionChanged += (s, e) => RaiseCollectionChanged(s, e);
        }


        //public event EventHandler? CollectionChanged;


        public Dictionary<string, IPanel> PanelsSource { get; private set; }
        public LayoutDockPanelContent LeftDock { get; private set; }
        public LayoutDockPanelContent RightDock { get; private set; }



        private void RaiseCollectionChanged(object? sender, EventArgs e)
        {
            //CollectionChanged?.Invoke(sender, e);
            CustomLayoutPanelMessenger.RaiseCollectionChanged(sender, e);
        }

        public IPanel GetPanel(string key)
        {
            return PanelsSource[key];
        }

        public void SelectPanel(string key, bool isSelected, bool isFocus)
        {
            if (!_initialized) throw new InvalidOperationException();

            if (isSelected)
            {
                Open(key, isFocus);
            }
            else
            {
                Close(key);
            }
        }

        public void Open(string key, bool isFocus)
        {
            Open(Panels[key]);
            if (isFocus)
            {
                Focus(key);
            }
        }

        public void OpenDock(string key, bool isFocus)
        {
            OpenDock(Panels[key]);
            if (isFocus)
            {
                Focus(key);
            }
        }

        public void OpenWindow(string key, bool isFocus)
        {
            OpenWindow(Panels[key]);
            if (isFocus)
            {
                Focus(key);
            }
        }

        public void Close(string key)
        {
            Close(Panels[key]);
        }

        public void Focus(string key)
        {
            PanelsSource[key].Focus();
            SidePanelFrame.Current.VisibleAtOnce(key);
        }

        public bool IsPanelSelected(string key)
        {
            if (!_initialized) throw new InvalidOperationException();

            return IsPanelSelected(this.Panels[key]);
        }

        public bool IsPanelVisible(string key)
        {
            if (!_initialized) throw new InvalidOperationException();

            return IsPanelVisible(this.Panels[key]);
        }

        public bool IsPanelFloating(string key)
        {
            if (!_initialized) throw new InvalidOperationException();

            return IsPanelFloating(this.Panels[key]);
        }

        public void SetIsStoreEnabled(bool allow)
        {
            if (!_initialized) throw new InvalidOperationException();

            _isStoreEnabled = allow;
        }

        public void Store()
        {
            if (_initialized && _isStoreEnabled)
            {
                Config.Current.Panels.Layout = CreateMemento();
            }
        }

        public void Restore()
        {
            if (_initialized)
            {
                Restore(Config.Current.Panels.Layout);
            }
        }

        /// <summary>
        /// LayoutPanelWindow作成
        /// </summary>
        class LayoutPanelWindowBuilder : ILayoutPanelWindowBuilder
        {
            public LayoutPanelWindow CreateWindow(LayoutPanelWindowManager manager, LayoutPanel layoutPanel)
            {
                return new CustomLayoutPanelWindow(manager, layoutPanel);
            }
        }
    }
}
