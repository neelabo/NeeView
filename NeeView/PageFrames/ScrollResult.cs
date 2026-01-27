using NeeView.ComponentModel;
using System.Windows;

namespace NeeView.PageFrames
{
    public class ScrollResult
    {
        public ScrollResult(NScrollType scrollType, Vector vector)
        {
            ScrollType = scrollType;
            Vector = vector;
        }

        public NScrollType ScrollType { get; }
        public Vector Vector { get; }

        public bool IsTerminated => Vector.IsZero();
        public bool IsLineBreak => Vector.IsZero() || ScrollType != NScrollType.Diagonal && Vector.X != 0.0 && Vector.Y != 0.0;
    }

}
