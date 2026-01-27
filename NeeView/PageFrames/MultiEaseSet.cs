using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 複数の曲線を連続した曲線として管理する
    /// </summary>
    public class MultiEaseSet : IEaseSet
    {
        public void Add(EaseSet easeSet)
        {
            var t = easeSet.Milliseconds;

            EaseX.Add(easeSet.EaseX, easeSet.Delta.X, t);
            EaseY.Add(easeSet.EaseY, easeSet.Delta.Y, t);

            Delta += easeSet.Delta;
            Milliseconds += t;
        }

        public bool IsValid => Milliseconds > 0;

        public Vector Delta { get; private set; }
        public double Milliseconds { get; private set; }
        public MultiEase EaseX { get; } = new();
        public MultiEase EaseY { get; } = new();

        IEasingFunction IEaseSet.EaseX => EaseX;
        IEasingFunction IEaseSet.EaseY => EaseY;
    }




}
