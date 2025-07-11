using NeeLaboratory;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// タッチ操作用トランスフォーム
    /// </summary>
    public class TouchDragTransform
    {
        public TouchDragTransform()
        {
        }

        public TouchDragTransform(Vector trans, double angle, double scale) : this(trans, angle, scale, default)
        {
        }

        public TouchDragTransform(Vector trans, double angle, double scale, Vector center)
        {
            Trans = trans;
            Angle = angle;
            Scale = scale;
            Center = center;
        }


        public Vector Trans { get; init; }
        public double Angle { get; init; }
        public double Scale { get; init; } = 1.0;

        // 回転、拡大縮小の中心
        public Vector Center { get; init; }


        public TouchDragTransform Clone()
        {
            return (TouchDragTransform)this.MemberwiseClone();
        }
    }
}
