using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeLaboratory.Collection;
using NeeView.Windows.Property;
using System.Linq;

namespace NeeView
{
    [Equatable]
    public partial class SetEffectProfileCommandParameter : CommandParameter
    {
        [PropertyIntegers(MapGenerator = typeof(EffectProfileMapGenerator))]
        [ObservableProperty]
        public partial int Id { get; set; }
    }


    public class EffectProfileMapGenerator : IKeyValueListGenerator<int>
    {
        public KeyValuePairList<int, string> CreateMap()
        {
            return Config.Current.EffectProfiles.Profiles.OrderBy(e => e.Id != 0).ThenBy(e => e.Name).ToKeyValuePairList(e => e.Id, e => e.DisplayName);
        }
    }
}
