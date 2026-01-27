using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class SelectedContainer : INotifyPropertyChanged, IDisposable
    {
        private readonly PageFrameContainerCollection _containers;
        private readonly Func<LinkedListNode<PageFrameContainer>> _selectFunc;
        private LinkedListNode<PageFrameContainer> _node;
        private bool _disposedValue;


        public SelectedContainer(PageFrameContainerCollection containers, Func<LinkedListNode<PageFrameContainer>> selectFunc)
        {
            _containers = containers;
            _containers.CollectionChanged += Containers_CollectionChanged;

            _selectFunc = selectFunc;

            _node = _containers.CollectNode().First();
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;


        public LinkedListNode<PageFrameContainer> Node => _node;

        public Page? Page => (_node.Value.Content as PageFrameContent)?.PageFrame.Elements[0].Page;

        public PageRange PageRange => _node.Value.FrameRange;

        public bool IsValid => _node.Value.Content is PageFrameContent;


        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Detach();
                    _containers.CollectionChanging -= Containers_CollectionChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Containers_CollectionChanged(object? sender, PageFrameContainerCollectionChangedEventArgs e)
        {
            // 選択ノードが削除されたときの再選出
            if (e.Node == _node && e.Action == PageFrameContainerCollectionChangedEventAction.Remove)
            {
                SetAuto();
            }
        }

        public void Set(LinkedListNode<PageFrameContainer> node, bool force)
        {
            if (_disposedValue) return;

            if (!force && _node == node) return;
            Debug.Assert(node?.Value.Content is PageFrameContent);

            Detach();
            _node = node;
            RaisePropertyChanged(nameof(Node));
            RaisePropertyChanged(nameof(Page));
            RaisePropertyChanged(nameof(PageRange));
            Attach();
        }

        public void SetAuto()
        {
            if (_disposedValue) return;
            if (_selectFunc is null) return;

            Set(_selectFunc.Invoke(), false);
        }


        private void Attach()
        {
            _node.Value.ContentChanged += Container_ContentChanged;
            _node.Value.ViewContentChanged += Container_ViewContentChanged;
            _node.Value.Activity.IsSelected = true;

            if (_node.Value.Content is PageFrameContent content)
            {
                ViewContentChanged?.Invoke(this, new FrameViewContentChangedEventArgs(ViewContentChangedAction.Selection, content, content.ViewContents, content.ViewContentsDirection));
            }
        }


        private void Detach()
        {
            if (_node is null) return;

            _node.Value.ContentChanged -= Container_ContentChanged;
            _node.Value.ViewContentChanged -= Container_ViewContentChanged;
            _node.Value.Activity.IsSelected = false;
        }

        private void Container_ContentChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Page));
            RaisePropertyChanged(nameof(PageRange));
        }

        private void Container_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            ViewContentChanged?.Invoke(this, e);
        }
    }

}
