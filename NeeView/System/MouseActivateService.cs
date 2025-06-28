//#define LOCAL_DEBUG

using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// マウスによるすべてのウィンドウアクティブ化の監視
    /// </summary>
    public class MouseActivateService
    {
        static MouseActivateService() => Current = new();
        public static MouseActivateService Current { get; }


        private readonly List<MouseActivate> _activates = [];


        public bool IsMouseActivate => _activates.Any(e => e.IsMouseActivate);


        public void Add(MouseActivate activate)
        {
            if (_activates.Contains(activate)) return;

            _activates.Add(activate);
        }

        public void Remove(MouseActivate activate)
        {
            _activates.Remove(activate);
        }
    }
}
