﻿using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace NeeView
{
    /// <summary>
    /// MainWindow : ViewModel
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        #region SidePanels

        /// <summary>
        /// SidePanelMargin property.
        /// メニュの自動非表示ON/OFFによるサイドパネル上部の余白
        /// </summary>
        public Thickness SidePanelMargin
        {
            get { return _SidePanelMargin; }
            set { if (_SidePanelMargin != value) { _SidePanelMargin = value; RaisePropertyChanged(); } }
        }

        //
        private Thickness _SidePanelMargin;

        //
        private void UpdateSidePanelMargin()
        {
            SidePanelMargin = new Thickness(0, _model.CanHideMenu ? 32 : 0, 0, _model.CanHidePageSlider ? 20 : 0);
        }


        /// <summary>
        /// CanvasWidth property.
        /// キャンバスサイズ。SidePanelから引き渡される
        /// </summary>
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set { if (_CanvasWidth != value) { _CanvasWidth = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasWidth;


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set { if (_CanvasHeight != value) { _CanvasHeight = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasHeight;

        #endregion

        #region Window Icon

        // ウィンドウアイコン：標準
        private ImageSource? _windowIconDefault;

        // ウィンドウアイコン初期化
        private void InitializeWindowIcons()
        {
            _windowIconDefault = null;
        }

        // 現在のウィンドウアイコン取得
        public ImageSource? WindowIcon => _windowIconDefault;

        #endregion


        private bool _initialized;
        private MainWindowModel _model;
        private readonly MainViewComponent _viewComponent;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindowViewModel(MainWindowModel model)
        {
            _viewComponent = MainViewComponent.Current;

            MenuAutoHideDescription = new MenuAutoHideDescription(MainWindow.Current.LayerMenuSocket, MainWindow.Current.SidePanelFrame);
            StatusAutoHideDescription = new StatusAutoHideDescription(MainWindow.Current.LayerStatusArea, MainWindow.Current.SidePanelFrame);
            ThumbnailListAutoHideDescription = new StatusAutoHideDescription(MainWindow.Current.LayerThumbnailListSocket, MainWindow.Current.SidePanelFrame);

            // icon
            InitializeWindowIcons();

            // main window model
            _model = model;

            _model.AddPropertyChanged(nameof(_model.CanHideMenu),
                (s, e) => UpdateSidePanelMargin());

            _model.AddPropertyChanged(nameof(_model.CanHidePageSlider),
                (s, e) =>
                {
                    UpdateSidePanelMargin();
                    RaisePropertyChanged(nameof(CanHideThumbnailList));
                });

            _model.AddPropertyChanged(nameof(_model.CanHidePanel),
                (s, e) => UpdateSidePanelMargin());

            _model.FocusMainViewCall += Model_FocusMainViewCall;

            _model.VisibleAtOnceRequest += Model_VisibleAtOnceRequest;

            MainWindow.Current.Activated +=
                (s, e) => RaisePropertyChanged(nameof(IsMenuBarActive));

            MainWindow.Current.Deactivated +=
                (s, e) => RaisePropertyChanged(nameof(IsMenuBarActive));

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsTopmost),
                (s, e) => RaisePropertyChanged(nameof(IsTopmost)));

            ThumbnailList.Current.AddPropertyChanged(nameof(CanHideThumbnailList),
                (s, e) => RaisePropertyChanged(nameof(CanHideThumbnailList)));

            BookHub.Current.BookChanged +=
                (s, e) => CommandManager.InvalidateRequerySuggested();

            Environment.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    SaveData.Current.DisableSave(); // 保存禁止
                    App.Current.MainWindow.Close();
                };

            PageTitle.Current.SubscribePropertyChanged(nameof(PageTitle.Title),
                (s, e) => AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(Title))));

            // TODO: アプリの初期化処理で行うべき
            // ダウンロードフォルダー生成
            if (!System.IO.Directory.Exists(Temporary.Current.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.Current.TempDownloadDirectory);
            }
        }


        public event EventHandler? FocusMainViewCall;



        public bool IsClosing { get; set; }

        public string Title => PageTitle.Current.Title;

        public bool IsTopmost
        {
            get { return Config.Current.Window.IsTopmost; }
            set { Config.Current.Window.IsTopmost = value; }
        }

        // for Binding
        public MainWindowController WindowController => _model.WindowController;
        public WindowTitle WindowTitle => WindowTitle.Current;
        public PageTitle PageTitle => PageTitle.Current;
        public ThumbnailList ThumbnailList => ThumbnailList.Current;
        //public ContentCanvasBrush ContentCanvasBrush => _viewComponent.ContentCanvasBrush;
        public ImageEffect ImageEffect => ImageEffect.Current;
        public MouseInput MouseInput => _viewComponent.MouseInput;
        public InfoMessage InfoMessage => InfoMessage.Current;
        public SidePanelFrame SidePanel => SidePanelFrame.Current;
        //public ContentCanvas ContentCanvas => _viewComponent.ContentCanvas;
        //public LoupeTransform LoupeTransform => _viewComponent.LoupeTransform;
        public ToastService ToastService => ToastService.Current;
        public App App => App.Current;
        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;


        public MainWindowModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public bool CanHideThumbnailList
        {
            get
            {
                return ThumbnailList.CanHideThumbnailList && !Model.CanHidePageSlider;
            }
        }

        /// <summary>
        /// Menu用AutoHideBehavior補足
        /// </summary>
        public MenuAutoHideDescription MenuAutoHideDescription { get; }

        /// <summary>
        /// FilmStrep, Slider 用 AutoHideBehavior 補足
        /// </summary>
        public BasicAutoHideDescription StatusAutoHideDescription { get; }

        public BasicAutoHideDescription ThumbnailListAutoHideDescription { get; }

        public bool IsMenuBarActive => MainWindow.Current.IsActive;


        private void Model_FocusMainViewCall(object? sender, EventArgs e)
        {
            FocusMainViewCall?.Invoke(sender, e);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _model.Loaded();
            _model.ContentRendered();
        }


        /// <summary>
        /// Window OnLoaded
        /// </summary>
        public void Loaded()
        {
        }

        /// <summary>
        /// Window OnContentRendered
        /// </summary>
        public void ContentRendered()
        {
            Initialize();
        }

        /// <summary>
        /// ウィンドウがアクティブ化したときの処理
        /// </summary>
        public void Activated()
        {
            if (IsClosing) return;

            RoutedCommandTable.Current.UpdateInputGestures();
        }

        /// <summary>
        /// ウィンドウが非アクティブ化したときの処理
        /// </summary>
        public void Deactivated()
        {
            if (IsClosing) return;

            _ = ArchiveManager.Current.UnlockAllArchivesAsync();
        }

        /// <summary>
        /// パネルの一時表示要求
        /// </summary>
        private void Model_VisibleAtOnceRequest(object? sender, VisibleAtOnceRequestEventArgs e)
        {
            switch (e.Key)
            {
                case "Menu":
                    MenuAutoHideDescription.VisibleOnce(e.IsVisible);
                    break;

                case "Status":
                    StatusAutoHideDescription.VisibleOnce(e.IsVisible);
                    break;

                case "ThumbnailList":
                    ThumbnailListAutoHideDescription.VisibleOnce(e.IsVisible);
                    break;

                case "" or null:
                    MenuAutoHideDescription.VisibleOnce(e.IsVisible);
                    StatusAutoHideDescription.VisibleOnce(e.IsVisible);
                    ThumbnailListAutoHideDescription.VisibleOnce(e.IsVisible);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 自動表示パネルをすぐ閉じる
        /// </summary>
        public void AllPanelHideAtOnce()
        {
            _model.AllPanelHideAtOnce();
        }
    }
}
