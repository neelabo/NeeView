using NeeLaboratory;
using System;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Page custom size
    /// </summary>
    public class CustomSize
    {
        static CustomSize() => Current = new CustomSize();
        public static CustomSize Current { get; }


        private readonly ImageCustomSizeConfig _customSize;

        private CustomSize()
        {
            _customSize = Config.Current.ImageCustomSize;
        }



        public Size TransformToCustomSize(Size originalSize)
        {
            if (!_customSize.IsEnabled || originalSize.IsEmptyOrZero())
            {
                return originalSize;
            }

            return ApplyApplicabilityRate(originalSize, ApplyAspectRatio(originalSize, _customSize.Size, _customSize.AspectRatio, _customSize.IsAlignLongSide));
        }


        private Size ApplyApplicabilityRate(Size sourceSize, Size targetSize)
        {
            var width = MathUtility.Lerp(sourceSize.Width, targetSize.Width, _customSize.ApplicabilityRate);
            var heigth = MathUtility.Lerp(sourceSize.Height, targetSize.Height, _customSize.ApplicabilityRate);
            return new Size(width, heigth);
        }


        private static Size ApplyAspectRatio(Size sourceSize, Size targetSize, CustomSizeAspectRatio aspectRatio, bool isTransposed)
        {
            if (sourceSize.IsEmptyOrZero()) return sourceSize;

            if (isTransposed)
            {
                targetSize = targetSize.Transpose();
            }

            switch (aspectRatio)
            {
                case CustomSizeAspectRatio.None:
                    return ApplyLongSide(targetSize, sourceSize, isTransposed);
                case CustomSizeAspectRatio.Origin:
                    return sourceSize.Uniformed(ApplyLongSide(targetSize, sourceSize, isTransposed));
                case CustomSizeAspectRatio.Ratio_1_1:
                    return ApplyLongSide(targetSize.AspectRatioUniformed(1.0, 1.0), sourceSize, isTransposed);
                case CustomSizeAspectRatio.Ratio_2_3:
                    return ApplyLongSide(targetSize.AspectRatioUniformed(2.0, 3.0), sourceSize, isTransposed);
                case CustomSizeAspectRatio.Ratio_4_3:
                    return ApplyLongSide(targetSize.AspectRatioUniformed(4.0, 3.0), sourceSize, isTransposed);
                case CustomSizeAspectRatio.Ratio_8_9:
                    return ApplyLongSide(targetSize.AspectRatioUniformed(8.0, 9.0), sourceSize, isTransposed);
                case CustomSizeAspectRatio.Ratio_16_9:
                    return ApplyLongSide(targetSize.AspectRatioUniformed(16.0, 9.0), sourceSize, isTransposed);
                case CustomSizeAspectRatio.HalfView:
                    {
                        var viewSize = MainViewComponent.Current.ViewSize;
                        return ApplyLongSide(targetSize.AspectRatioUniformed(viewSize.Width * 0.5, viewSize.Height), sourceSize, isTransposed);
                    }
                case CustomSizeAspectRatio.View:
                    {
                        var viewSize = MainViewComponent.Current.ViewSize;
                        return ApplyLongSide(targetSize.AspectRatioUniformed(viewSize.Width, viewSize.Height), sourceSize, isTransposed);
                    }
                default:
                    throw new NotImplementedException();
            }
        }


        private static Size ApplyLongSide(Size sourceSize, Size referenceSize, bool isTransposed)
        {
            if (!isTransposed)
            {
                return sourceSize;
            }

            var sourceHorizontally = sourceSize.IsHorizontally();
            var targetHorizontally = referenceSize.IsHorizontally();

            if (sourceHorizontally != targetHorizontally)
            {
                return sourceSize.Transpose();
            }
            else
            {
                return sourceSize;
            }
        }

    }
}
