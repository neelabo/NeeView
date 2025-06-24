using System.Windows.Controls;

namespace NeeView
{
    public class ThumbnailListItemCommandResource : PageCommandResource<Page>
    {
        public ThumbnailListItemCommandResource(IToolTipService? toolTipService) : base(toolTipService)
        {
        }

        protected override ListBoxItemRenamer<Page>? CreateListBoxItemRenamer(ListBox listBox, Page item, IToolTipService? toolTipService)
        {
            var renamer = new PageListItemRenamer(listBox, toolTipService, false);
            renamer.SelectedItemChanged += (s, e) => BookOperation.Current.JumpPage(this, item);
            renamer.ArchiveChanged += (s, e) => BookOperation.Current.BookControl.ReLoad();
            return renamer;
        }
    }
}
