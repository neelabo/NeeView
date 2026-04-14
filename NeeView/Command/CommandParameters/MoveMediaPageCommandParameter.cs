using Generator.Equals;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class MoveMediaPositionCommandParameter : CommandParameter
    {
        [DefaultEquality] private double _delta;

        [PropertyRange(0.0, 60.0, IsEditable = true)]
        public double Delta
        {
            get { return _delta; }
            set { SetProperty(ref _delta, Math.Max(AppMath.Round(value), 0.0)); }
        }
    }
}
