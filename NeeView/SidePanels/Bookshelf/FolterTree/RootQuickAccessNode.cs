using NeeLaboratory.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Media;

namespace NeeView
{

    public class RootQuickAccessNode : FolderTreeNodeBase
    {
        public RootQuickAccessNode()
        {
            // NOTE: need call Initialize()

            Icon = new SingleImageSourceCollection(ResourceTools.GetElementResource<ImageSource>(MainWindow.Current, "ic_lightning"));
        }


        public override string Name { get => QueryScheme.QuickAccess.ToSchemeString(); set { } }

        public override string DisplayName { get => Properties.TextResources.GetString("Word.QuickAccess"); set { } }

        public override IImageSourceCollection Icon { get; } 


        [NotNull]
        public override ObservableCollection<FolderTreeNodeBase>? Children
        {
            get { return _children = _children ?? new ObservableCollection<FolderTreeNodeBase>(QuickAccessCollection.Current.Items.Select(e => new QuickAccessNode(e, this))); }
            set { SetProperty(ref _children, value); }
        }


        public void Initialize(FolderTreeNodeBase parent)
        {
            Parent = parent;

            RefreshChildren();
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;
        }

        private void QuickAccessCollection_CollectionChanged(object? sender, QuickAccessCollectionChangeEventArgs e)
        {

            switch (e.Action)
            {
                case QuickAccessCollectionChangeAction.Refresh:
                    RefreshChildren(isExpanded: true);
                    break;

                case QuickAccessCollectionChangeAction.Add:
                    var addItem = e.Element as QuickAccess ?? throw new InvalidOperationException();
                    var index = QuickAccessCollection.Current.Items.IndexOf(addItem);
                    var node = new QuickAccessNode(addItem, null) { IsSelected = true }; // NOTE: 選択項目として追加
                    Insert(index, node);
                    break;

                case QuickAccessCollectionChangeAction.Remove:
                    var removeItem = e.Element as QuickAccess ?? throw new InvalidOperationException();
                    Remove(removeItem);
                    break;

                case QuickAccessCollectionChangeAction.Move:
                    this.Children.Move(e.OldIndex, e.NewIndex);
                    AssertChildrenOrder();
                    break;

                case QuickAccessCollectionChangeAction.Rename:
                case QuickAccessCollectionChangeAction.PathChanged:
                    // nop
                    break;
            }
        }

        [Conditional("DEBUG")]
        private void AssertChildrenOrder()
        {
            Debug.Assert(this.Children.Count == QuickAccessCollection.Current.Items.Count);
            for (int i = 0; i < this.Children.Count; i++)
            {
                var quickAccess = this.Children[i].Source as QuickAccess;
                Debug.Assert(quickAccess is not null);
                Debug.Assert(QuickAccessCollection.Current.Items[i] == quickAccess);
            }
        }
    }
}
