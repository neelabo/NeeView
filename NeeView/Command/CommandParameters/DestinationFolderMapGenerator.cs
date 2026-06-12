using NeeLaboratory.Collection;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class DestinationFolderMapGenerator : IKeyValueListGenerator<int>
    {
        public KeyValuePairList<int, string> CreateMap()
        {
            return Config.Current.System.DestinationFolderCollection
                .Select((e, index) => KeyValuePair.Create(index + 1, $"{index + 1} {e.Name}"))
                .Prepend(KeyValuePair.Create(0, TextResources.GetString("Word.SelectionMenu")))
                .ToKeyValuePairList();
        }
    }
}
