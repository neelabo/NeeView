﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using NeeView.Windows;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class PageFrameBoxPresenter : INotifyPropertyChanged, IDragTransformContextFactory, IBookPageContext
    {
        private Config _config;
        private BookHub _bookHub;

        private Book? _book;
        private BookContext? _bookContext;
        private BookShareContext _shareContext;
        private PageFrameBox? _box;
        private BookCommandControl? _pageControl;
        private BookMementoControl? _bookMementoControl;
        private bool _isLoading;


        public PageFrameBoxPresenter(Config config, BookHub bookHub)
        {
            _config = config;
            _shareContext = new BookShareContext(_config);
            _bookHub = bookHub;

            _bookHub.BookChanging += BookHub_BookChanging;
            _bookHub.BookChanged += BookHub_BookChanged;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler? PageFrameBoxChanged;

        [Subscribable]
        public event EventHandler? SelectedRangeChanged;

        [Subscribable]
        public event EventHandler<ViewContentChangedEventArgs>? ViewContentChanged;

        [Subscribable]
        public event EventHandler? SelectedContainerLayoutChanged;

        [Subscribable]
        public event EventHandler? SelectedContentSizeChanged;

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;


        public bool IsEnabled => _box != null;


        public bool IsLoading => _bookHub.IsLoading || _isLoading;

        public IReadOnlyList<Page> Pages => _bookContext?.Pages ?? new List<Page>();

        public PageRange SelectedRange
        {
            get => _bookContext?.SelectedRange ?? new PageRange();
        }

        public IReadOnlyList<Page> SelectedPages
        {
            get
            {
                var bookContext = _bookContext;
                if (bookContext is null) return new List<Page>();
                return bookContext.SelectedRange.CollectPositions().Select(e => bookContext.Pages[e.Index]).Distinct().ToList();
            }
        }

        public IPageFrameBox? ValidPageFrameBox => ValidBox();


        public PageFrameBox? View => _box;

        public BookCommandControl? PageControl => _pageControl;

        public bool IsMedia => _bookContext?.IsMedia ?? false;



        private void BookHub_BookChanging(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                _isLoading = true;
                PageFrameBoxChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void BookHub_BookChanged(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                Open(_bookHub.GetCurrentBook());
                _isLoading = false;
                PageFrameBoxChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void Open(Book? book)
        {
            Close();
            if (book is null) return;

            _book = book;

            _bookContext = new BookContext(_book, _config, _shareContext);
            _bookContext.PagesChanged += BookContext_PagesChanged;
            _bookContext.SelectedRangeChanged += BookContext_SelectedRangeChanged;
            _bookContext.PropertyChanged += BookContext_PropertyChanged;

            _box = new PageFrameBox(_bookContext);
            _box.ViewContentChanged += Box_ViewContentChanged;
            _box.TransformChanged += Box_TransformChanged;
            _box.SelectedContainerLayoutChanged += Box_SelectedContainerLayoutChanged;
            _box.SelectedContentSizeChanged += Box_SelectedContentSizeChanged;

            _pageControl = new BookCommandControl(_bookContext, _box);

            _bookMementoControl = new BookMementoControl(_book, BookHistoryCollection.Current);

            RaisePropertyChanged(nameof(View));
            RaisePropertyChanged(null);
            PagesChanged?.Invoke(this, EventArgs.Empty);
            SelectedRangeChanged?.Invoke(this, EventArgs.Empty);
        }



        private void Close()
        {
            if (_box is null) return;

            _bookMementoControl?.Dispose();
            _bookMementoControl = null;

            _pageControl?.Dispose();
            _pageControl = null;

            Debug.Assert(_box is PageFrameBox);
            if (_box is not null)
            {
                (_box as IDisposable)?.Dispose();
                _box.ViewContentChanged -= Box_ViewContentChanged;
                _box.TransformChanged -= Box_TransformChanged;
                _box.SelectedContainerLayoutChanged -= Box_SelectedContainerLayoutChanged;
                _box.SelectedContentSizeChanged -= Box_SelectedContentSizeChanged;
                _box = null;
            }
            RaisePropertyChanged(nameof(View));

            Debug.Assert(_bookContext is not null);
            _bookContext.PagesChanged -= BookContext_PagesChanged;
            _bookContext.SelectedRangeChanged -= BookContext_SelectedRangeChanged;
            _bookContext.PropertyChanged -= BookContext_PropertyChanged;
            _bookContext.Dispose();
            _bookContext = null;

            Debug.Assert(_book is not null);
            _book = null; // Dispose は BookHub の仕事

            GC.Collect();
            GC.WaitForFullGCComplete();
        }


        private void Box_ViewContentChanged(object? sender, ViewContentChangedEventArgs e)
        {
            ViewContentChanged?.Invoke(this, e);
        }

        private void Box_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            switch (e.Category)
            {
                case TransformCategory.Loupe:
                    ShowLoupeTransformMessage(e.Source, e.Action);
                    break;
                case TransformCategory.Content:
                    var originalScale = e is OriginalScaleTransformChangedEventArgs arg ? arg.OriginalScale : 1.0;
                    ShowContentTransformMessage(e.Source, e.Action, originalScale);
                    break;
            }

            TransformChanged?.Invoke(this, e);
        }

        // TODO: Selected の情報をまとめたクラスみたいなものがほしいかも？
        private void Box_SelectedContainerLayoutChanged(object? sender, EventArgs e)
        {
            SelectedContainerLayoutChanged?.Invoke(this, e);
        }

        private void Box_SelectedContentSizeChanged(object? sender, EventArgs e)
        {
            SelectedContentSizeChanged?.Invoke(this, e);
        }


        private void BookContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookContext.SelectedRange):
                    RaisePropertyChanged(nameof(SelectedRange));
                    break;
                case nameof(BookContext.Pages):
                    RaisePropertyChanged(nameof(Pages));
                    break;
            }
        }

        private void BookContext_PagesChanged(object? sender, EventArgs e)
        {
            PagesChanged?.Invoke(this, e);
        }

        private void BookContext_SelectedRangeChanged(object? sender, EventArgs e)
        {
            SelectedRangeChanged?.Invoke(this, e);
        }

        public void ReOpen()
        {
            if (_bookContext is null) return;

            _bookHub.RequestReLoad(this);
            //var memento = _bookContext.CreateMemento();
            //_bookHub.RequestLoad(memento);
        }


        private void ShowLoupeTransformMessage(ITransformControlObject source, TransformAction action)
        {
            var infoMessage = InfoMessage.Current; // TODO: not singleton
            if (Config.Current.Notice.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            switch (action)
            {
                case TransformAction.Scale:
                    var scale = ((IScaleControl)source).Scale;
                    if (scale != 1.0)
                    {
                        infoMessage.SetMessage(InfoMessageType.ViewTransform, $"×{scale:0.0}");
                    }
                    break;
            }
        }

        private void ShowContentTransformMessage(ITransformControlObject source, TransformAction action, double originalScale)
        {
            var infoMessage = InfoMessage.Current; // TODO: not singleton
            if (Config.Current.Notice.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            switch (action)
            {
                case TransformAction.Scale:
                    var scale = ((IScaleControl)source).Scale;
                    if (Config.Current.Notice.IsOriginalScaleShowMessage)
                    {
                        var dpi = (Window.GetWindow(this.View) is IDpiScaleProvider dpiProvider) ? dpiProvider.GetDpiScale().ToFixedScale().DpiScaleX : 1.0;
                        scale = scale * originalScale * dpi;
                    }
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(scale * 100.0 + 0.1)}%");
                    break;
                case TransformAction.Angle:
                    var angle = ((IAngleControl)source).Angle;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(angle)}°");
                    break;
                case TransformAction.FlipHorizontal:
                    var isFlipHorizontal = ((IFlipControl)source).IsFlipHorizontal;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, Properties.Resources.Notice_FlipHorizontal + " " + (isFlipHorizontal ? "ON" : "OFF"));
                    break;
                case TransformAction.FlipVertical:
                    var isFlipVertical = ((IFlipControl)source).IsFlipVertical;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, Properties.Resources.Notice_FlipVertical + " " + (isFlipVertical ? "ON" : "OFF"));
                    break;
            }
        }

        #region IPageFrameBox

        private IPageFrameBox? ValidBox()
        {
            return IsLoading ? null : _box;
        }

        public DragTransformContext? CreateDragTransformContext(bool isPointContainer, bool isLoupeTransform)
        {
            return _box?.CreateDragTransformContext(isPointContainer, isLoupeTransform);
        }


        public void MoveTo(PagePosition position, LinkedListDirection direction)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.MoveTo(position, direction));
        }

        public void MoveToNextPage(LinkedListDirection direction)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.MoveToNextPage(direction));
        }

        public void MoveToNextFrame(LinkedListDirection direction)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.MoveToNextFrame(direction));
        }

        // 前のフォルダーに戻る
        public void MoveToNextFolder(LinkedListDirection direction, bool isShowMessage)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() =>
            {
                box.MoveToNextFolder(direction, isShowMessage);
            });


        }



        public void ScrollToNextFrame(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.ScrollToNextFrame(direction, parameter, lineBreakStopMode, endMargin));
        }

        public bool ScrollToNext(LinkedListDirection direction, IScrollNTypeParameter parameter)
        {
            return ValidBox()?.ScrollToNext(direction, parameter) ?? false;
        }

        public void ResetTransform()
        {
            ValidBox()?.ResetTransform();
        }

        public void Stretch(bool ignoreViewOrigin)
        {
            ValidBox()?.Stretch(ignoreViewOrigin);
        }

        public void Reset()
        {
            if (_book is null) return;
            ReOpen();
        }

        public PageFrameTransformAccessor? CreateSelectedTransform()
        {
            return _box?.CreateSelectedTransform();
        }


        /// <summary>
        /// 選択中の <see cref="PageFrameContent"/> を取得
        /// </summary>
        /// <returns></returns>
        public PageFrameContent? GetSelectedPageFrameContent()
        {
            return _box?.GetSelectedPageFrameContent();
        }

        /// <summary>
        /// 背景情報取得
        /// </summary>
        /// <returns></returns>
        public PageFrameBackground? GetBackground()
        {
            return _box?.GetBackground();
        }

        #endregion IPageFrameBox
    }
}
