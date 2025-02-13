namespace NeeView
{
    public record class DirectoryNodeSource
    {
        private readonly DirectoryNode _node;

        public DirectoryNodeSource(DirectoryNode node)
        {
            _node = node;
        }

        [WordNodeMember(AltName = "@Word.Path")]
        public string Path
        {
            get { return _node.Path; }
        }

        [WordNodeMember(AltName = "@Word.Name")]
        public string Name
        {
            get { return _node.DisplayName; }
        }
    }
}
