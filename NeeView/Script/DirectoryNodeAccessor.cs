using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    [WordNodeMember]
    public record class DirectoryNodeAccessor : NodeAccessor
    {
        private readonly DirectoryNode _node;
        private readonly DirectoryNodeSource _value;

        public DirectoryNodeAccessor(FolderTreeModel model, DirectoryNode node) : base(model, node)
        {
            _node = node;
            _value = new DirectoryNodeSource(_node);
        }


        [WordNodeMember(AltName = "@DirectoryNodeSource")]
        [ReturnType(typeof(DirectoryNodeSource))]
        public override object? Value => _value;


        [WordNodeMember(AltClassType = typeof(NodeAccessor))]
        public override NodeAccessor[]? Children
        {
            get
            {
                if (_node.ChildrenRaw is null)
                {
                    _node.CreateChildren(false);
                }
                return GetChildren() ?? [];
            }
        }


        protected override string GetName() => _value.Name;

        [WordNodeMember(IsEnabled = false)]
        public override NodeAccessor Add(IDictionary<string, object?>? parameter)
        {
            throw new NotSupportedException();
        }

        [WordNodeMember(IsEnabled = false)]
        public override NodeAccessor Insert(int index, IDictionary<string, object?>? parameter)
        {
            throw new NotSupportedException();
        }

        [WordNodeMember(IsEnabled = false)]
        public override bool Remove()
        {
            throw new NotSupportedException();
        }

    }
}
