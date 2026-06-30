using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeView.Collections.ObjectModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [Equatable(Explicit = true)]
    public partial class ColorizeEffectUnit : EffectUnit
    {
        private static List<ColorizeControlPoint> _default = new()
        {
            new ColorizeControlPoint(Colors.Black, 3.0),
            new ColorizeControlPoint(Colors.Indigo, 1.0),
            new ColorizeControlPoint(Colors.IndianRed, 1.0),
            new ColorizeControlPoint(Colors.Wheat, 1.0),
            new ColorizeControlPoint(Colors.AliceBlue, 1.0),
        };

        public ColorizeEffectUnit() : base(EffectSampleType.Luminance)
        {
            Points.CollectionChanged += (s, e) => UpdateLutBrush();
            UpdateLutBrush();
        }

        [ObservableProperty]
        [OrderedEquality]
        [DiffJsonDefault(typeof(PointsDefaultable))]
        [PropertyMember(Reset = typeof(PointsResetable))]
        public partial ColorizeControlPointCollection Points { get; set; } = new(_default);

        [ObservableProperty]
        [DefaultEquality]
        [PropertyRange(0.0, 1.0)]
        [DefaultValue(1.0)]
        public partial double LuminanceWeight { get; set; } = 1.0;

        [ObservableProperty]
        [JsonIgnore]
        [PropertyMapIgnore]
        public partial Brush LutBrush { get; private set; } = Effect.ImplicitInput;

        private void UpdateLutBrush()
        {
            Debug.Assert(Points.Count >= 2);
            LutBrush = ColorizeEffectTools.CreateLutBrush(Points);
        }


        public class PointsDefaultable : IDefaultable
        {
            public bool IsDefault(object? obj)
            {
                if (obj is not ObservableCollectionEx<ColorizeControlPoint> x) throw new InvalidOperationException($"Not the target type: {obj?.GetType().FullName}");

                if (x.Count != _default.Count) return false;

                for (int i=0; i<x.Count; i++)
                {
                    if (!x[i].ValueEquals(_default[i])) return false;
                }

                return true;
            }
        }

        public class PointsResetable : IProeprtyResetable
        {
            public void ResetProperties(PropertyValueSource source)
            {
                var points = ((ColorizeEffectUnit)source.Source).Points;

                points.Reset(_default);
            }
        }
    }



    public class ColorizeEffectAdapter : EffectAdapter
    {
        private readonly ColorizeEffect _effect = new();
        private readonly ColorizeEffectUnit _source;

        public override Effect Effect => _effect;
        public override EffectUnit Source => _source;

        public ColorizeEffectAdapter(ColorizeEffectUnit source)
        {
            _source = source;

            BindingOperations.SetBinding(_effect, ColorizeEffect.LuminanceWeightProperty, new Binding(nameof(ColorizeEffectUnit.LuminanceWeight)) { Source = _source });
            BindingOperations.SetBinding(_effect, ColorizeEffect.LutTextureProperty, new Binding(nameof(ColorizeEffectUnit.LutBrush)) { Source = _source });

            _source.RaisePropertyChangedAll();
        }
    }
}