using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace NeeView
{
    public record class QuickAccessFolderNodeAccessor : NodeAccessor
    {
        private readonly QuickAccessFolderNode _node;
        private readonly QuickAccessFolderNodeSource _value;

        public QuickAccessFolderNodeAccessor(FolderTreeModel model, QuickAccessFolderNode node) : base(model, node)
        {
            _node = node;
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
