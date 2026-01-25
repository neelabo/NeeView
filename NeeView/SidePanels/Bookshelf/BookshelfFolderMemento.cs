namespace NeeView
{
    public record class BookshelfFolderMemento : FolderParameter.Memento
    {
        public BookshelfFolderMemento()
        {
            Path = "";
        }

        public BookshelfFolderMemento(QueryPath query, QueryPath? selectedItem, FolderParameter? parameter)
        {
            Path = query.SimpleQuery;
            Select = selectedItem?.SimplePath;
            FolderOrder = parameter?.FolderOrder ?? FolderParameter.GetDefaultFolderOrder(query.SimplePath);
            IsFolderRecursive = parameter?.IsFolderRecursive ?? false;
            Seed = parameter?.Seed ?? 0;
        }

        public string Path { get; set; }

        public string? Select { get; set; }

        public void Register()
        {
            var path = new QueryPath(Path).SimplePath;
            FolderConfigCollection.Current.SetFolderParameter(path, new FolderParameter.Memento(this.FolderOrder, this.IsFolderRecursive, this.Seed));
        }
    }

}
