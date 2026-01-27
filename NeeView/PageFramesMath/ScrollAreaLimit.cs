using NeeView.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Windows;

namespace NeeView.Maths
{
    /// <summary>
    /// from NeeView.DragTransformControl
    /// スクロールエリア制限。
    /// 表示範囲内に収まる方向の座標計算を提供する。
    /// TODO: DragArea に統合可能？
    /// </summary>
    public class ScrollAreaLimit
    {
        /// <summary>
        /// コンテンツ領域
        /// </summary>
        private readonly Rect _contentRect;

        /// <summary>
        /// 表示領域
        /// </summary>
        private readonly Rect _viewRect;

        /// <summary>
        /// スクロールエリア制限計算
        /// </summary>
        /// <param name="contentRect">コンテンツ領域</param>
        /// <param name="viewRect">表示領域</param>
        public ScrollAreaLimit(Rect contentRect, Rect viewRect)
        {
            _contentRect = contentRect;
            _viewRect = viewRect;
        }

        /// <summary>
        /// 表示領域の移動量限界計算
        /// </summary>
        /// <param name="delta">表示領域の希望移動量</param>
        /// <returns>制限された移動量</returns>
        public Vector GetLimitViewMove(Vector delta)
        {
            var marginX = _contentRect.Width < _viewRect.Width ? _viewRect.Width - _contentRect.Width : 0;
            var marginY = _contentRect.Height < _viewRect.Height ? _viewRect.Height - _contentRect.Height : 0;

            if (delta.X < 0 && _viewRect.Left + delta.X < _contentRect.Left - marginX)
            {
                delta.X = Math.Min(_contentRect.Left - marginX - _viewRect.Left, 0.0);
            }
            else if (delta.X > 0 && _viewRect.Right + delta.X > _contentRect.Right + marginX)
            {
                delta.X = Math.Max(_contentRect.Right + marginX - _viewRect.Right, 0.0);
            }

            if (delta.Y < 0 && _viewRect.Top + delta.Y < _contentRect.Top - marginY)
            {
                delta.Y = Math.Min(_contentRect.Top - marginY - _viewRect.Top, 0.0);
            }
            else if (delta.Y > 0 && _viewRect.Bottom + delta.Y > _contentRect.Bottom + marginY)
            {
                delta.Y = Math.Max(_contentRect.Bottom + marginY - _viewRect.Bottom, 0.0);
            }

            return delta;
        }

        /// <summary>
        /// コンテンツ領域の移動量限界計算
        /// </summary>
        /// <param name="delta">コンテンツ領域の希望移動量</param>
        /// <returns>制限された移動量</returns>
        public Vector GetLimitContentMove(Vector delta)
        {
            var marginX = _contentRect.Width < _viewRect.Width ? _viewRect.Width - _contentRect.Width : 0;
            var marginY = _contentRect.Height < _viewRect.Height ? _viewRect.Height - _contentRect.Height : 0;

            if (delta.X < 0 && _contentRect.Right + delta.X < _viewRect.Right - marginX)
            {
                delta.X = Math.Min(_viewRect.Right - marginX - _contentRect.Right, 0.0);
            }
            else if (delta.X > 0 && _contentRect.Left + delta.X > _viewRect.Left + marginX)
            {
                delta.X = Math.Max(_viewRect.Left + marginX - _contentRect.Left, 0.0);
            }

            if (delta.Y < 0 && _contentRect.Bottom + delta.Y < _viewRect.Bottom - marginY)
            {
                delta.Y = Math.Min(_viewRect.Bottom - marginY - _contentRect.Bottom, 0.0);
            }
            else if (delta.Y > 0 && _contentRect.Top + delta.Y > _viewRect.Top + marginY)
            {
                delta.Y = Math.Max(_viewRect.Top + marginY - _contentRect.Top, 0.0);
            }

            return delta;
        }

