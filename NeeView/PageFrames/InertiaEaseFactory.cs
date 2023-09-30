﻿using System.Diagnostics;
using System.Windows;
using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 移動制限を反映した慣性減速の EasingFunction を作成
    /// </summary>
    public class InertiaEaseFactory
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
        /// <returns>移動曲線情報</returns>
        public MultiEaseSet Create(Point start, Vector velocity)
        {
            var multiEaseSet = new MultiEaseSet();

            if (velocity.LengthSquared < 0.01) return multiEaseSet;

            if (velocity.LengthSquared > 40.0 * 40.0)
            {
                velocity = velocity * (40.0 / velocity.Length);
            }

            var pos = start;

            // scroll lock
            {
                var easeSet = DecelerationEaseSetFactory.Create(velocity, 1.0);
                var hit = GetScrollLockHit(pos, easeSet.Delta);

                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = DecelerationEaseSetFactory.Create(velocity, hit.Rate);
                        multiEaseSet.Add(easeSet);
                        pos += easeSet.Delta;
                        velocity = easeSet.V1;
                    }
                    var vx = hit.XHit ? 0.0 : velocity.X;
                    var vy = hit.YHit ? 0.0 : velocity.Y;
                    velocity = new Vector(vx, vy);
                    Trace($"Add.LockHit: Delta={easeSet.Delta:f2}, Rate={hit.Rate:f2}, V1={velocity:f2}");
                }
            }

            // area limit
            while (!velocity.NearZero(0.1))
            {
                var easeSet = DecelerationEaseSetFactory.Create(velocity, 1.0);

                var hit = GetAreaLimitHit(pos, easeSet.Delta);
                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = DecelerationEaseSetFactory.Create(velocity, hit.Rate);
                        multiEaseSet.Add(easeSet);
                        pos += easeSet.Delta;
                        velocity = easeSet.V1;
                    }
                    var vx = hit.XHit ? 0.0 : velocity.X;
                    var vy = hit.YHit ? 0.0 : velocity.Y;
                    velocity = new Vector(vx, vy);
                    Trace($"Add.Hit: Delta={easeSet.Delta:f2}, Rate={hit.Rate:f2}, V1={velocity:f2}");
                }
                else
                {
                    multiEaseSet.Add(easeSet);
                    Trace($"Add.End: Delta={easeSet.Delta:f2}, Rate={1}, V1={easeSet.V1:f2}");
                    break;
                }
            }

            return multiEaseSet;
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{nameof(InertiaEaseFactory)}: {string.Format(s, args)}");
        }
    }
}