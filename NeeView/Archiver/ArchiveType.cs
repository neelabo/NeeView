
namespace NeeView
{
    /// <summary>
    /// アーカイバーの種類
    /// </summary>
    public enum ArchiveType
    {
        None,

        [AliasName("@Archiver.Folder")]
        FolderArchive,

        [AliasName("@Archiver.Zip")]
        ZipArchive,

        [AliasName("@Archiver.SevenZip")]
        SevenZipArchive,

        [AliasName("@Archiver.Pdfium")]
        PdfArchive,

        [AliasName("@Archiver.Susie")]
        SusieArchive,

        [AliasName("@Archiver.Media")]
        MediaArchive,

        [AliasName("@Archiver.Playlist")]
        PlaylistArchive,
    }

    public static class ArchiveTypeExtensions
    {
        // 多重圧縮ファイルが可能なアーカイブであるか
        public static bool IsRecursiveSupported(this ArchiveType self)
        {
            return self switch
            {
                ArchiveType.PdfArchive or ArchiveType.MediaArchive => false,
                _ => true,
            };
        }
    }
}

