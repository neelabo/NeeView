namespace NeeView
{
    public record class BookshelfFolderMemento : FolderParameter.Memento
    {
        public BookshelfFolderMemento()
        {
            Path = "";
        }

        public BookshelfFolderMemento(QueryPath query, FolderParameter? parameter)
        {
            Path = query.SimpleQuery;
            FolderOrder = parameter?.FolderOrder ?? FolderParameter.GetDefaultFolderOrder(query.SimplePath);
            IsFolderRecursive = parameter?.IsFolderRecursive ?? false;
            Seed = parameter?.Seed ?? 0;
        }

        public string Path { get; set; }

        public void Register()
        {
            var path = new QueryPath(Path).SimplePath;
            BookHistoryCollection.Current.SetFolderMemento(path, this);
        }
    }

}
