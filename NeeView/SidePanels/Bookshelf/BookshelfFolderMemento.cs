namespace NeeView
{
    public record class BookshelfFolderMemento : FolderParameterMemento
    {
        public BookshelfFolderMemento()
        {
            Path = "";
        }

        public BookshelfFolderMemento(QueryPath query, QueryPath? selectedItem, FolderParameter? parameter)
        {
            Path = query.SimpleQuery;
            Select = selectedItem?.SimplePath;
            FolderOrder = parameter?.FolderOrder;
            IsFolderRecursive = parameter?.IsFolderRecursive ?? false;
            Seed = parameter?.Seed ?? 0;
        }

        public string Path { get; init; }

        public string? Select { get; init; }

        public void Register()
        {
            var path = new QueryPath(Path).SimplePath;
            var folderOrder = FolderParameter.GetFolderOrder(path, this.FolderOrder);
            FolderConfigCollection.Current.SetFolderParameter(path, new FolderParameterMemento(folderOrder, this.IsFolderRecursive, this.Seed));
        }
    }

}
