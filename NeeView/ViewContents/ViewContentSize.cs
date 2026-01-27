using NeeView.PageFrames;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace NeeView
{
    public interface IViewContentSize
    {
        bool IsRightAngle { get; }
        Size LayoutSize { get; }
        Size PixelSize { get; }
        Size RenderingSize { get; }
        Size SourceSize { get; }

        Size GetPictureSize();
    }

    public class ViewContentSize : IViewContentSize
    {
        private PageFrameElement _element;
        private PageFrameElementScale _scale;
        private Size _pixelSize;
        private bool _isRightAngle;


        public ViewContentSize(PageFrameElement element, PageFrameElementScale scale)
        {
            SetSource(element, scale);
        }


        public event EventHandler<ViewContentSizeChangedEventArgs>? SizeChanged;


        /// <summary>
        /// 元データのサイズ
        /// </summary>
        public Size SourceSize => _element.PageDataSource.Size;

        /// <summary>
        /// レイアウトサイズ (LayoutTransform適用)
        /// </summary>
        public Size LayoutSize { get; private set; }

        /// <summary>
        /// レンダーサイズ (RenderTransform適用)
        /// </summary>
        public Size RenderingSize { get; private set; }

        /// <summary>
        /// DPIを適用した実Pixelサイズ
        /// </summary>
        public Size PixelSize
        {
            get { return _pixelSize; }
            set
            {
                if (_pixelSize != value)
                {
                    _pixelSize = value;
                    SizeChanged?.Invoke(this, ViewContentSizeChangedEventArgs.SizeEventArgs);
                }
            }
        }

        /// <summary>
        /// 直角か？
        /// </summary>
        public bool IsRightAngle
        {
            get { return _isRightAngle; }
            private set
            {
                if (_isRightAngle != value)
                {
                    _isRightAngle = value;
                    SizeChanged?.Invoke(this, ViewContentSizeChangedEventArgs.IsRightAngleEventArgs);
                }
            }
        }


        [MemberNotNull(nameof(_element), nameof(_scale))]
        public void SetSource(PageFrameElement element, PageFrameElementScale scale)
        {
            _element = element;
            _scale = scale;
            Update();
        }

        private void Update()
        {
            var layoutWidth = _element.Width * _scale.LayoutScale;
            var layoutHeight = _element.Height * _scale.LayoutScale;
            IsRightAngle = Math.Abs(_scale.RenderAngle % 90) < 1.0;
            LayoutSize = new Size(layoutWidth, layoutHeight);
            RenderingSize = new Size(Math.Abs(layoutWidth * _scale.RenderScale), Math.Abs(layoutHeight * _scale.RenderScale));
            PixelSize = new Size(RenderingSize.Width * _scale.BaseScale * _scale.DpiScale.DpiScaleX, RenderingSize.Height * _scale.BaseScale * _scale.DpiScale.DpiScaleY);
        }

        /// <summary>
        /// 表示サイズからリソース画像サイズに変換
        /// </summary>
        /// <param name="size">PageFrameElement.Size</param>
        /// <returns></returns>
        public virtual Size GetPictureSize()
        {
            return _element.ViewSizeCalculator.GetSourceSize(PixelSize);
        }
    }

    public enum ViewContentSizeChangedAction
    {
        None,
        Size,
        IsRightAngle,
    }

    public class ViewContentSizeChangedEventArgs : EventArgs
    {
        public static ViewContentSizeChangedEventArgs SizeEventArgs { get; } = new ViewContentSizeChangedEventArgs(ViewContentSizeChangedAction.Size);
        public static ViewContentSizeChangedEventArgs IsRightAngleEventArgs { get; } = new ViewContentSizeChangedEventArgs(ViewContentSizeChangedAction.IsRightAngle);

        public ViewContentSizeChangedEventArgs(ViewContentSizeChangedAction action)
        {
            Action = action;
        }

        public ViewContentSizeChangedAction Action { get; }
    }
}
