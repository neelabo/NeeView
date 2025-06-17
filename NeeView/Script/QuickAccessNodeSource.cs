using NeeView.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    public record class QuickAccessNodeSource : ISetParameter
    {
        private readonly TreeListNode<QuickAccessEntry> _node;

        public QuickAccessNodeSource(TreeListNode<QuickAccessEntry> node)
        {
            Debug.Assert(node.Value is QuickAccess);
            _node = node;
        }

        [WordNodeMember]
        public string Path
        {
            get { return _node.Path; }
            set { AppDispatcher.Invoke(() => _node.Value.Path = value); }
        }

        [WordNodeMember(AltName = "@Word.Name")]
        public string Name
        {
            get { return _node.Name; }
            set { AppDispatcher.Invoke(() => _node.Value.Name = value); }
        }


        public void SetParameter(IDictionary<string, object?>? obj )
        {
            if (obj == null) return;
            var name = JavaScriptObjectTools.GetValue<string>(obj, nameof(Name));
            if (name is not null)
            {
                Name = name;
            }
            var path = JavaScriptObjectTools.GetValue<string>(obj, nameof(Path));
            if (path is not null)
            {
                Path = path;
            }
        }
    }

}
