using NeeView.PageFrames;
using System.Windows;

namespace NeeView
{
    public class PageFrameElementScaleFactory
    {
        private readonly PageFrameContext _bookContext;
        private readonly PageFrameTransformMap _transformMap;
        private readonly LoupeTransformContext _loupeTransform;

        public PageFrameElementScaleFactory(PageFrameContext bookContext, PageFrameTransformMap transformMap, LoupeTransformContext loupeTransform)
        {
            _bookContext = bookContext;
            _transformMap = transformMap;
            _loupeTransform = loupeTransform;
        }

        public PageFrameElementScale Create(PageFrame pageFrame)
        {
            var key = PageFrameTransformTool.CreateKey(pageFrame);
            var transformScale = _transformMap.GetScale(key);
            var transformAngle = _transformMap.GetAngle(key);

            return new PageFrameElementScale(
                layoutScale: pageFrame.Scale,
                renderScale: transformScale * _loupeTransform.Scale,
                renderAngle: transformAngle,
                baseScale: _bookContext.BaseScale,
                dpiScale: _bookContext.DpiScale);
        }

        public PageFrameElementScale Create(PageFrame pageFrame, IPageFrameTransform transform)
        {
            return new PageFrameElementScale(
                layoutScale: pageFrame.Scale,
                renderScale: transform.Scale * _loupeTransform.Scale,
                renderAngle: transform.Angle,
                baseScale: _bookContext.BaseScale,
                dpiScale: _bookContext.DpiScale);
        }

        public static PageFrameElementScale Create(PageFrame pageFrame, IPageFrameTransform transform, LoupeTransformContext loupeTransform, BaseScaleTransform baseScaleTransform, DpiScale dpiScale)
        {
            return new PageFrameElementScale(
                layoutScale: pageFrame.Scale,
                renderScale: transform.Scale * loupeTransform.Scale,
                renderAngle: transform.Angle,
                baseScale: baseScaleTransform.Scale,
                dpiScale: dpiScale);
        }
    }


}
