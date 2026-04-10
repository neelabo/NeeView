//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    [LocalDebug]
    public static partial class ViewContentTools
    {
        public static FrameworkElement CreateLoadingContent(PageFrameElement source, bool isBlackBackground, bool showProgress)
        {
            var grid = new Grid();
            grid.Background = isBlackBackground ? new SolidColorBrush(Color.FromRgb(0x10, 0x10, 0x10)) : new SolidColorBrush(Config.Current.Book.LoadingPageColor);

            if (showProgress)
            {
                var stackPanel = new StackPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                grid.Children.Add(stackPanel);

                var foregroundBrush = Brushes.Gray;

                var textBlock = new TextBlock()
                {
                    Text = source.Page.EntryLastName,
                    Foreground = foregroundBrush,
                    FontSize = 20,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                stackPanel.Children.Add(textBlock);

                var loading = new ProgressRing()
                {
                    Foreground = foregroundBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                stackPanel.Children.Add(loading);
            }

            return grid;
        }


        public static FrameworkElement CreateErrorContent(PageFrameElement source, string? message)
        {
            var entry = source.Page.ArchiveEntry;
            var bitmapSource = AppDispatcher.Invoke(() =>
            {
                var bitmapSourceCollection = entry.IsDirectory
                    ? FileIconCollection.Current.CreateDefaultFolderIcon()
                    : FileIconCollection.Current.CreateFileIcon(entry.SystemPath, IO.FileIconType.FileType, true, true);
                bitmapSourceCollection.Freeze();
                return bitmapSourceCollection.GetBitmapSource(48.0);
            });

            var viewData = new FileViewData(entry, FilePageIcon.Alert, message ?? "Error", bitmapSource);
            return new MessagePageControl(viewData);
        }


        public static void SetBitmapScalingMode(UIElement element, Size imageSize, ViewContentSize contentSize, BitmapScalingMode? scalingMode)
        {
            var pixelSize = GetRenderPixelSize(element, imageSize);

            // ScalingMode が指定されている
            if (scalingMode is not null)
            {
                LocalDebug.WriteLine($"XX: Force {scalingMode.Value}: {pixelSize:f0} / {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, scalingMode.Value);
                element.SnapsToDevicePixels = scalingMode.Value == BitmapScalingMode.NearestNeighbor;
            }
            // 画像サイズがビッタリの場合はドットバイドットになるような設定
            else if (contentSize.IsRightAngle && SizeEquals(contentSize.PixelSize, pixelSize, 1.1))
            {
                LocalDebug.WriteLine($"OO: NearestNeighbor: {pixelSize:f0} /{imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.NearestNeighbor);
                element.SnapsToDevicePixels = true;
            }
            // DotKeep mode
            // TODO: Config.Current参照はよろしくない
            else if (Config.Current.ImageDotKeep.IsImageDotKeep(contentSize.PixelSize, pixelSize))
            {
                LocalDebug.WriteLine($"XX: NearestNeighbor: {pixelSize:f0} / {imageSize:f0} != request {contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.NearestNeighbor);
                element.SnapsToDevicePixels = true;
            }
            else
            {
                LocalDebug.WriteLine($"XX: Fantastic: {pixelSize:f0} / {imageSize:f0} != request {contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.Fant);
                element.SnapsToDevicePixels = false;
            }
        }


        private static Size GetRenderPixelSize(UIElement element, Size imageSize)
        {
#if DEBUG
            var sourceSize = BrushImageContentTools.GetImageSourcePixelSize(element);
            Debug.Assert(sourceSize.IsEmpty || SizeEquals(sourceSize, imageSize, 1.0), $"GetRenderPixelSize: {sourceSize} != {imageSize}");
#endif
            var viewbox = BrushImageContentTools.GetViewBox(element);
            if (viewbox.IsEmpty)
            {
                return imageSize;
            }
            else
            {
                return new Size(viewbox.Width * imageSize.Width, viewbox.Height * imageSize.Height);
            }
        }

        private static bool SizeEquals(Size a, Size b, double margin)
        {
            return DoubleEquals(a.Width, b.Width, margin) && DoubleEquals(a.Height, b.Height, margin);
        }

        private static bool DoubleEquals(double a, double b, double margin)
        {
            return Math.Abs(a - b) < margin;
        }
    }

}