        /// <summary>
        /// コンテンツ領域の衝突判定
        /// </summary>
        /// <param name="start">初期座標</param>
        /// <param name="delta">移動量</param>
        /// <returns>衝突データ</returns>
        public HitData HitTest(Point start, Vector delta)
        {
            var marginX = _contentRect.Width < _viewRect.Width ? _viewRect.Width - _contentRect.Width : 0;
            var marginY = _contentRect.Height < _viewRect.Height ? _viewRect.Height - _contentRect.Height : 0;

            var rateX = double.PositiveInfinity;
            var rateY = double.PositiveInfinity;

            if (delta.X < 0 && _contentRect.Right + delta.X < _viewRect.Right - marginX)
            {
                var x = Math.Min(_viewRect.Right - marginX - _contentRect.Right, 0.0);
                rateX = x / delta.X;
            }
            else if (delta.X > 0 && _contentRect.Left + delta.X > _viewRect.Left + marginX)
            {
                var x = Math.Max(_viewRect.Left + marginX - _contentRect.Left, 0.0);
                rateX = x / delta.X;
            }

            if (delta.Y < 0 && _contentRect.Bottom + delta.Y < _viewRect.Bottom - marginY)
            {
                var y = Math.Min(_viewRect.Bottom - marginY - _contentRect.Bottom, 0.0);
                rateY = y / delta.Y;
            }
            else if (delta.Y > 0 && _contentRect.Top + delta.Y > _viewRect.Top + marginY)
            {
                var y = Math.Max(_viewRect.Top + marginY - _contentRect.Top, 0.0);
                rateY = y / delta.Y;
            }

            if (rateX < 1.0 || rateY < 1.0)
            {
                var xHit = false;
                var yHit = false;
                double rate;
                if (Math.Abs(rateX - rateY) < 0.001)
                {
                    xHit = true;
                    yHit = true;
                    rate = Math.Min(rateX, rateY);
                }
                else if (rateX < rateY)
                {
                    xHit = true;
                    rate = rateX;
                }
                else
                {
                    yHit = true;
                    rate = rateY;
                }

                return new HitData(start, delta)
                {
                    XHit = xHit,
                    YHit = yHit,
                    Rate = rate,
                };
            }

            return new HitData(start, delta);
        }


        /// <summary>
        ///  エリアサイズ内に座標を収める
        /// </summary>
        /// <param name="centered">範囲内に収まるときは中央に配置</param>
        /// <returns>補正された座標</returns>
        public Point SnapView(bool centered)
        {
            const double margin = 1.0;

            var pos = _contentRect.Center();

            // ウィンドウサイズ変更直後は rect のスクリーン座標がおかしい可能性があるのでPositionから計算しなおす
            var rect = new Rect()
            {
                X = pos.X - _contentRect.Width * 0.5 + _viewRect.Width * 0.5,
                Y = pos.Y - _contentRect.Height * 0.5 + _viewRect.Height * 0.5,
                Width = _contentRect.Width,
                Height = _contentRect.Height,
            };

            var minX = _viewRect.Width * -0.5 + rect.Width * 0.5;
            var maxX = minX + _viewRect.Width - rect.Width;

            if (rect.Width <= _viewRect.Width + margin)
            {
                if (centered)
                {
                    pos.X = 0.0;
                }
                else if (rect.Left < 0)
                {
                    pos.X = minX;
                }
                else if (rect.Right > _viewRect.Width)
                {
                    pos.X = maxX;
                }
            }
            else
            {
                if (rect.Left > 0)
                {
                    pos.X -= rect.Left;
                }
                else if (rect.Right < _viewRect.Width)
                {
                    pos.X += _viewRect.Width - rect.Right;
                }
            }

            var minY = _viewRect.Height * -0.5 + rect.Height * 0.5;
            var maxY = minY + _viewRect.Height - rect.Height;

            if (rect.Height <= _viewRect.Height + margin)
            {
                if (centered)
                {
                    pos.Y = 0.0;
                }
                else if (rect.Top < 0)
                {
                    pos.Y = minY;
                }
                else if (rect.Bottom > _viewRect.Height)
                {
                    pos.Y = maxY;
                }
            }
            else
            {
                if (rect.Top > 0)
                {
                    pos.Y = minY;
                }
                else if (rect.Bottom < _viewRect.Height)
                {
                    pos.Y = maxY;
                }
            }

            return pos;
        }
    }
}
