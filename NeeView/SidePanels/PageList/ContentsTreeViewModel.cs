using NeeLaboratory.ComponentModel;
using NeeLaboratory.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ContentsTreeViewModel : BindableBase
    {
        public ContentsTreeViewModel()
        {
            BookOperation.Current.BookChanged += BookOperation_BookChanged;
        }


        public List<ContentsPageNode>? Items
        {
            get
            {
                if (field is null)
                {
                    OnUpdateItems();
                }
                return field;
            }
            private set { SetProperty(ref field, value); }
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

        public void SelectIndex(ContentsPageNode node)
        {
            if (node.Page is null) return;
            BookOperation.Current.JumpPage(this, node.Page);
        }
    }
}
