namespace NeeView
{
    public static class PdfArchiveFactory
    {
        public static PdfArchive Create(string path, ArchiveEntry? source, ArchiveHint archiveHint)
        {
#if USE_WINRT
            return PdfArchiveConfig.GetPdfRenderer() switch
            {
                PdfRenderer.WinRT
                    => new PdfWinRTArchive(path, source, archiveHint),
                _
                    => new PdfPdfiumArchive(path, source, archiveHint),
            };
#else
            return new PdfPdfiumArchive(path, source, archiveHint);
#endif
        }
    }
}
