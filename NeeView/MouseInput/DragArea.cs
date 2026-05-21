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

        /// <summary>
        ///  エリアサイズ内にコンテンツを収める
        /// </summary>
        /// <param name="centered">範囲内に収まるときは中央に配置</param>
        /// <returns>補正された中心座標</returns>
        private Point SnapView(bool centered)
        {
            const double margin = 1.0;

            var pos = ContentRect.Center();

            if (ContentRect.Width <= ViewRect.Width + margin)
            {
                if (centered)
                {
                    pos.X = ViewRect.Center().X;
                }
                else if (ContentRect.Left < ViewRect.Left)
                {
                    pos.X = ViewRect.Left + ContentRect.Width * 0.5;
                }
                else if (ContentRect.Right > ViewRect.Right)
                {
                    pos.X = ViewRect.Right - ContentRect.Width * 0.5;
                }
            }
            else
            {
                if (ContentRect.Left > ViewRect.Left + margin)
                {
                    pos.X = ViewRect.Left + ContentRect.Width * 0.5;
                }
                else if (ContentRect.Right  < ViewRect.Right - margin)
                {
                    pos.X = ViewRect.Right - ContentRect.Width * 0.5;
                }
            }

            if (ContentRect.Height <= ViewRect.Height + margin)
            {
                if (centered)
                {
                    pos.Y = ViewRect.Center().Y;
                }
                else if (ContentRect.Top < ViewRect.Top)
                {
                    pos.Y = ViewRect.Top + ContentRect.Height * 0.5;
                }
                else if (ContentRect.Bottom > ViewRect.Bottom)
                {
                    pos.Y = ViewRect.Bottom - ContentRect.Height * 0.5;
                }
            }
            else
            {
                if (ContentRect.Top > ViewRect.Top + margin)
                {
                    pos.Y = ViewRect.Top + ContentRect.Height * 0.5;
                }
                else if (ContentRect.Bottom < ViewRect.Bottom - margin)
                {
                    pos.Y = ViewRect.Bottom - ContentRect.Height * 0.5;
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

        private Point GetAlignmentCenter(HorizontalAlignment horizontal, VerticalAlignment vertical, bool snap)
        {
            Point p1 = default;
            Point p2 = SnapView(false);

            if (snap || ContentRect.Width > ViewRect.Width)
            {
                p1.X = horizontal switch
                {
                    HorizontalAlignment.Left => ViewRect.Left + ContentRect.Width * 0.5,
                    HorizontalAlignment.Right => ViewRect.Right - ContentRect.Width * 0.5,
                    _ => ViewRect.Left + ViewRect.Width * 0.5
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
                    VerticalAlignment.Top => ViewRect.Top + ContentRect.Height * 0.5,
                    VerticalAlignment.Bottom => ViewRect.Bottom - ContentRect.Height * 0.5,
                    _ => ViewRect.Top + ViewRect.Height * 0.5
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
