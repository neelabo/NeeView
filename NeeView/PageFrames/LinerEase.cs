using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public class LinerEase : EasingFunctionBase
    {
        public LinerEase() { }

        protected override Freezable CreateInstanceCore()
        {
            return new LinerEase();
        }

        protected override double EaseInCore(double normalizedTime)
        {
            return normalizedTime;
        }
    }
}
