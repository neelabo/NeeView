using NeeView.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public class QuickAccessFolderNode : QuickAccessNodeBase
    {
        public QuickAccessFolderNode(TreeListNode<IQuickAccessEntry> source, FolderTreeNodeBase? parent)
        {
            Source = source;
            Parent = parent;

            Icon = new SingleImageSourceCollection(ResourceTools.GetElementResource<ImageSource>(MainWindow.Current, "ic_lightning"));
        }

        public override string Name { get => QuickAccessSource.Value.Name ?? ""; set { } }

        public override string DisplayName { get => Name; set { } }

        public override IImageSourceCollection Icon { get; }


        [NotNull]
        public override ObservableCollection<FolderTreeNodeBase>? Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<FolderTreeNodeBase>(QuickAccessSource.Children
                        .Select(e => CreateFolderNode(e, this)));
                }
                return _children;
            }
            set
            {
                SetProperty(ref _children, value);
            }
        }

        protected FolderTreeNodeBase CreateFolderNode(TreeListNode<IQuickAccessEntry> source, FolderTreeNodeBase? parent)
        {
            if (source.Value is QuickAccessFolder)
            {
                return new QuickAccessFolderNode(source, parent);
            }
            else if (source.Value is QuickAccess)
            {
                return new QuickAccessNode(source, parent);
            }
            else
            {
                throw new InvalidOperationException("Unknown QuickAccessEntry type.");
            }
        }

        public override async ValueTask<bool> RenameAsync(string name)
        {
            return await ValueTask.FromResult(Rename(name));
        }

        public bool Rename(string name)
        {
            if (QuickAccessSource.Value.Name == name) return false;

            QuickAccessSource.Value.Name = name;

            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(DisplayName));
            return true;
        }

        protected override void Sort(FolderTreeNodeBase node)
        {
        }
    }
}
