using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ListBoxItem用名前変更機能
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public abstract class ListBoxItemRenamer<TItem>
        where TItem : class, IRenameable
    {
        private readonly ListBox _listBox;
        private readonly IToolTipService? _toolTipService;
        private readonly bool _selectItem;


        public ListBoxItemRenamer(ListBox listBox, IToolTipService? toolTipService) : this(listBox, toolTipService, true)
        {
        }

        public ListBoxItemRenamer(ListBox listBox, IToolTipService? toolTipService, bool selectItem)
        {
            _listBox = listBox;
            _toolTipService = toolTipService;
            _selectItem = selectItem;
        }


        public event EventHandler? SelectedItemChanged;


        protected virtual void BeginRename()
        {
            if (_toolTipService is null) return;
            _toolTipService.IsToolTipEnabled = false;
        }

        protected virtual void EndRename()
        {
            if (_toolTipService is null) return;
            _toolTipService.IsToolTipEnabled = true;
        }

        public async ValueTask RenameAsync(TItem item)
        {
            await RenameLoopAsync(_listBox, item);
        }

        private async ValueTask RenameLoopAsync(ListBox listBox, TItem? item)
        {
            BeginRename();
            try
            {
                while (item != null && item.CanRename())
                {
                    if (_selectItem)
                    {
                        SelectItem(listBox, item);
                    }
                    var control = CreateRenameControl(listBox, item);
                    var result = await control.ShowAsync();
                    item = ListBoxItemRenamer<TItem>.GetNextItem(listBox, item, result.MoveRename);
                }
            }
            finally
            {
                EndRename();
            }
        }

        protected virtual RenameControl CreateRenameControl(ListBox listBox, TItem item)
        {
            return new ListBoxItemRenameControl<TItem>(listBox, item);
        }


        private static TItem? GetNextItem(ListBox listBox, TItem item, int delta)
        {
            if (delta == 0) return null;
            Debug.Assert(delta == -1 || delta == +1);

            listBox.SelectedItem = item;
            if (listBox.SelectedItem != item) return null;
            listBox.SelectedIndex = (listBox.SelectedIndex + listBox.Items.Count + delta) % listBox.Items.Count;
            return listBox.SelectedItem as TItem;
        }

        private void SelectItem(ListBox listBox, TItem item)
        {
            listBox.SelectedItem = item;
            if (listBox.SelectedItem != item) return;
            listBox.ScrollIntoView(listBox.SelectedItem);
            listBox.UpdateLayout();
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// TreeViewItem用名前変更機能
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public abstract class TreeViewItemRenamer<TItem>
        where TItem : class, IRenameable
    {
        private readonly TreeView _treeView;
        private readonly IToolTipService? _toolTipService;


        public TreeViewItemRenamer(TreeView treeView, IToolTipService? toolTipService)
        {
            _treeView = treeView;
            _toolTipService = toolTipService;
        }


        public async ValueTask RenameAsync(TItem item)
        {
            BeginRename();
            try
            {
                var control = CreateRenameControl(_treeView, item);
                await control.ShowAsync();
            }
            finally
            {
                EndRename();
            }
        }

        protected virtual void BeginRename()
        {
            if (_toolTipService is null) return;
            _toolTipService.IsToolTipEnabled = false;
        }

        protected virtual void EndRename()
        {
            if (_toolTipService is null) return;
            _toolTipService.IsToolTipEnabled = true;
        }

        protected virtual RenameControl CreateRenameControl(TreeView treeView, TItem item)
        {
            return new TreeViewRenameControl<TItem>(treeView, item);
        }
    }


}
