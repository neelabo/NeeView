using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class BitmapViewContentSize : ViewContentSize
    {
        public BitmapViewContentSize(PageFrameElement element, PageFrameElementScale scale) : base(element, scale)
        {
        }

        /// <summary>
        /// 表示サイズからリソース画像サイズに変換
        /// </summary>
        /// <param name="size">PageFrameElement.Size</param>
        /// <returns></returns>
        public override Size GetPictureSize()
        {
            var size = base.GetPictureSize();

            var sourceSize = SourceSize; // ((BitmapPageContent)Element.Page.Content).PictureInfo.Size;

            // not ResizeFilter.IsEnabled => Size.Empty
            if (!Config.Current.ImageResizeFilter.IsEnabled)
            {
                size = sourceSize;
            }

            // Performance.MaximumSize Limit
            var maxWidth = Math.Max(sourceSize.Width, Config.Current.Performance.MaximumSize.Width);
            var maxHeight = Math.Max(sourceSize.Height, Config.Current.Performance.MaximumSize.Height);
            var maxSize = new Size(maxWidth, maxHeight);
            size = size.Limit(maxSize);

            // IsDotKeep & scale >= 1 => Size.Empty
            if (Config.Current.ImageDotKeep.IsImageDotKeep(size, sourceSize))
            {
                size = sourceSize;
            }

            Debug.Assert(!size.IsEmpty);

            return size;
        }
    }

}
