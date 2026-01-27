using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 移動衝突判定用座標
    /// </summary>
    public class HitData
    {
        public HitData(Point start, Vector delta)
        {
            Start = start;
            Delta = delta;
        }

        /// <summary>
        /// 開始座標
        /// </summary>
        public Point Start { get; init; }

        /// <summary>
        /// 移動量
        /// </summary>
        public Vector Delta { get; init; }

        /// <summary>
        /// 衝突位置のレート(0.0-1.0)
        /// </summary>
        /// <remarks>
        /// 衝突がない場合は不定
        /// </remarks>
        public double Rate { get; init; }

        /// <summary>
        /// X方向の衝突あり
        /// </summary>
        public bool XHit { get; init; }

        /// <summary>
        /// Y方向の衝突あり
        /// </summary>
        public bool YHit { get; init; }

        /// <summary>
        /// 衝突あり
        /// </summary>
        public bool IsHit => XHit || YHit;
    }




}
