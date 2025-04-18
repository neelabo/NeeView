﻿// from https://github.com/takanemu/WPFDragAndDropSample

// WIN32APIの高DPI対応
// from http://grabacr.net/archives/1105

using NeeView.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeeView.Windows
{
    /// <summary>
    /// マウスカーソル座標取得(WIN32)
    /// </summary>
    public static class CursorInfo
    {
        /// <summary>
        /// 現在のマウスカーソル座標を取得
        /// </summary>
        /// <param name="visual">ハンドルを取得するためのビジュアル要素</param>
        /// <returns></returns>
        public static Point GetNowPosition(Visual visual)
        {
            NativeMethods.GetCursorPos(out POINT point);

            if (HwndSource.FromVisual(visual) is not HwndSource source)
            {
                // visual does not exist in virual tree.
                return new Point(double.NaN, double.NaN);
            }

            var hwnd = source.Handle;

            var isSuccess = NativeMethods.ScreenToClient(hwnd, ref point);
            if (!isSuccess)
            {
                return new Point(double.NaN, double.NaN);
            }

            var dpiScaleFactor = GetDpiScaleFactor(visual);
            return new Point(point.x / dpiScaleFactor.X, point.y / dpiScaleFactor.Y);
        }


        /// <summary>
        /// 現在のマウスカーソルのスクリーン座標を取得
        /// </summary>
        /// <returns></returns>
        public static Point GetNowScreenPosition(Window window)
        {
            NativeMethods.GetCursorPos(out POINT point);

            var dpiScaleFactor = GetDpiScaleFactor(window);
            return new Point(point.x / dpiScaleFactor.X, point.y / dpiScaleFactor.Y);
        }


        /// <summary>
        /// 現在の <see cref="T:System.Windows.Media.Visual"/> から、DPI 倍率を取得します。
        /// </summary>
        /// <returns>
        /// X 軸 および Y 軸それぞれの DPI 倍率を表す <see cref="T:System.Windows.Point"/>
        /// 構造体。取得に失敗した場合、(1.0, 1.0) を返します。
        /// </returns>
        public static Point GetDpiScaleFactor(System.Windows.Media.Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null && source.CompositionTarget != null)
            {
                return new Point(
                    source.CompositionTarget.TransformToDevice.M11,
                    source.CompositionTarget.TransformToDevice.M22);
            }

            return new Point(1.0, 1.0);
        }
    }
}
