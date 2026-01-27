using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// DecelerationEase 生成
    /// </summary>
    public static class DecelerationEaseSetFactory
    {
        /// <summary>
        /// 初速度から DecelerationEase のセットを生成する
        /// </summary>
        /// <param name="velocity">初速度</param>
        /// <param name="acceleration">加速度(<0.0)</param>
        /// <param name="sRate">Easing関数の適用範囲 [0.0-1.0]</param>
        /// <returns></returns>
        public static EaseSet Create(Vector velocity, double acceleration, double sRate)
        {
            Debug.Assert(acceleration < 0.0);

            var v0 = velocity.Length;
            var a = acceleration;

            // 慣性移動で停止するまでの時間と距離を求める
            var inertiaT = Kinematics.GetStopTime(v0, a);
            var inertiaS = Kinematics.GetSpan(v0, a, inertiaT);
            if (Math.Abs(inertiaS) < 0.1)
            {
                return new EaseSet(VectorExtensions.Zero, 0.0, new QuarticEase(), new QuarticEase(), velocity, velocity);
            }

            // 指定範囲での距離と時間を求める
            var s = inertiaS * sRate;
            var t = Kinematics.GetTime(v0, a, s);
            if (double.IsNaN(t))
            {
                // ここにくることはないはずだが、もしものための回避値を設定
                Debug.Assert(!double.IsNaN(t));
                t = inertiaT * sRate;
            }

            var delta = velocity.TransScalar(s);
            var tRate = t / inertiaT;

            var easeX = new DecelerationEase() { Acceleration = a, MaxRate = tRate };
            var easeY = new DecelerationEase() { Acceleration = a, MaxRate = tRate };

            //var testX = easeX.Ease(1.0) * delta.X;
            //Debug.Assert(testX == delta.X);
            //var testY = easeY.Ease(1.0) * delta.Y;
            //Debug.Assert(testY == delta.Y);

            var v1 = Kinematics.GetVelocity(v0, a, t);
            var lastVelocity = velocity.TransScalar(v1);

            //Debug.WriteLine($"EaseSet: v0={velocity:f2}({v0:f2}), v1={lastVelocity:f2}({v1:f2}), delta={delta:f2}, ms={t:f0}");
            return new EaseSet(delta, t, easeX, easeY, velocity, lastVelocity);
        }
    }




}
