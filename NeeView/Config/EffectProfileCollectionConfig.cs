using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView
{
    [DiffJsonDefault(typeof(Defaultable))]
    public partial class EffectProfileCollectionConfig : ObservableObject
    {
        [PropertyMapIgnore]
        public int IdCounter { get; set; }

        [PropertyMapIgnore]
        public ObservableCollection<EffectProfile> Profiles { get; set; } = new() { new() };


        public class Defaultable : IDefaultable
        {
            public bool IsDefault(object? obj)
            {
                if (obj is not EffectProfileCollectionConfig x) throw new InvalidOperationException($"Not the target type: {obj?.GetType().FullName}");

                var def = new EffectProfileCollectionConfig();

                return x.IdCounter == def.IdCounter
                    && x.Profiles.SequenceEqual(def.Profiles, new EffectProfileEqualityComparer());
            }
        }
    }

}
