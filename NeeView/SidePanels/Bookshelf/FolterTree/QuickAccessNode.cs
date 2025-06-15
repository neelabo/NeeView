using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{

    public class QuickAccessNode : QuickAccessNodeBase
    {
        public QuickAccessNode(TreeListNode<IQuickAccessEntry> source, FolderTreeNodeBase? parent)
        {
            Source = source;
            Parent = parent;
        }


        public override string Name { get => QuickAccessSource.Value.Name ?? ""; set { } }

        public override string DisplayName { get => Name; set { } }

        public string Path { get => QuickAccessSource.Value.Path ?? ""; }

        public override IImageSourceCollection Icon => PathToPlaceIconConverter.Convert(new QueryPath(QuickAccessSource.Value?.Path));

        public override string GetRenameText()
        {
            return this.Name;
        }

        public override bool CanRename()
        {
            return true;
        }

        public override async ValueTask<bool> RenameAsync(string name)
        {
            return await Task.FromResult(Rename(name));
        }

        public bool Rename(string name)
        {
            if (QuickAccessSource.Value.Name == name) return false;

            QuickAccessSource.Value.Name = name;
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(DisplayName));
            return true;
        }

        public void SetPath(string path)
        {
            if (QuickAccessSource.Value.Path == path) return;

            QuickAccessSource.Value.Path = path;
            RefreshAllProperties();
        }
    }
}
