using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace NeeView.PageFrames
{
    public class PageFrameBackground : Grid, IDisposable
    {
        private readonly DpiScaleProvider _dpiScaleProvider;
        private readonly ContentCanvasBrushSource _brushSource;
        private readonly CanvasBackgroundSource _canvasBrush;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;
        private readonly Grid _bg1;
        private readonly Grid _bg2;

        public PageFrameBackground(DpiScaleProvider dpiScaleProvider) : this(dpiScaleProvider, null)
        {
        }

        public PageFrameBackground(DpiScaleProvider dpiScaleProvider, Page? page)
        {
            _dpiScaleProvider = dpiScaleProvider;
            _brushSource = new ContentCanvasBrushSource(_dpiScaleProvider, page);
            _canvasBrush = new CanvasBackgroundSource(_brushSource);
            _disposables.Add(_canvasBrush);

            _bg1 = this;
            _bg2 = new Grid();
            _bg1.Children.Add(_bg2);

            _disposables.Add(_canvasBrush.SubscribePropertyChanged(nameof(_canvasBrush.BackgroundBrush), (s, e) => UpdateBackground1()));
            _disposables.Add(_canvasBrush.SubscribePropertyChanged(nameof(_canvasBrush.BackgroundFrontBrush), (s, e) => UpdateBackground2()));

            UpdateBackground1();
            UpdateBackground2();

            //_bg1.SetBinding(Grid.BackgroundProperty, new Binding(nameof(_canvasBrush.BackgroundBrush)) { Source = _canvasBrush });
            //_bg2.SetBinding(Grid.BackgroundProperty, new Binding(nameof(_canvasBrush.BackgroundFrontBrush)) { Source = _canvasBrush });
        }



        public Page? Page
        {
            get { return (Page?)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static readonly DependencyProperty PageProperty =
            DependencyProperty.Register("Page", typeof(Page), typeof(PageFrameBackground), new PropertyMetadata(null, Page_Changed));

        private static void Page_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageFrameBackground control)
            {
                control.Update();
            }
        }

        public Brush Bg1Brush => _bg1.Background;
        public Brush Bg2Brush => _bg2.Background;


        private void Update()
        {
            _brushSource.SetPage(Page);
        }

        private void UpdateBackground1()
        {
            _bg1.Background = _canvasBrush.BackgroundBrush;
        }

        private void UpdateBackground2()
        {
            _bg2.Background = _canvasBrush.BackgroundFrontBrush;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    //BindingOperations.ClearBinding(_bg1, Grid.BackgroundProperty);
                    //BindingOperations.ClearBinding(_bg2, Grid.BackgroundProperty);
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
