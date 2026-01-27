using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 座標移動曲線情報
    /// </summary>
    public interface IEaseSet
    {
        /// <summary>
        /// 移動量
        /// </summary>
        Vector Delta { get; }

        /// <summary>
        /// 移動時間
        /// </summary>
        double Milliseconds { get; }

        /// <summary>
        /// X座標の移動曲線
        /// </summary>
        IEasingFunction EaseX { get; }

        /// <summary>
        /// Y座標の移動曲線
        /// </summary>
        IEasingFunction EaseY { get; }
    }




}
