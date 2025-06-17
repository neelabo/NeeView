// TODO: パネルからのUI操作とスクリプトからの操作の２系統がごちゃまぜになっているので整備する

namespace NeeView
{
    public class RootFolderTree : FolderTreeNodeBase
    {
        public override string Name { get => ""; set { } }
        public override string DisplayName { get => "@Bookshelf"; set { } }

        public override IImageSourceCollection? Icon => null;
    }
}
