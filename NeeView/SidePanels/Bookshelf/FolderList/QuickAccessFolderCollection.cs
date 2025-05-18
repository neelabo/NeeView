using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public QuickAccessFolderCollection(bool isOverlayEnabled) : base(new QueryPath(QueryScheme.QuickAccess, null), isOverlayEnabled)
        {
        }


        public override FolderOrderClass FolderOrderClass => FolderOrderClass.None;


        public override async ValueTask InitializeItemsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var items = QuickAccessCollection.Current.Items.Select(e => CreateFolderItem(Place, e));

            this.Items = new ObservableCollection<FolderItem>(items);

            //TODO:
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;

            await Task.CompletedTask;
        }


        private void QuickAccessCollection_CollectionChanged(object? sender, QuickAccessCollectionChangeEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.Action)
            {
                case QuickAccessCollectionChangeAction.Add:
                    {
                        var target = e.Element as QuickAccess ?? throw new InvalidOperationException("e.Element must be QuickAccess");
                        var item = Items.FirstOrDefault(i => target == i.Source);
                        if (item == null)
                        {
                            item = CreateFolderItem(Place, target);
                            var index = QuickAccessCollection.Current.Items.IndexOf(target);
                            InsertItem(item, index);
                        }
                    }
                    break;

                case QuickAccessCollectionChangeAction.Remove:
                    {
                        var target = e.Element as QuickAccess ?? throw new InvalidOperationException("e.Element must be QuickAccess");
                        var item = Items.FirstOrDefault(i => target == i.Source);
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                    break;

                case QuickAccessCollectionChangeAction.Move:
                    break;

                case QuickAccessCollectionChangeAction.Refresh:
                    BookshelfFolderList.Current.RequestPlace(new QueryPath(QueryScheme.QuickAccess, null), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
                    break;

                case QuickAccessCollectionChangeAction.Rename:
                case QuickAccessCollectionChangeAction.PathChanged:
                    // nop.
                    break;
            }
        }

        private FolderItem CreateFolderItem(QueryPath parent, QuickAccess quickAccess)
        {
            return new ConstFolderItem(new FolderThumbnail(), _isOverlayEnabled)
            {
                Source = quickAccess,
                Type = FolderItemType.Directory,
                Place = parent,
                Name = quickAccess.Name,
                TargetPath = new QueryPath(quickAccess.Path),
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
                    QuickAccessCollection.Current.CollectionChanged -= QuickAccessCollection_CollectionChanged;
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }

}
