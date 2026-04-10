//#define LOCAL_DEBUG

using System;
using System.Windows;

namespace NeeView.PageFrames
{
    public static class TransformTools
    {
        /// <summary>
        /// 座標をデバイスピクセルに合わせた位置に丸める
        /// </summary>
        /// <param name="value"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Point RoundPoint(this Point value)
        {
            var mainView = MainViewComponent.Current.MainView;
            var dpi = mainView.DpiProvider.DpiScale;
          
            var px = Math.Round(value.X * dpi.DpiScaleX) / dpi.DpiScaleX;
            var py = Math.Round(value.Y * dpi.DpiScaleY) / dpi.DpiScaleY;
            return new Point(px, py);
        }
    }
}
