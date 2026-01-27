using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace NeeView
{
    /// <summary>
    /// TreeViewNode基底.
    /// Childrenの遅延生成に対応
    /// </summary>
    public abstract class FolderTreeNodeDelayBase : FolderTreeNodeBase
    {
        private static readonly ObservableCollection<FolderTreeNodeBase> _dummyChildren = new() { new DummyNode() };


        public FolderTreeNodeDelayBase()
        {
        }


        /// <summary>Expandのタイミングまで子供の生成を遅らせる</summary>
        public bool IsDelayCreation { get; set; }

        public override bool IsExpanded
        {
            get { return base.IsExpanded; }
            set
            {
                base.IsExpanded = value;
                if (_children == null && base.IsExpanded == true)
                {
                    CreateChildren(true);
                }
            }
        }

        [NotNull]
        public override ObservableCollection<FolderTreeNodeBase>? Children
        {
            get
            {
                if (_children == null && !IsDelayCreation)
                {
                    CreateChildren(false);
                }
                return _children ?? _dummyChildren;
            }
            set
            {
                base.Children = value;
            }
        }

        /// <param name="isForce">falseの場合、生成失敗ののときはExpandされるまで遅延させる</param>
        public abstract void CreateChildren(bool isForce);

        protected override void RealizeChildren()
        {
            CreateChildren(true);
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }
    }


    /// <summary>
    /// ダミーノード
    /// </summary>
    public class DummyNode : FolderTreeNodeBase
    {
        public override string Name { get => ""; set { } }
        public override string DisplayName { get => ""; set { } }

        public override IImageSourceCollection? Icon => null;
    }
}

