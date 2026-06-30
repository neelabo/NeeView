using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace NeeView.Effects
{
    [Equatable(IgnoreInheritedMembers = true)]
    [JsonConverter(typeof(JsonEffectUnitConverter))]
    public partial class EffectUnit : ObservableObject
    {
        public EffectUnit()
        {
        }

        public EffectUnit(EffectSampleType sampleType)
        {
            SampleType = sampleType;
        }

        [PropertyMapIgnore]
        public EffectSampleType SampleType { get; }

        public void RaisePropertyChangedAll()
        {
            OnPropertyChanged("");
        }

        public static EffectUnit CreateInstance(Type type)
        {
            Debug.Assert(type.IsAssignableTo(typeof(EffectUnit)));
            return (EffectUnit)Activator.CreateInstance(type)!;
        }
    }


    public sealed class JsonEffectUnitConverter : PolymorphicDiffJsonConverter<EffectUnit>
    {
        private const string _typeNamePostfix = "EffectUnit";

        private readonly static JsonDerivedTypeData[] _knownTypes =
        [
            new(typeof(LevelEffectUnit)),
            new(typeof(HsvEffectUnit)),
            new(typeof(ColorSelectEffectUnit)),
            new(typeof(BlurEffectUnit)),
            new(typeof(BloomEffectUnit)),
            new(typeof(MonochromeEffectUnit)),
            new(typeof(ColorToneEffectUnit)),
            new(typeof(ColorizeEffectUnit)),
            new(typeof(SharpenEffectUnit)),
            new(typeof(EmbossedEffectUnit)),
            new(typeof(PixelateEffectUnit)),
            new(typeof(MagnifyEffectUnit)),
            new(typeof(RippleEffectUnit)),
            new(typeof(SwirlEffectUnit)),
        ];

        static JsonEffectUnitConverter()
        {
            Initialize(_knownTypes, ToTypeName);
        }

        public JsonEffectUnitConverter()
        {
            IsTrimEnabled = AppSettings.Current.TrimSaveData;
        }

        private static string ToTypeName(JsonDerivedTypeData data)
        {
            return data.TypeDiscriminator ?? ClassTools.CreateName(data.DerivedType.Name, _typeNamePostfix);
        }
    }

}
