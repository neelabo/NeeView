//#define LOCAL_DEBUG

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using NeeLaboratory.Generators;
using NeeView.PageFrames;

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
            // ScalingMode が指定されている
            if (scalingMode is not null)
            {
                LocalDebug.WriteLine($"XX: Force {scalingMode.Value}: {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, scalingMode.Value);
                element.SnapsToDevicePixels = scalingMode.Value == BitmapScalingMode.NearestNeighbor;
            }
            // 画像サイズがビッタリの場合はドットバイドットになるような設定
            else if (contentSize.IsRightAngle && Math.Abs(contentSize.PixelSize.Width - imageSize.Width) < 1.1 && Math.Abs(contentSize.PixelSize.Height - imageSize.Height) < 1.1)
            {
                LocalDebug.WriteLine($"OO: NearestNeighbor: {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.NearestNeighbor);
                element.SnapsToDevicePixels = true;
            }
            // DotKeep mode
            // TODO: Config.Current参照はよろしくない
            else if (Config.Current.ImageDotKeep.IsImageDotKeep(contentSize.PixelSize, imageSize))
            {
                LocalDebug.WriteLine($"XX: NearestNeighbor: {imageSize:f0} != request {contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.NearestNeighbor);
                element.SnapsToDevicePixels = true;
            }
            else
            {
                LocalDebug.WriteLine($"XX: Fantastic: {imageSize:f0} != request {contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.Fant);
                element.SnapsToDevicePixels = false;
            }
        }

    }
}
