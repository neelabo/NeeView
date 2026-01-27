using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 移動制限を反映した慣性減速の EasingFunction を作成
    /// </summary>
    [LocalDebug]
    public partial class InertiaEaseFactory
    {
        public delegate HitData GetHitDataFunc(Point start, Vector delta);

        public InertiaEaseFactory(GetHitDataFunc getScrollLockHit, GetHitDataFunc getAreaLimitHit)
        {
            GetScrollLockHit = getScrollLockHit;
            GetAreaLimitHit = getAreaLimitHit;
        }

        /// <summary>
        /// 移動ロック制限の衝突計算用関数
        /// </summary>
        public GetHitDataFunc GetScrollLockHit { get; }

        /// <summary>
        /// エリア制限の衝突計算用関数
        /// </summary>
        public GetHitDataFunc GetAreaLimitHit { get; }

        /// <summary>
        /// 移動制限を反映した慣性減速の EasingFunction を作成
        /// </summary>
        /// <param name="start">初期座標</param>
        /// <param name="velocity">入力速度</param>
        /// <param name="acceleration">加速度(<0.0)</param>
        /// <returns>移動曲線情報</returns>
        public MultiEaseSet Create(Point start, Vector velocity, double acceleration)
        {
            Debug.Assert(acceleration < 0.0);

            var multiEaseSet = new MultiEaseSet();

            if (velocity.LengthSquared < 0.01 || acceleration < -0.999) return multiEaseSet;

            if (velocity.LengthSquared > 40.0 * 40.0)
            {
                velocity = velocity * (40.0 / velocity.Length);
            }

            // limit distance
            {
                var v0 = velocity.Length;

                // 慣性移動で停止するまでの時間と距離を求める
                var inertiaT = Kinematics.GetStopTime(v0, acceleration);
                var inertiaS = Kinematics.GetSpan(v0, acceleration, inertiaT);
                //NVDebug.WriteInfo("Inertia", $"v={v0:f2}, a={acceleration:f6}, s={inertiaS:f0}");

                // 最大距離を超えないように加速度を制限する
                var maxS = 30000.0;
                if (inertiaS > maxS)
                {
                    acceleration = Kinematics.GetAccelerate(v0, 0.0, maxS);
                    //inertiaT = Kinematics.GetStopTime(v0, acceleration);
                    //inertiaS = Kinematics.GetSpan(v0, acceleration, inertiaT);
                    //NVDebug.WriteInfo("Inertia", $"limit: v={v0:f2}, a={acceleration:f6}, s={inertiaS:f0}");
                }
            }

            var pos = start;

            // scroll lock
            {
                var easeSet = DecelerationEaseSetFactory.Create(velocity, acceleration, 1.0);
                var hit = GetScrollLockHit(pos, easeSet.Delta);

                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = DecelerationEaseSetFactory.Create(velocity, acceleration, hit.Rate);
                        multiEaseSet.Add(easeSet);
                        pos += easeSet.Delta;
                        velocity = easeSet.V1;
                    }
                    var vx = hit.XHit ? 0.0 : velocity.X;
                    var vy = hit.YHit ? 0.0 : velocity.Y;
                    velocity = new Vector(vx, vy);
                    LocalDebug.WriteLine($"Add.LockHit: Delta={easeSet.Delta:f2}, Rate={hit.Rate:f2}, V1={velocity:f2}");
                }
            }

            // area limit
            while (!velocity.NearZero(0.1))
            {
                var easeSet = DecelerationEaseSetFactory.Create(velocity, acceleration, 1.0);

                var hit = GetAreaLimitHit(pos, easeSet.Delta);
                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = DecelerationEaseSetFactory.Create(velocity, acceleration, hit.Rate);
                        multiEaseSet.Add(easeSet);
                        pos += easeSet.Delta;
                        velocity = easeSet.V1;
                    }
                    var vx = hit.XHit ? 0.0 : velocity.X;
                    var vy = hit.YHit ? 0.0 : velocity.Y;
                    velocity = new Vector(vx, vy);
                    LocalDebug.WriteLine($"Add.Hit: Delta={easeSet.Delta:f2}, Rate={hit.Rate:f2}, V1={velocity:f2}");
                }
                else
                {
                    multiEaseSet.Add(easeSet);
                    LocalDebug.WriteLine($"Add.End: Delta={easeSet.Delta:f2}, Rate={1}, V1={easeSet.V1:f2}");
                    break;
                }
            }

            return multiEaseSet;
        }

    }
}
