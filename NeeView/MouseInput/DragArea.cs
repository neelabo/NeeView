using NeeView.ComponentModel;
using System.Linq;
using System.Windows;

namespace NeeView
{

    /// <summary>
    /// from NeeView.DragArea
    /// - ElementではなくRectを扱うように変更
    /// - 正方向へのはみ出しを計算に入れた
    /// </summary>
    public class DragArea
    {
        public DragArea(Rect viewRect, Rect contentRect)
        {
            ViewRect = viewRect;
            ContentRect = contentRect;

            var left = ContentRect.Left < ViewRect.Left ? ContentRect.Left - ViewRect.Left : 0;
            var right = ContentRect.Right > ViewRect.Right ? ContentRect.Right - ViewRect.Right : 0;
            var top = ContentRect.Top < ViewRect.Top ? ContentRect.Top - ViewRect.Top : 0;
            var bottom = ContentRect.Bottom > ViewRect.Bottom ? ContentRect.Bottom - ViewRect.Bottom : 0;
            Over = new Rect(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// ビューエリア矩形
        /// </summary>
        public Rect ViewRect { get; private set; }

        /// <summary>
        /// コンテンツ矩形
        /// </summary>
        public Rect ContentRect { get; private set; }

        /// <summary>
        /// ビューエリアオーバー情報.
        /// Left,Top はターゲットがビューエリアからマイナスにはみ出している場合のみその値を記憶する。
        /// Right,Bottom はターゲットがビューエリアからプラスにはみ出している場合のみその値を記憶する。
        /// </summary>
        public Rect Over { get; private set; }

        // コントロールの表示RECTを取得
        public static Rect GetRealSize(FrameworkElement target, FrameworkElement parent)
        {
            Point[] pos = new Point[4];
            double width = target.ActualWidth;
            double height = target.ActualHeight;

            pos[0] = target.TranslatePoint(new Point(0, 0), parent);
            pos[1] = target.TranslatePoint(new Point(width, 0), parent);
            pos[2] = target.TranslatePoint(new Point(0, height), parent);
            pos[3] = target.TranslatePoint(new Point(width, height), parent);

            var min = new Point(pos.Min(e => e.X), pos.Min(e => e.Y));
            var max = new Point(pos.Max(e => e.X), pos.Max(e => e.Y));

            return new Rect(min, max);
        }


        // エリアサイズ内に座標を収める
        public Point SnapView(Point pos)
        {
            return (Point)SnapView((Vector)pos, false);
        }

        /// <summary>
        ///  エリアサイズ内に座標を収める
        /// </summary>
        /// <param name="pos">コンテンツ中心座標</param>
        /// <param name="centered">範囲内に収まるときは中央に配置</param>
        /// <returns>補正された中心座標</returns>
        public Vector SnapView(Vector pos, bool centered)
        {
            const double margin = 1.0;

            // ウィンドウサイズ変更直後はrectのスクリーン座標がおかしい可能性があるのでPositionから計算しなおす
            var rect = new Rect()
            {
                X = pos.X - ContentRect.Width * 0.5 + ViewRect.Width * 0.5,
                Y = pos.Y - ContentRect.Height * 0.5 + ViewRect.Height * 0.5,
                Width = ContentRect.Width,
                Height = ContentRect.Height,
            };

            var minX = ViewRect.Width * -0.5 + rect.Width * 0.5;
            var maxX = minX + ViewRect.Width - rect.Width;

            if (rect.Width <= ViewRect.Width + margin)
            {
                if (centered)
                {
                    pos.X = 0.0;
                }
                else if (rect.Left < 0)
                {
                    pos.X = minX;
                }
                else if (rect.Right > ViewRect.Width)
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
                else if (rect.Right < ViewRect.Width)
                {
                    pos.X += ViewRect.Width - rect.Right;
                }
            }

            var minY = ViewRect.Height * -0.5 + rect.Height * 0.5;
            var maxY = minY + ViewRect.Height - rect.Height;

            if (rect.Height <= ViewRect.Height + margin)
            {
                if (centered)
                {
                    pos.Y = 0.0;
                }
                else if (rect.Top < 0)
                {
                    pos.Y = minY;
                }
                else if (rect.Bottom > ViewRect.Height)
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
                else if (rect.Bottom < ViewRect.Height)
                {
                    pos.Y = maxY;
                }
            }

            return pos;
        }

        /// <summary>
        /// コンテンツをアライメント位置に配置する移動量を求める
        /// </summary>
        /// <param name="horizontal">水平位置</param>
        /// <param name="vertical">垂直位置</param>
        /// <param name="snap">既に表示範囲内であってもアライメントを強制する</param>
        /// <returns></returns>
        public Vector SnapAlignment(HorizontalAlignment horizontal, VerticalAlignment vertical, bool snap)
        {
            var p0 = ContentRect.Center();
            var p1 = GetAlignmentCenter(horizontal, vertical, snap);

            var delta = p1 - p0;
            return delta;
        }

        public Point GetAlignmentCenter(HorizontalAlignment horizontal, VerticalAlignment vertical, bool snap)
        {
            Point p0 = ContentRect.Center();
            Point p1 = default;
            Point p2 = SnapView(p0);

            if (snap || ContentRect.Width > ViewRect.Width)
            {
                p1.X = horizontal switch
                {
                    HorizontalAlignment.Left => (ContentRect.Width - ViewRect.Width) * 0.5,
                    HorizontalAlignment.Right => (ContentRect.Width - ViewRect.Width) * -0.5,
                    _ => 0.0,
                };
            }
            else
            {
                p1.X = p2.X;
            }

            if (snap || ContentRect.Height > ViewRect.Height)
            {
                p1.Y = vertical switch
                {
                    VerticalAlignment.Top => (ContentRect.Height - ViewRect.Height) * 0.5,
                    VerticalAlignment.Bottom => (ContentRect.Height - ViewRect.Height) * -0.5,
                    _ => 0,
                };
            }
            else
            {
                p1.Y = p2.Y;
            }

            return p1;
        }
    }
}
