using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NeeLaboratory.ComponentModel;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;

namespace NeeView
{
    public class SidePanelDropAcceptor : BindableBase
    {
        /// <summary>
        /// ドロップ受け入れ先コントロール.
        /// ドロップイベント受信コントロールとは異なるために用意した.
        /// </summary>
        private readonly ItemsControl _itemsControl;
        private readonly LayoutDockPanelContent _dock;
        private readonly ItemsControlDropAssist _dropAssist;
        private DropAcceptDescription _description;


        public SidePanelDropAcceptor(ItemsControl itemsControl, LayoutDockPanelContent dock)
        {
            _itemsControl = itemsControl;
            _dock = dock;

            _dropAssist = new ItemsControlDropAssist(_itemsControl);

            _description = new DropAcceptDescription();
            _description.DragEnter += Description_DragEnter;
            _description.DragLeave += Description_DragLeave;
            _description.DragOver += Description_DragOver;
            _description.DragDrop += Description_DragDrop;
        }


        /// <summary>
        /// ドロップイベント
        /// </summary>
        public EventHandler<LayoutPanelDroppedEventArgs>? PanelDropped;


        /// <summary>
        /// ドロップ処理設定プロパティ
        /// </summary>
        public DropAcceptDescription Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }


        private void Description_DragEnter(object? sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(LayoutPanel)))
            {
                return;
            }

            _dropAssist.OnDragEnter(sender, e);
        }

        private void Description_DragLeave(object? sender, DragEventArgs e)
        {
            _dropAssist.OnDragLeave(sender, e);
        }

        private void Description_DragOver(object? sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(LayoutPanel)))
            {
                return;
            }

            _dropAssist.OnDragOver(sender, e);

            if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void Description_DragDrop(object? sender, DragEventArgs e)
        {
            var result = _dropAssist.OnDrop(sender, e);

            try
            {
                var panel = e.Data.GetData<LayoutPanel>();
                if (panel == null) return;

                var index = GetItemInsertIndex(result, panel);
                PanelDrop(index, panel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Drop failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ドロップ実行
        /// </summary>
        /// <param name="index">挿入位置</param>
        /// <param name="panel">挿入するパネル</param>
        private void PanelDrop(int index, LayoutPanel panel)
        {
            _dock.MovePanel(index, panel);

            // NOTE: 未使用？
            PanelDropped?.Invoke(this, new LayoutPanelDroppedEventArgs(panel, index));
        }

        /// <summary>
        /// リストの挿入位置を求める
        /// </summary>
        private int GetItemInsertIndex(ItemsControlDropTarget args, LayoutPanel panel)
        {
            if (_itemsControl == null) return -1;

            var src = GetItemIndex(panel);
            var dst = GetItemIndex(args.Item);

            if (dst < 0)
            {
                dst = Math.Max(_itemsControl.Items.Count, 0);
            }

            if (src < 0 || dst < src)
            {
                return dst + (args.Delta > 0 ? 1 : 0);
            }
            else
            {
                return dst;
            }
        }

        /// <summary>
        /// アイテムのインデックスを取得する
        /// </summary>
        /// <param name="control">項目コンテナ</param>
        /// <returns></returns>
        private int GetItemIndex(ContentPresenter? control)
        {
            if (_itemsControl == null) return -1;

            var count = _itemsControl.Items.Count;
            for (int index = 0; index < count; ++index)
            {
                var item = _itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as ContentPresenter;
                if (item == control)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// アイテムのインデックスを取得する
        /// </summary>
        /// <param name="panel">パネル</param>
        /// <returns></returns>
        private int GetItemIndex(LayoutPanel panel)
        {
            if (_itemsControl == null) return -1;

            var count = _itemsControl.Items.Count;
            for (int index = 0; index < count; ++index)
            {
                var item = _itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as ContentPresenter;
                if (item?.Content == panel)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
