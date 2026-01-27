using NeeView.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// クイックアクセスのフォルダーコレクション
    /// (未使用)
    /// </summary>
    public class QuickAccessFolderCollection : FolderCollection, IDisposable
    {
        private readonly TreeListNode<QuickAccessEntry>? _node;


        public QuickAccessFolderCollection(QueryPath path, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
            if (path.Scheme != QueryScheme.QuickAccess) throw new ArgumentException("Not a QuickAccess scheme path.", nameof(path));

            _node = QuickAccessCollection.Current.FindNode(Place);
        }


        public override FolderOrderClass FolderOrderClass => FolderOrderClass.None;


        public override async ValueTask InitializeItemsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_node is not null)
            {
                this.Items = new ObservableCollection<FolderItem>(_node.Select(e => CreateFolderItem(Place, e)));
                _node.CollectionChanged += Children_CollectionChanged;
            }

            await Task.CompletedTask;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems is null) return;
                    foreach (var target in e.NewItems.Cast<TreeListNode<QuickAccessEntry>>().Reverse())
                    {
                        var item = Items.FirstOrDefault(i => target == i.Source);
                        if (item == null)
                        {
                            item = CreateFolderItem(Place, target);
                            InsertItem(item, e.NewStartingIndex);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is null) return;
                    foreach (var target in e.OldItems.Cast<TreeListNode<QuickAccessEntry>>().Reverse())
                    {
                        var item = Items.FirstOrDefault(i => target == i.Source);
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    // nop.
                    break;

                case NotifyCollectionChangedAction.Reset:
                    BookshelfFolderList.Current.RequestPlace(Place, null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
                    break;
            }
        }

        private FolderItem CreateFolderItem(QueryPath parent, TreeListNode<QuickAccessEntry> quickAccess)
        {
            return new ConstFolderItem(new FolderThumbnail(), _isOverlayEnabled)
            {
                Source = quickAccess,
                Type = FolderItemType.Directory,
                Place = parent,
                Name = quickAccess.Value.Name,
                TargetPath = new QueryPath(quickAccess.Value.Path),
                Length = -1,
                Attributes = FolderItemAttribute.Directory | FolderItemAttribute.System | FolderItemAttribute.QuickAccess,
                IsReady = true
            };
        }

        private void InsertItem(FolderItem item, int index)
        {
            if (item == null) return;

            if (this.Items.Count == 1 && this.Items.First().Type == FolderItemType.Empty)
            {
                this.Items.RemoveAt(0);
                this.Items.Add(item);
            }
            else if (index >= 0)
            {
                this.Items.Insert(index, item);
            }
            else
            {
                this.Items.Add(item);
            }
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_node is not null)
                    {
                        _node.CollectionChanged -= Children_CollectionChanged;
                    }
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }

}
