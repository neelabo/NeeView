namespace NeeView
{
    public interface IExportImageFileNamePolicy
    {
        string CreateFileName(IExportPageSource source, ExportImageMode mode, ExportImageFileNameMode fileNameMode, BitmapImageFormat format);
    }

    public sealed class DefaultExportImageFileNamePolicy : IExportImageFileNamePolicy
    {
        public string CreateFileName(IExportPageSource source, ExportImageMode mode, ExportImageFileNameMode fileNameMode, BitmapImageFormat format)

        {
            var nameMode = fileNameMode == ExportImageFileNameMode.Default
                ? mode == ExportImageMode.Original ? ExportImageFileNameMode.Original : ExportImageFileNameMode.BookPageNumber
                : fileNameMode;

            var extension = mode == ExportImageMode.Original
                ? LoosePath.GetExtension(source.Pages[0].EntryLastName).ToLowerInvariant()
                : format == BitmapImageFormat.Png ? ".png" : ".jpg";

            if (nameMode == ExportImageFileNameMode.Original)
            {
                var filename = LoosePath.ValidFileName(source.Pages[0].EntryLastName);
                return System.IO.Path.ChangeExtension(filename, extension).TrimEnd('.');
            }
            else
            {
                // TODO: bookAddress は pages[0].BookPath でもいいかも？
                var bookName = LoosePath.GetFileNameWithoutExtension(source.BookAddress);

                var indexLabel = mode != ExportImageMode.Original && source.Pages.Count > 1
                    ? $"{source.Pages[0].Index:000}-{source.Pages[1].Index:000}"
                    : $"{source.Pages[0].Index:000}";

                return LoosePath.ValidFileName($"{bookName}_{indexLabel}{extension}");
            }
        }
    }
}