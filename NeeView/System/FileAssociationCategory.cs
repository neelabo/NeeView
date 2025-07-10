namespace NeeView
{
    public enum FileAssociationCategory
    {
        [AliasName("Word.ForNeeView")]
        ForNeeView,

        [AliasName("Word.Image")]
        Image,

        [AliasName("Word.Archive")]
        Archive,
        
        [AliasName("Word.Media")]
        Media,
    }

    public static class FileAssociationCategoryExtensions
    {
        public static string ToIconPath(this FileAssociationCategory category)
        {
            var fileName = category is FileAssociationCategory.Image ? "File.ico" : "Book.ico";
            return System.IO.Path.Combine(Environment.AssemblyFolder, "Libraries", fileName);
        }
    }
}