namespace NeeView
{
    public record class FileAssociationIcon
    {
        public FileAssociationIcon(FileAssociationCategory category)
        {
            FilePath = category.ToIconPath();
            Index = 0;
        }

        public FileAssociationIcon(string filePath, int index)
        {
            FilePath = filePath;
            Index = index;
        }

        public string FilePath { get; }

        public int Index { get; }

        public string CreateDefaultIconLiteral()
        {
            return $"{FilePath},{Index}";
        }
    }
}
