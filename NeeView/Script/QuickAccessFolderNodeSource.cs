using System.Collections.Generic;

namespace NeeView
{
    public record class QuickAccessFolderNodeSource : ISetParameter
    {
        private readonly QuickAccessFolderNode _node;

        public QuickAccessFolderNodeSource(QuickAccessFolderNode node)
        {
            _node = node;
        }

        [WordNodeMember(AltName = "@Word.Name")]
        public string Name
        {
            get { return _node.Name; }
            set { AppDispatcher.Invoke(() => _node.Rename(value)); }
        }

        public void SetParameter(IDictionary<string, object?>? obj)
        {
            if (obj == null) return;
            var name = JavaScriptObjectTools.GetValue<string>(obj, nameof(Name));
            if (name is not null)
            {
                Name = name;
            }
        }
    }

}
