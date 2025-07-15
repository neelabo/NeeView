using NeeView.PageFrames;
using System.Windows;

namespace NeeView.Maths
{
    /// <summary>
    /// from NeeView.DragTransformControl
    /// スクロールロック
    /// </summary>
    public class ScrollLock
    {
        private bool _lockMoveX = true;
        private bool _lockMoveY = true;


        public bool IsXLocked => _lockMoveX;
        public bool IsYLocked => _lockMoveY;


        public void Update(Rect contentRect, Rect viewRect)
        {
            var area = new DragArea(viewRect, contentRect);

            double margin = 1.1;

            if (_lockMoveX)
            {
                if (area.Over.Left + margin < 0 || area.Over.Right - margin > 0)
                {
                    _lockMoveX = false;
                }
            }
            if (_lockMoveY)
            {
                if (area.Over.Top + margin < 0 || area.Over.Bottom - margin > 0)
                {
                    _lockMoveY = false;
                }
            }
        }

        public void Lock()
        {
            SetLock(true);
        }

        public void Unlock()
        {
            SetLock(false);
        }

        public void SetLock(bool value)
        {
            _lockMoveX = value;
            _lockMoveY = value;
        }

        /// <summary>
        /// 移動ベクトルに制限を適用した値を返す
        /// </summary>
        public Vector Limit(Vector delta)
        {
            return new Vector(_lockMoveX ? 0.0 : delta.X, _lockMoveY ? 0.0 : delta.Y);
        }

        /// <summary>
        /// 移動制限の衝突判定
        /// </summary>
        /// <param name="start">初期座標</param>
        /// <param name="delta">移動量</param>
        /// <returns>衝突データ</returns>
        public HitData HitTest(Point start, Vector delta)
        {
            bool xHit = false;
            bool yHit = false;

            if (_lockMoveX && delta.X != 0.0)
            {
                xHit = true;
            }
            if (_lockMoveY && delta.Y != 0.0)
            {
                yHit = true;
            }

            return new HitData(start, delta)
            {
                XHit = xHit,
                YHit = yHit,
                Rate = 0.0,
            };
        }
    }
}
