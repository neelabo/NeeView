using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using NeeView.Maths;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame のサイズに影響するパラメータと計算
    /// </summary>
    // NOTE: NeeView exist
    // TODO: ２つ並べたコンテンツのサイズをあわせる計算。 PageSource に Scale を保持させる
    public class ContentSizeCalculator : IContentSizeCalculatorProfile
    {
        private readonly IContentSizeCalculatorProfile _profile;

        public ContentSizeCalculator(IContentSizeCalculatorProfile profile)
        {
            _profile = profile;
        }


        public Size CanvasSize => _profile.CanvasSize;
        public AutoRotateType AutoRotate => _profile.AutoRotate;
        public AutoRotatePolicy AutoRotatePolicy => _profile.AutoRotatePolicy;
        public bool AllowFileContentAutoRotate => _profile.AllowFileContentAutoRotate;
        public PageStretchMode StretchMode => _profile.StretchMode;
        public double ContentsSpace => _profile.ContentsSpace;
        public bool AllowEnlarge => _profile.AllowEnlarge;
        public bool AllowReduce => _profile.AllowReduce;
        public Size ReferenceSize => _profile.ReferenceSize;
        public DpiScale DpiScale => _profile.DpiScale;
        public WidePageStretch WidePageStretch => _profile.WidePageStretch;


        /// <summary>
        /// 自動回転を求める
        /// </summary>
        /// <param name="size"></param>
        /// <param name="isFileContent"></param>
        /// <returns></returns>
        public double CalcAutoRotate(Size size, bool isFileContent)
        {
            return CalcAutoRotate(size, isFileContent, ReferenceSize);
        }

        /// <summary>
        /// 自動回転を求める
        /// </summary>
        /// <param name="size"></param>
        /// <param name="isFileContent"></param>
        /// <param name="canvasSize"></param>
        /// <returns></returns>
        public double CalcAutoRotate(Size size, bool isFileContent, Size canvasSize)
        {
            if (isFileContent && !AllowFileContentAutoRotate)
            {
                return 0.0;
            }

            if (AutoRotate == AutoRotateType.None)
            {
                return 0.0;
            }

            var isContentLandscape = AspectRatioTools.IsLandscape(size);
            var isCanvasLandscape = canvasSize.Width >= canvasSize.Height;
            return (CheckAutoRotate(isContentLandscape, isCanvasLandscape) || AutoRotate.IsForced()) ? AutoRotate.ToAngle() : 0.0;
        }

        private bool CheckAutoRotate(bool isContentLandscape, bool isCanvasLandscape)
        {
            return AutoRotatePolicy switch
            {
                AutoRotatePolicy.FitToViewArea => isContentLandscape != isCanvasLandscape,
                AutoRotatePolicy.ToLandscape => !isContentLandscape,
                AutoRotatePolicy.ToPortrait => isContentLandscape,
                _ => false,
            };
        }

        /// <summary>
        /// ストレッチスケールを求める
        /// </summary>
        /// <param name="size"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public double CalcStretchScale(Size size, RotateTransform rotate)
        {
            return CalcStretchScale(size, rotate, CanvasSize);
        }

        /// <summary>
        /// ストレッチスケールを求める
        /// </summary>
        /// <param name="size"></param>
        /// <param name="angle"></param>
        /// <param name="canvasSize"></param>
        /// <returns></returns>
        public double CalcStretchScale(Size size, RotateTransform rotate, Size canvasSize)
        {
            var scales = GetFillScale(size, rotate, canvasSize);
            return Math.Min(scales.X, scales.Y);
        }

        /// <summary>
        /// ストレッチモードを適用したストレッチスケールを求める
        /// </summary>
        /// <param name="size"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public double CalcModeStretchScale(Size size, RotateTransform rotate)
        {
            return CalcModeStretchScale(size, rotate, CanvasSize);
        }

        /// <summary>
        /// ストレッチモードを適用したストレッチスケールを求める
        /// </summary>
        /// <param name="size"></param>
        /// <param name="rotate"></param>
        /// <param name="canvasSize"></param>
        /// <returns></returns>
        public double CalcModeStretchScale(Size size, RotateTransform rotate, Size canvasSize)
        {
            var scales = GetFillScale(size, rotate, canvasSize);
            return CalcStretchScale(StretchMode, scales, size, canvasSize);
        }

        /// <summary>
        /// ストレッチスケールを求める。PageFrame用
        /// </summary>
        /// <param name="size">コンテンツサイズ</param>
        /// <param name="span">コンテンツ間スペース</param>
        /// <param name="rotate">自動回転</param>
        /// <returns></returns>
        public double CalcFrameStretchScale(Size size, double span, RotateTransform rotate)
        {
            return CalcFrameStretchScale(size, span, rotate, CanvasSize);
        }

        /// <summary>
        /// ストレッチスケールを求める。PageFrame用
        /// </summary>
        /// <param name="size"></param>
        /// <param name="span"></param>
        /// <param name="rotate"></param>
        /// <param name="canvasSize"></param>
        /// <returns></returns>
        public double CalcFrameStretchScale(Size size, double span, RotateTransform rotate, Size canvasSize)
        {
            return CalcStretchScale(StretchMode, GetFillScaleWithSpan(size, span, rotate.Angle, canvasSize), size, canvasSize);
        }


        private double CalcStretchScale(PageStretchMode stretchMode, Vector scales, Size size, Size canvasSize)
        {
            return stretchMode switch
            {
                PageStretchMode.None => OneScale(),
                PageStretchMode.Uniform => Math.Min(scales.X, scales.Y),
                PageStretchMode.UniformToFill => Math.Max(scales.X, scales.Y),
                PageStretchMode.UniformToHorizontal => scales.X,
                PageStretchMode.UniformToVertical => scales.Y,
                PageStretchMode.UniformToSize => GetFillSizeScale(size, canvasSize),
                _ => throw new NotImplementedException(),
            };
        }

        private double GetFillSizeScale(Size size, Size canvasSize)
        {
            if (size.Width <= 0.0 || size.Height <= 0.0) return 1.0;
            var s0 = canvasSize.Width * canvasSize.Height;
            var s1 = size.Width * size.Height;
            var scale = AllowScale(Math.Sqrt(s0 / s1));
            return scale;
        }

        /// <summary>
        /// Calculate fill-scale
        /// </summary>
        /// <param name="size"></param>
        /// <param name="rotate"></param>
        /// <param name="canvasSize"></param>
        /// <returns></returns>
        private Vector GetFillScale(Size size, RotateTransform rotate, Size canvasSize)
        {
            if (size.Width <= 0.0 || size.Height <= 0.0) return new Vector(1.0, 1.0);
            var bounds = rotate.TransformBounds(size.ToRect());
            var scaleX = AllowScale(canvasSize.Width / bounds.Width);
            var scaleY = AllowScale(canvasSize.Height / bounds.Height);
            return new Vector(scaleX, scaleY);
        }

        /// <summary>
        /// Calculate fill-scale for PageFrame
        /// </summary>
        /// <param name="size"></param>
        /// <param name="span">PageContent 間の距離。スケールに影響されない固定値</param>
        /// <param name="angle">自動回転角度。90度単位のみ対応</param>
        /// <param name="canvasSize"></param>
        /// <returns>回転前に適用されるScale</returns>
        private Vector GetFillScaleWithSpan(Size size, double span, double angle, Size canvasSize)
        {
            Debug.Assert(Math.Abs(angle % 90) < 1.0, "Only 90-degree units are supported.");

            if (size.Width <= 0.0 || size.Height <= 0.0) return new Vector(1.0, 1.0);
            var radian = Math.PI * angle / 180.0;
            var isTranspose = Math.Abs(Math.Sin(radian)) > 0.5;
            if (isTranspose)
            {
                var scaleY = AllowScale((canvasSize.Height - span) / size.Width);
                var scaleX = AllowScale(canvasSize.Width / size.Height);
                return new Vector(scaleX, scaleY);
            }
            else
            {
                var scaleX = AllowScale((canvasSize.Width - span) / size.Width);
                var scaleY = AllowScale(canvasSize.Height / size.Height);
                return new Vector(scaleX, scaleY);
            }
        }

        private double AllowScale(double scale)
        {
            if (!AllowEnlarge)
            {
                scale = Math.Min(scale, OneScale());
            }
            if (!AllowReduce)
            {
                scale = Math.Max(scale, OneScale());
            }
            return scale;
        }

        public double[] CalcContentScale(IEnumerable<Size> contents)
        {
            if (!contents.Any())
            {
                return Array.Empty<double>();
            }

            if (contents.Count() == 1)
            {
                return new double[] { 1.0 };
            }

            if (StretchMode == PageStretchMode.None)
            {
                return contents.Select(e => 1.0).ToArray();
            }

            return WidePageStretch switch
            {
                WidePageStretch.None => contents.Select(e => 1.0).ToArray(),
                WidePageStretch.UniformHeight => CalcUniformHeightScale(contents),
                WidePageStretch.UniformWidth => CalcUniformWidthScale(contents),
                _ => throw new InvalidOperationException($"WidePageAlignment.{WidePageStretch} is not supported."),
            };
        }

        private static double[] CalcUniformHeightScale(IEnumerable<Size> contents)
        {
            var height = contents.Max(e => e.Height);
            return contents.Select(e => height / e.Height).ToArray();
        }

        private static double[] CalcUniformWidthScale(IEnumerable<Size> contents)
        {
            var width = contents.Sum(e => e.Width) / contents.Count();
            return contents.Select(e => width / e.Width).ToArray();
        }

        // DPIを加味した基準スケール
        private double OneScale() => 1.0 / _profile.DpiScale.ToFixedScale().DpiScaleX;
    }

    public static class SizeExtensions
    {
        public static Rect ToRect(this Size size)
        {
            return new Rect(-size.Width * 0.5, -size.Height * 0.5, size.Width, size.Height);
        }
    }
}
