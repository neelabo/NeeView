using NeeLaboratory.ComponentModel;
using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class CanvasBackgroundSource : BindableBase, IDisposable
    {
        private readonly IContentCanvasBrushSource _contentCanvas;
        private SolidColorBrush _foregroundBrush = Brushes.White;
        private Brush? _backgroundBrush = Brushes.Black;
        private Brush? _backgroundFrontBrush;
        private Brush? _customBackgroundBrush;
        private Brush? _customBackgroundFrontBrush;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;


        public CanvasBackgroundSource(IContentCanvasBrushSource contentCanvas)
        {
            _contentCanvas = contentCanvas;

            _contentCanvas.ContentChanged += ContentCanvas_ContentChanged;
            _disposables.Add(() => _contentCanvas.ContentChanged -= ContentCanvas_ContentChanged);

            _contentCanvas.DpiChanged += ContentCanvas_DpiChanged;
            _disposables.Add(() => _contentCanvas.DpiChanged -= ContentCanvas_DpiChanged);

            _disposables.Add(Config.Current.Background.SubscribePropertyChanged(nameof(BackgroundConfig.CustomBackground), (s, e) =>
            {
                Debug.Assert(false, "Config.Current.Background.CustomBackground が変更されない前提");
            }));

            // NOTE: Config.Current.Background.CustomBackground が変更されない前提の購読
            _disposables.Add(Config.Current.Background.CustomBackground.SubscribePropertyChanged((s, e) =>
            {
                UpdateCustomBackgroundBrush();
            }));

            _disposables.Add(Config.Current.Background.SubscribePropertyChanged(nameof(BackgroundConfig.BackgroundType), (s, e) =>
            {
                UpdateBackgroundBrush();
            }));

            // Initialize
            UpdateCustomBackgroundBrush();
            UpdateBackgroundBrush();
        }


        // Foreground Brush：ファイルページのフォントカラー用
        public SolidColorBrush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { SetProperty(ref _foregroundBrush, value); }
        }

        // Background Brush
        public Brush? BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { if (SetProperty(ref _backgroundBrush, value)) { UpdateForegroundBrush(); } }
        }

        // BackgroundFrontBrush
        public Brush? BackgroundFrontBrush
        {
            get { return _backgroundFrontBrush; }
            set { SetProperty(ref _backgroundFrontBrush, value); }
        }

        /// <summary>
        /// カスタム背景
        /// </summary>
        public Brush? CustomBackgroundBrush
        {
            get { return _customBackgroundBrush ?? (_customBackgroundBrush = Config.Current.Background.CustomBackground?.CreateBackBrush()); }
        }

        /// <summary>
        /// カスタム背景
        /// </summary>
        public Brush? CustomBackgroundFrontBrush
        {
            get { return _customBackgroundFrontBrush ?? (_customBackgroundFrontBrush = Config.Current.Background.CustomBackground?.CreateFrontBrush()); }
        }

        /// <summary>
        /// チェック模様
        /// </summary>
        public Brush CheckBackgroundBrush { get; } = (DrawingBrush)Application.Current.Resources["CheckerBrush"];
        public Brush CheckBackgroundBrushDark { get; } = (DrawingBrush)Application.Current.Resources["CheckerBrushDark"];


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void ContentCanvas_ContentChanged(object? sender, EventArgs e)
        {
            AppDispatcher.BeginInvoke(() => UpdateBackgroundBrush());
        }

        private void ContentCanvas_DpiChanged(object? sender, EventArgs e)
        {
            UpdateBackgroundBrush();
        }

        // from http://msdn.microsoft.com/en-us/library/aa970904.aspx
        public static Brush CreateCheckerBrush(Color color)
        {
            var brush1 = new SolidColorBrush(color);

            var hsv = color.ToHSV();
            hsv.V = hsv.V + (hsv.V < 0.1 ? 0.1 : -0.1);
            var brush2 = new SolidColorBrush(hsv.ToARGB());

            var checkerBrush = new DrawingBrush();

            var backgroundSquare =
                new GeometryDrawing(
                    brush1,
                    null,
                    new RectangleGeometry(new Rect(0, 0, 8, 8)));

            var aGeometryGroup = new GeometryGroup();
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 4, 4)));
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(4, 4, 4, 4)));

            var checkers = new GeometryDrawing(brush2, null, aGeometryGroup);

            var checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            checkerBrush.Drawing = checkersDrawingGroup;
            checkerBrush.ViewportUnits = BrushMappingMode.Absolute;
            checkerBrush.Viewport = new Rect(0, 0, 16, 16);
            checkerBrush.TileMode = TileMode.Tile;

            return checkerBrush;
        }


        // Foreground Brush 更新
        private void UpdateForegroundBrush()
        {
            if (BackgroundBrush is SolidColorBrush solidColorBrush)
            {
                double y =
                    (double)solidColorBrush.Color.R * 0.299 +
                    (double)solidColorBrush.Color.G * 0.587 +
                    (double)solidColorBrush.Color.B * 0.114;

                ForegroundBrush = (y < 128.0) ? Brushes.White : Brushes.Black;
            }
            else
            {
                ForegroundBrush = Brushes.Black;
            }
        }

        // Background Brush 更新
        public void UpdateBackgroundBrush()
        {
            BackgroundBrush = CreateBackgroundBrush();
            BackgroundFrontBrush = CreateBackgroundFrontBrush(_contentCanvas.Dpi);
        }

        private void UpdateCustomBackgroundBrush()
        {
            _customBackgroundBrush = null;
            _customBackgroundFrontBrush = null;
            if (Config.Current.Background.BackgroundType == BackgroundType.Custom)
            {
                UpdateBackgroundBrush();
            }
        }

        /// <summary>
        /// 背景ブラシ作成
        /// </summary>
        /// <returns></returns>
        public Brush? CreateBackgroundBrush()
        {
            return Config.Current.Background.BackgroundType switch
            {
                BackgroundType.White => Brushes.White,
                BackgroundType.Auto => new SolidColorBrush(_contentCanvas.GetContentColor()),
                BackgroundType.Check => null,
                BackgroundType.Custom => CustomBackgroundBrush,
                _ => Brushes.Black,
            };
        }

        /// <summary>
        /// 背景ブラシ(画像)作成
        /// </summary>
        /// <param name="dpi">適用するDPI</param>
        /// <returns></returns>
        public Brush? CreateBackgroundFrontBrush(DpiScale dpi)
        {
            switch (Config.Current.Background.BackgroundType)
            {
                default:
                case BackgroundType.Black:
                case BackgroundType.White:
                case BackgroundType.Auto:
                    return null;
                case BackgroundType.Check:
                    {
                        var brush = CheckBackgroundBrush.Clone();
                        brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        return brush;
                    }
                case BackgroundType.CheckDark:
                    {
                        var brush = CheckBackgroundBrushDark.Clone();
                        brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        return brush;
                    }
                case BackgroundType.Custom:
                    {
                        var brush = CustomBackgroundFrontBrush?.Clone();
                        // 画像タイルの場合はDPI考慮
                        if (brush is ImageBrush imageBrush && imageBrush.TileMode == TileMode.Tile)
                        {
                            brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        }
                        return brush;
                    }
            }
        }


    }
}
