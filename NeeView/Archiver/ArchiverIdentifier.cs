
namespace NeeView
{
    /// <summary>
    /// アーカイバー識別子
    /// </summary>
    /// <param name="ArchiveType">アーカイブの種類</param>
    /// <param name="PluginName">プラグイン種別。現状 Susie のみ使用</param>
    public record ArchiverIdentifier(ArchiveType ArchiveType, string? PluginName = null)
    {
        public static ArchiverIdentifier None { get; } = new ArchiverIdentifier(ArchiveType.None);
        public static ArchiverIdentifier FolderArchive { get; } = new ArchiverIdentifier(ArchiveType.FolderArchive);
        public static ArchiverIdentifier ZipArchive { get; } = new ArchiverIdentifier(ArchiveType.ZipArchive);
        public static ArchiverIdentifier SevenZipArchive { get; } = new ArchiverIdentifier(ArchiveType.SevenZipArchive);
        public static ArchiverIdentifier PdfArchive { get; } = new ArchiverIdentifier(ArchiveType.PdfArchive);
        public static ArchiverIdentifier SusieArchive { get; } = new ArchiverIdentifier(ArchiveType.SusieArchive);
        public static ArchiverIdentifier MediaArchive { get; } = new ArchiverIdentifier(ArchiveType.MediaArchive);
        public static ArchiverIdentifier PlaylistArchive { get; } = new ArchiverIdentifier(ArchiveType.PlaylistArchive);

        public string ToDisplayString()
        {
            if (PluginName is null)
            {
                return ArchiveType.ToAliasName();
            }
            else
            {
                return ArchiveType.ToAliasName() + " (" + PluginName + ")";
            }
        }

        public override string ToString()
        {
            return ToDisplayString();
        }
    }
}

