namespace NeeView
{
    public class FolderItemPosition
    {
        public FolderItemPosition(QueryPath path)
        {
            this.Path = path;
            this.Index = -1;
        }

        public FolderItemPosition(QueryPath path, int index)
        {
            this.Path = path;
            this.Index = index;
        }

        public FolderItemPosition(object source)
        {
            this.Source = source;
            this.Path = null;
            this.Index = -1;
        }


        public QueryPath? Path { get; private set; }
        public int Index { get; private set; }
        public object? Source { get; private set; }

        public override string ToString()
        {
            return $"{Path},{Index}";
        }
    }

}
