using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// UnsharpMask setting for resize filter (PhotoSauce.MagicScaler)
    /// </summary>
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class UnsharpMaskConfig : BindableBase, ICloneable
    {
        [DefaultEquality] private int _amount = 40;
        [DefaultEquality] private double _radius = 1.5;
        [DefaultEquality] private int _threshold = 0;

        /// <summary>
        /// UnsharpAmount property.
        /// 25-200
        /// </summary>
        [PropertyRange(25, 200)]
        [DefaultValue(40)]
        public int Amount
        {
            get { return _amount; }
            set { SetProperty(ref _amount, value); }
        }

        /// <summary>
        /// UnsharpRadius property.
        /// 0.3-3.0
        /// </summary>
        [PropertyRange(0.3, 3.0, TickFrequency = 0.05)]
        [DefaultValue(1.5)]
        public double Radius
        {
            get { return _radius; }
            set { SetProperty(ref _radius, value); }
        }

        /// <summary>
        /// UnsharpThreshold property.
        /// 0-10
        /// </summary>
        [PropertyRange(0, 10)]
        [DefaultValue(0)]
        public int Threshold
        {
            get { return _threshold; }
            set { SetProperty(ref _threshold, value); }
        }


        public UnsharpMaskSettings CreateUnsharpMaskSetting()
        {
            return new UnsharpMaskSettings(_amount, _radius, (byte)_threshold);
        }

        public int GetEnvironmentHashCode()
        {
            return HashCode.Combine(_amount, _radius, _threshold);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

}
