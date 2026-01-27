using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    public class ViewImageExporter : IImageExporter, IDisposable
    {
        private readonly ExportImageSource _source;
        private bool _disposedValue;


        public ViewImageExporter(ExportImageSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public ImageExporterContent? CreateView(ImageExporterCreateOptions options)
        {
            if (_source == null) return null;

            var grid = new Grid();

            if (options.HasBackground)
            {
                grid.Background = _source.Background;

                var backgroundFront = new Rectangle();
                backgroundFront.HorizontalAlignment = HorizontalAlignment.Stretch;
                backgroundFront.VerticalAlignment = VerticalAlignment.Stretch;
                backgroundFront.Fill = _source.BackgroundFront;
                RenderOptions.SetBitmapScalingMode(backgroundFront, BitmapScalingMode.HighQuality);
                grid.Children.Add(backgroundFront);
            }

            var viewRect = _source.PageFrameContent.GetRawContentRect();

            var rectangle = new Rectangle();
            rectangle.Width = viewRect.Width;
            rectangle.Height = viewRect.Height;
            var brush = new VisualBrush(_source.View);
            brush.Stretch = Stretch.None;
            rectangle.Fill = brush;
            rectangle.LayoutTransform = _source.ViewTransform;
            rectangle.Effect = _source.ViewEffect;
            rectangle.UseLayoutRounding = true;
            rectangle.SnapsToDevicePixels = true;
            grid.Children.Add(rectangle);

            // 描画サイズ取得
            var rect = new Rect(0, 0, rectangle.Width, rectangle.Height);
            rect = _source.ViewTransform.TransformBounds(rect);

            // オリジナルサイズ補正
            if (options.IsOriginalSize)
            {
                var rawSize = _source.PageFrameContent.PageFrame.GetRawContentSize();
                rectangle.Width = rawSize.Width;
                rectangle.Height = rawSize.Height;
                brush.Stretch = Stretch.Uniform;
                rectangle.LayoutTransform = null;
                rect = new Rect(0, 0, rectangle.Width, rectangle.Height);
            }

            // スケールモード設定
            SetScalingMode(options.IsDotKeep ? BitmapScalingMode.NearestNeighbor : (options.IsOriginalSize ? BitmapScalingMode.HighQuality : null));

            return new ImageExporterContent(grid, rect.Size);
        }


        private BitmapImageFormat GetBitmapImageFormatFromFileName(string path)
        {
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".png" => BitmapImageFormat.Png,
                _ => BitmapImageFormat.Jpeg,
            };
        }

        public async ValueTask ExportAsync(string path, bool isOverwrite, int qualityLevel, ImageExporterCreateOptions options, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // create bitmap
            var bitmapSource = AppDispatcher.Invoke(() => CreateBitmapSource(options));

            // ensure directory
            var outputDir = System.IO.Path.GetDirectoryName(path) ?? throw new IOException($"Illegal path: {path}");
            Directory.CreateDirectory(outputDir);

            // export to file
            var fileMode = isOverwrite ? FileMode.Create : FileMode.CreateNew;
            using (var stream = new FileStream(path, fileMode))
            {
                var format = GetBitmapImageFormatFromFileName(path);
                await AppDispatcher.InvokeAsync(() => Export(stream, format, qualityLevel, bitmapSource));
            }
        }

        public void Export(Stream stream, string path, int qualityLevel, ImageExporterCreateOptions options)
        {
            Export(stream, path, qualityLevel, CreateBitmapSource(options));
        }

        public void Export(Stream stream, string path, int qualityLevel, BitmapSource bitmapSource)
        {
            Export(stream, GetBitmapImageFormatFromFileName(path), qualityLevel, bitmapSource);
        }

        public void Export(Stream stream, BitmapImageFormat format, int qualityLevel, BitmapSource bitmapSource)
        {
            if (format == BitmapImageFormat.Png)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
            }
            else
            {
                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = qualityLevel;
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
            }
        }

        public ImageSource CreateImageSource(ImageExporterCreateOptions options)
        {
            return CreateBitmapSource(options);
        }

        private BitmapSource CreateBitmapSource(ImageExporterCreateOptions options)
        {
            if (_source.View == null) throw new InvalidOperationException();

            var canvas = new Canvas();

            var content = CreateView(options);
            if (content is null) throw new InvalidOperationException();

            canvas.Children.Add(content.View);

            // calc content size
            UpdateElementLayout(canvas, new Size(256, 256));
            var rect = new Rect(0, 0, content.View.ActualWidth, content.View.ActualHeight);
            canvas.Width = rect.Width;
            canvas.Height = rect.Height;

            UpdateElementLayout(canvas, rect.Size);

            double dpi = 96.0;
            var bmp = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(canvas);

            canvas.Children.Clear(); // コンテンツ開放

            return bmp;
        }

        private static void UpdateElementLayout(FrameworkElement element, Size size)
        {
            element.Measure(size);
            element.Arrange(new Rect(size));
            element.UpdateLayout();
        }

        public string CreateFileName()
        {
            var bookName = LoosePath.ValidFileName(LoosePath.GetFileNameWithoutExtension(_source.BookAddress));
            var indexLabel = (_source.Pages.Count > 1) ? $"{_source.Pages[0].Index:000}-{_source.Pages[1].Index:000}" : $"{_source.Pages[0].Index:000}";
            return $"{bookName}_{indexLabel}.png";
        }

        private void ResetScalingMode()
        {
            SetScalingMode(null);
        }

        private void SetScalingMode(BitmapScalingMode? scalingMode)
        {
            foreach (var viewContent in _source.PageFrameContent.ViewContents.OfType<IHasScalingMode>())
            {
                viewContent.ScalingMode = scalingMode;
            }
        }

        public bool CanExport()
        {
            return true;
        }

        public void ThrowIfCannotExport()
        {
        }

        public DateTime GetLastWriteTime()
        {
            return DateTime.MinValue;
        }

        public long GetLength(string path, int qualityLevel, ImageExporterCreateOptions options)
        {
            using var stream = new MemoryStream();
            Export(stream, path, qualityLevel, options);
            return stream.Length;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ResetScalingMode();
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
