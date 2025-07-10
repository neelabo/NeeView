using NeeView.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    public record class QuickAccessFolderNodeSource : ISetParameter
    {
        private readonly TreeListNode<QuickAccessEntry> _node;

        public QuickAccessFolderNodeSource(TreeListNode<QuickAccessEntry> node)
        {
            Debug.Assert(node.Value is QuickAccessFolder);
            _node = node;
        }

        [WordNodeMember(AltName = "Word.Name")]
        public string Name
        {
            get { return _node.Name; }
            set { AppDispatcher.Invoke(() => _node.Value.Name = value); }
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
