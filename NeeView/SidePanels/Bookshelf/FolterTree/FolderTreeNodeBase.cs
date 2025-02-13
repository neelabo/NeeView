using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// TreeViewNode基底.
    /// </summary>
    public abstract class FolderTreeNodeBase : BindableBase, IRenameable, IDisposable, ITreeViewItemData
    {
        private bool _isDisposed;
        private bool _isSelected;
        private bool _isExpanded;
        protected ObservableCollection<FolderTreeNodeBase>? _children;


        public FolderTreeNodeBase()
        {
        }


        public bool IsDisposed
        {
            get { return _isDisposed || _parent?.IsDisposed == true; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value && _parent is not null)
                {
                    _parent.IsExpanded = value;
                }

                SetProperty(ref _isExpanded, value);
            }
        }

        public virtual ObservableCollection<FolderTreeNodeBase>? Children
        {
            get { return _children; }
            set
            {
                if (_children != value)
                {
                    if (_children is not null)
                    {
                        foreach (var child in _children)
                        {
                            child.Dispose();
                        }
                    }
                    SetProperty(ref _children, value);
                }
            }
        }

        public ObservableCollection<FolderTreeNodeBase>? ChildrenRaw => _children;

        public abstract string Name { get; set; }

        public abstract string DispName { get; set; }

        public abstract IImageSourceCollection? Icon { get; }

        public object? Source { get; protected set; }

        private FolderTreeNodeBase? _parent;
        public FolderTreeNodeBase? Parent
        {
            get { return _parent; }
            set
            {
                if (SetProperty(ref _parent, value))
                {
                    OnParentChanged(this, EventArgs.Empty);
                }
            }
        }

        public FolderTreeNodeBase? Previous
        {
            get
            {
                if (Parent?._children != null)
                {
                    var index = Parent._children.IndexOf(this);
                    return Parent._children.ElementAtOrDefault(index - 1);
                }

                return null;
            }
        }

        public FolderTreeNodeBase? Next
        {
            get
            {
                if (Parent?._children != null)
                {
                    var index = Parent._children.IndexOf(this);
                    return Parent._children.ElementAtOrDefault(index + 1);
                }

                return null;
            }
        }

        /// <summary>
        /// 階層コレクション
        /// </summary>
        public IEnumerable<FolderTreeNodeBase> Hierarchy
        {
            get
            {
                return HierarchyReverse.Reverse();
            }
        }

        public IEnumerable<FolderTreeNodeBase> HierarchyReverse
        {
            get
            {
                yield return this;
                for (var parent = Parent; parent != null; parent = parent.Parent)
                {
                    yield return parent;
                }
            }
        }

        IEnumerable<ITreeViewItemData>? ITreeViewItemData.Children => Children;

        protected virtual void OnParentChanged(object sender, EventArgs e)
        {
        }

        public virtual void RefreshAllProperties()
        {
            RaisePropertyChanged("");
        }

        public virtual void RefreshIcon()
        {
            RaisePropertyChanged(nameof(Icon));
        }

        public void RefreshChildren(bool isExpanded = false)
        {
            IsExpanded = isExpanded;

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.Dispose();
                }
            }
            _children = null;

            RaisePropertyChanged(nameof(Children));
        }

        /// <summary>
        /// 指定パスの<see cref="FolderTreeNodeBase"/>を取得
        /// </summary>
        /// <param name="path">指定パス</param>
        /// <param name="createChildren">まだ生成されていなければChildrenを生成する</param>
        /// <param name="asFarAsPossible">指定パスが存在しない場合、存在する上位フォルダーを返す</param>
        /// <returns></returns>
        public FolderTreeNodeBase? GetFolderTreeNode(string? path, bool createChildren, bool asFarAsPossible)
        {
            if (path == null) return null;

            var pathTokens = path.Trim(LoosePath.Separators).Split(LoosePath.Separators);
            return GetFolderTreeNode(pathTokens, createChildren, asFarAsPossible);
        }

        /// <summary>
        /// 指定パスのFolderTreeNodeを取得
        /// </summary>
        public FolderTreeNodeBase? GetFolderTreeNode(IEnumerable<string> pathTokens, bool createChildren, bool asFarAsPossible)
        {
            if (!pathTokens.Any())
            {
                return this;
            }

            var token = pathTokens.First();

            if (_children == null && createChildren)
            {
                RealizeChildren();
            }

            var child = Children?.FirstOrDefault(e => e.Name == token);
            if (child != null)
            {
                return child.GetFolderTreeNode(pathTokens.Skip(1), createChildren, asFarAsPossible);
            }

            return asFarAsPossible ? this : null;
        }

        /// <summary>
        /// Childrenの実体化
        /// <para>遅延生成される場合に override して使用する</para>
        /// </summary>
        protected virtual void RealizeChildren()
        {
        }

        /// <summary>
        /// 子の検索
        /// <para>Sourceのリファレンス比較のみなので、必要に応じて override をして使用する</para>
        /// </summary>
        public virtual FolderTreeNodeBase? FindChild(object? source)
        {
            return _children?.FirstOrDefault(e => e.Source == source);
        }

        public void Insert(int index, FolderTreeNodeBase newNode)
        {
            Debug.Assert(newNode != null);
            Debug.Assert(newNode.Parent == null);

            if (newNode == null || _children == null) return;

            var node = FindChild(newNode.Source);
            if (node == null)
            {
                newNode.Parent = this;
                _children.Insert(index, newNode);
            }
        }

        public void Add(FolderTreeNodeBase newNode)
        {
            Debug.Assert(newNode != null);
            Debug.Assert(newNode.Parent == null);

            if (newNode == null || _children == null) return;

            var node = FindChild(newNode.Source);
            if (node == null)
            {
                newNode.Parent = this;
                _children.Add(newNode);
                Sort(newNode);
            }
        }

        public void Remove(object source)
        {
            if (_children == null) return;

            var node = FindChild(source);
            if (node != null)
            {
                _children.Remove(node);
                node.Parent = null;
                node.IsSelected = false;
                node.IsExpanded = false;
                node.Dispose();
            }
        }

        public void Renamed(object source)
        {
            if (_children == null) return;

            var node = FindChild(source);
            if (node != null)
            {
                node.RaisePropertyChanged(nameof(Name));
                node.RaisePropertyChanged(nameof(DispName));
                Sort(node);
            }
        }

        /// <summary>
        /// 指定した子を適切な位置に並び替える
        /// </summary>
        protected virtual void Sort(FolderTreeNodeBase node)
        {
            if (_children == null) return;

            var oldIndex = _children.IndexOf(node);
            if (oldIndex < 0) return;

            var isSelected = node.IsSelected;

            for (int index = 0; index < _children.Count; ++index)
            {
                var directory = _children[index];
                if (directory == node) continue;

                if (NaturalSort.Compare(node.Name, directory.Name) < 0)
                {
                    if (oldIndex != index - 1)
                    {
                        _children.Move(oldIndex, index);
                    }
                    return;
                }
            }

            _children.Move(oldIndex, _children.Count - 1);

            // NOTE: FolderTreeModel.SelectedItemを変更したいところだが、ここからは参照できないのでフラグで通知する。
            node.IsSelected = isSelected;
        }

        protected IImageSourceCollection CreateIconFromResource(string key)
        {
            return new SingleImageSourceCollection(MainWindow.Current.Resources[key] as ImageSource
                ?? throw new InvalidOperationException($"Cannot found resource: {key}"));
        }

        public virtual string GetRenameText()
        {
            return Name;
        }

        public virtual bool CanRename()
        {
            return false;
        }

        public virtual async Task<bool> RenameAsync(string name)
        {
            return await Task.FromResult(false);
        }

        public bool ContainsRoot(FolderTreeNodeBase root)
        {
            if (this == root) return true;
            return this.Parent is not null && this.Parent.ContainsRoot(root);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _parent = null;
        }

        public override string ToString()
        {
            return DispName;
        }
    }
}

