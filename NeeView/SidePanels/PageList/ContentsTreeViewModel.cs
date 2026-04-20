using NeeLaboratory.ComponentModel;
using NeeLaboratory.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ContentsTreeViewModel : BindableBase
    {
        private PageListConfig _pageListConfig;


        public ContentsTreeViewModel()
        {
            _pageListConfig = Config.Current.PageList;

            _pageListConfig.SubscribePropertyChanged(nameof(_pageListConfig.IsFolderTreeVisible),
                (s, e) => OnUpdateItems());

            BookOperation.Current.BookChanged += BookOperation_BookChanged;
        }

        List<ContentsPageNode>? _items;

        public List<ContentsPageNode>? Items
        {
            get
            {
                if (_items is null)
                {
                    OnUpdateItems();
                }
                return _items;
            }
            private set { SetProperty(ref _items, value); }
        }

        public bool IsVisible
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnUpdateItems();
                }
            }
        }


        public void OnUpdateItems()
        {
            UpdateItemsAsync(CancellationToken.None).FireAndForget();
        }

        private async Task UpdateItemsAsync(CancellationToken token)
        {
            var node = await GetItemsAsync(token);
            Items = node?.Children is not null ? [node] : [];
        }

        private async Task<ContentsPageNode?> GetItemsAsync(CancellationToken token)
        {
            if (!IsVisible)
            {
                return null;
            }

            var book = BookOperation.Current.Book;
            if (book is null)
            {
                return null;
            }

            return await book.TableOfContents.GetContentsTreeAsync(token);
        }


        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            OnUpdateItems();
        }

        internal void SelectIndex(ContentsPageNode node)
        {
            if (node.Page is null) return;
            BookOperation.Current.JumpPage(this, node.Page);
        }

        internal void SelectFirstItem()
        {
            _items?.FirstOrDefault()?.IsSelected = true;
        }
    }
}
