namespace NeeView
{
    public record class QuickAccessFolderNodeSource
    {
        private readonly RootQuickAccessNode _node;

        public QuickAccessFolderNodeSource(RootQuickAccessNode node)
        {
            _node = node;
        }

        [WordNodeMember(AltName = "@Word.Name")]
        public string Name => _node.DisplayName;
    }

}
