using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;

namespace NeeView
{
    public record class QuickAccessFolderNodeAccessor : NodeAccessor
    {
        private readonly TreeListNode<QuickAccessEntry> _node;
        private readonly QuickAccessFolderNodeSource _value;

        public QuickAccessFolderNodeAccessor(FolderTreeModel model, ITreeViewNode node) : base(model, node)
        {
            _node = (TreeListNode<QuickAccessEntry>)node;
            Debug.Assert(_node.Value is QuickAccessFolder);
            _value = new QuickAccessFolderNodeSource(_node);
        }


        [WordNodeMember(AltName = "@QuickAccessFolderNodeSource")]
        [ReturnType(typeof(QuickAccessFolderNodeSource))]
        public override object? Value => _value;


        protected override string GetName() => _value.Name;

        [WordNodeMember]
        [ReturnType(typeof(QuickAccessNodeAccessor), typeof(QuickAccessFolderNodeAccessor))]
        public override NodeAccessor Add(IDictionary<string, object?>? parameter)
        {
            return base.Add(parameter);
        }

        [WordNodeMember(AltName = nameof(Add))]
        [ReturnType(typeof(QuickAccessNodeAccessor), typeof(QuickAccessFolderNodeAccessor))]
        public override NodeAccessor Insert(int index, IDictionary<string, object?>? parameter)
        {
            return base.Insert(index, parameter);
        }

        [WordNodeMember(AltClassType = typeof(NodeAccessor))]
        public override void MoveTo(int newIndex)
        {
            base.MoveTo(newIndex);
        }

        internal static WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, typeof(QuickAccessFolderNodeAccessor));
            return node;
        }
    }
}
