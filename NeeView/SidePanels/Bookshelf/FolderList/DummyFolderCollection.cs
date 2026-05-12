namespace NeeView
{
    public class DummyFolderCollection : FolderCollection
    {
        public DummyFolderCollection() : base(new QueryPath(QueryScheme.None), false)
        {
        }

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.None;
    }
}
