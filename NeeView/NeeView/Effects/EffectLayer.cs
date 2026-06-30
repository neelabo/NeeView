using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeView.Effects;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace NeeView
{
    public partial class EffectLayer : ObservableObject
    {
        [ObservableProperty]
        [DefaultEquality]
        public partial bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        [ObjectMergeIgnore]
        public EffectType EffectType
        {
            get => Effect.ToEffectType();
            set
            {
                if (Effect.ToEffectType() != value)
                {
                    var type = value.ToType();
                    AddCache();
                    Effect = CreateParameter(type);
                    IsEnabled = true;
                }
            }
        }

        [DefaultEquality]
        public EffectUnit? Effect
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EffectType));
                }
            }
        }

        public void AddCache()
        {
            Config.Current.ImageEffectCache.Add(Effect);
        }

        public void Reset()
        {
            if (Effect is null) return;

            var def = EffectUnit.CreateInstance(Effect.GetType());
            ObjectMerge.Merge(Effect, def);
        }

        private EffectUnit? CreateParameter(Type? type)
        {
            if (type is null) return null;

            if (Config.Current.ImageEffect.Layers.Where(e => e != this).Any(e => e.Effect?.GetType() == type))
            {
                return EffectUnit.CreateInstance(type);
            }
            else
            {
                return Config.Current.ImageEffectCache.Get(type);
            }
        }
    }
}