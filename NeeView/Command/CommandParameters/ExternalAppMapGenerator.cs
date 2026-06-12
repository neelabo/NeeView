using NeeLaboratory.Collection;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NeeView
{
    public class ExternalAppMapGenerator : IKeyValueListGenerator<int>
    {
        public KeyValuePairList<int, string> CreateMap()
        {
            return Config.Current.System.ExternalAppCollection
                .Select((e, index) => KeyValuePair.Create(index + 1, $"{index + 1} {e.DisplayName}"))
                .Prepend(KeyValuePair.Create(0, TextResources.GetString("Word.SelectionMenu")))
                .ToKeyValuePairList();
        }
    }
}
