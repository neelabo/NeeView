//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class MainViewViewModel : BindableBase
    {
        private readonly MainViewComponent _viewComponent;
        private readonly PageFrameBoxPresenter _presenter;
        private readonly ContextMenu _contextMenu = new();
        private bool _isContextMenuDirty = true;
        private Visibility _busyVisibility = Visibility.Collapsed;
        private Page? _selectedPage;


        public MainViewViewModel(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
            _presenter = PageFrameBoxPresenter.Current;

            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.MainViewMargin),
                (s, e) => RaisePropertyChanged(nameof(MainViewMargin)));

            // context menu
            ContextMenuManager.Current.AddPropertyChanged(nameof(ContextMenuManager.Current.SourceTree),
                (s, e) => SetContextMenuDirty());

            RoutedCommandTable.Current.Changed +=
                (s, e) => SetContextMenuDirty();

            // busy visibility
            //_viewComponent.ContentRebuild.AddPropertyChanged(nameof(ContentRebuild.IsBusy),
            //    (s, e) => UpdateBusyVisibility());

            _viewComponent.SubscribePropertyChanged(nameof(MainViewComponent.IsProcessing),
                (s, e) => RaisePropertyChanged(nameof(IsProcessing)));

            BookOperation.Current.BookControl.AddPropertyChanged(nameof(IBookControl.IsBusy),
                (s, e) => UpdateBusyVisibility());

            _presenter.SubscribePropertyChanged(nameof(PageFrameBoxPresenter.IsLoading),
                (s, e) => UpdateBusyVisibility());

            _presenter.SubscribePropertyChanged(nameof(PageFrameBoxPresenter.View), Presenter_ViewChanged);

            _presenter.SubscribeViewContentChanged(Presenter_ViewContentChanged);
        }

        public MainViewComponent ViewComponent => _viewComponent;

        public PageFrameBoxPresenter PageFrameBoxPresenter => _presenter;

        //public ContentCanvas ContentCanvas => _viewComponent.ContentCanvas;

        //public ContentCanvasBrush ContentCanvasBrush => _viewComponent.ContentCanvasBrush;

        public ImageEffect ImageEffect => ImageEffect.Current;

        public WindowTitle WindowTitle => WindowTitle.Current;

        public InfoMessage InfoMessage => InfoMessage.Current;

        public LoupeContext LoupeContext => _viewComponent.LoupeContext;

        public MouseInput MouseInput => _viewComponent.MouseInput;

        public TouchInput TouchInput => _viewComponent.TouchInput;

        public Thickness MainViewMargin => new(Config.Current.View.MainViewMargin);

        public ContextMenu ContextMenu => _contextMenu;

        public Visibility BusyVisibility
        {
            get { return _busyVisibility; }
            set { if (_busyVisibility != value) { _busyVisibility = value; RaisePropertyChanged(); } }
        }

        // NOTE: 背景色をページに依存させる設定で使用される
        public Page? SelectedPage
        {
            get { return _selectedPage; }
            set { SetProperty(ref _selectedPage, value); }
        }

        public bool IsProcessing => _viewComponent.IsProcessing;


        private void Presenter_ViewChanged(object? sender, PropertyChangedEventArgs e)
        {
            var box = _presenter.View;
            SelectedPage = box is null || !box.Context.IsStaticFrame ? null : box.BookContext.SelectedPages.FirstOrDefault();
        }

        private void Presenter_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            if (e.State < ViewContentState.Loaded) return;
            if (_presenter.View is null) return;

            SelectedPage = _presenter.View.Context.IsStaticFrame ? e.ViewContents.FirstOrDefault()?.Page : null;
        }

        private void UpdateBusyVisibility()
        {
            ////Debug.WriteLine($"IsBusy: {BookHub.Current.IsLoading}, {BookOperation.Current.IsBusy}, {ContentRebuild.Current.IsBusy}");
            //this.BusyVisibility = Config.Current.Notice.IsBusyMarkEnabled && (BookHub.Current.IsLoading || BookOperation.Current.BookControl.IsBusy || _viewComponent.ContentRebuild.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
            this.BusyVisibility = Config.Current.Notice.IsBusyMarkEnabled && (_presenter.IsLoading || BookOperation.Current.BookControl.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
        }


        private void SetContextMenuDirty()
        {
            _isContextMenuDirty = true;
        }

        public void UpdateContextMenu()
        {
            if (!_isContextMenuDirty) return;
            _isContextMenuDirty = false;

            _contextMenu.Items.Clear();
            foreach (var item in ContextMenuManager.Current.CreateContextMenuItems())
            {
                _contextMenu.Items.Add(item);
            }

            _contextMenu.UpdateInputGestureText();
        }

        /// <summary>
        /// ウィンドウサイズをコンテンツサイズに合わせる
        /// </summary>
        /// <param name="window">ウィンドウ</param>
        /// <param name="canvasSize">現在のキャンバスサイズ</param>
        /// <param name="content">コンテンツ</param>
        public static void StretchWindow(Window window, Size canvasSize, PageFrameContent content)
        {
            if (window.WindowState != WindowState.Normal) return;

            var box = PageFrameBoxPresenter.Current.View;
            if (box is null) return;

            // スケールストレッチ強制有効
            using var forceTrack = box.Context.ForceScaleStretchTracking.Lock();

            // ウィンドウサイズに適用
            SetWindowSize(window, canvasSize, content.GetContentRect().Size);

            // (自動でコンテンツサイズが更新されるはず..)

            // コンテンツにストレッチ適用
            box.Stretch(true, TransformTrigger.WindowSnap);
        }

        /// <summary>
        /// ウィンドウサイズをリファレンスサイズに合わせる。
        /// 自動ウィンドウサイズ補正用
        /// </summary>
        /// <param name="window">ウィンドウ</param>
        /// <param name="canvasSize">現在のキャンバスサイズ</param>
        /// <param name="content">コンテンツ</param>
        public static void StretchReferenceWindow(Window window, Size canvasSize, PageFrameContent content)
        {
            if (window.WindowState != WindowState.Normal) return;

            var box = PageFrameBoxPresenter.Current.View;
            if (box is null) return;

            var bounds = box.CalcStretchContentBounds(content, box.Context.ReferenceSize);
            if (bounds.Size.IsEmpty) return;

            // スケールが変化して座標が変わるのでフレームのスナップは無効にする
            box.Context.IsSnapAnchor.Reset();

            // スケールストレッチ強制有効
            using var forceTrack = box.Context.ForceScaleStretchTracking.Lock();

            // ウィンドウサイズに適用
            SetWindowSize(window, canvasSize, bounds.Size);

            // (自動でコンテンツサイズが更新されるはず..)

            // コンテンツにストレッチ適用
            box.Stretch(true, TransformTrigger.WindowSnap);
        }

        /// <summary>
        /// ウィンドウサイズをキャンバスサイズで設定。
        /// 同時にスケールストレッチを行う
        /// </summary>
        /// <param name="window">ウィンドウ</param>
        /// <param name="canvasSize">現在のキャンバスサイズ</param>
        /// <param name="newCanvasSize">新しいキャンバスサイズ</param>
        /// <exception cref="ArgumentException"></exception>
        private static void SetWindowSize(Window window, Size canvasSize, Size newCanvasSize)
        {
            var frameWidth = window.ActualWidth - canvasSize.Width;
            var frameHeight = window.ActualHeight - canvasSize.Height;
            if (frameWidth < 0.0 || frameHeight < 0.0)
            {
                throw new ArgumentException($"canvasSize must be smaller than Window.Size.", nameof(canvasSize));
            }
            window.Width = newCanvasSize.Width + frameWidth;
            window.Height = newCanvasSize.Height + frameHeight;
        }

    }
}
