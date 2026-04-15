using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NeeView
{
#if DEBUG
    /// <summary>
    /// 処理の有効/無効をカウントする開発機能。
    /// Task と ValueTask の判断基準などに利用する
    /// </summary>
    public class NVHitCount
    {
        public static NVHitCount Instance = new NVHitCount();

        private readonly Dictionary<string, TaskCount> _map = new();

        public void HitCount(string key)
        {
            if (_map.TryGetValue(key, out var value))
            {
                value.HitCount++;
            }
            else
            {
                _map.Add(key, new TaskCount() { HitCount = 1 });
            }
        }

        public void UnHitCount(string key)
        {
            if (_map.TryGetValue(key, out var value))
            {
                value.UnHitCount++;
            }
            else
            {
                _map.Add(key, new TaskCount() { UnHitCount = 1 });
            }
        }

        public void DumpCount()
        {
            foreach (var item in _map)
            {
                Trace.WriteLine($"TaskCount: {item.Key}: Hit={item.Value.HitCount}, UnHit={item.Value.UnHitCount}");
            }
        }


        [Conditional("DEBUG")]
        public static void Hit([CallerMemberName] string memberName = "")
        {
            Instance.HitCount(memberName);
        }

        [Conditional("DEBUG")]
        public static void UnHit([CallerMemberName] string memberName = "")
        {
            Instance.UnHitCount(memberName);
        }

        [Conditional("DEBUG")]
        public static void Dump()
        {
            Instance.DumpCount();
        }


        private class TaskCount
        {
            public int UnHitCount { get; set; }
            public int HitCount { get; set; }
        }
    }
#endif
}
