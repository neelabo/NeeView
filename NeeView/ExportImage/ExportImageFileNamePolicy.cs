namespace NeeView
{
    public interface IExportImageFileNamePolicy
    {
        string CreateFileName(IExportPageSource source, int index);
    }

    public sealed class DefaultExportImageFileNamePolicy : IExportImageFileNamePolicy
    {
        private readonly IExportImageParameter _parameter;
        private string _fileNameFormat0;
        private string _fileNameFormat1;
        private string _fileNameFormat2;

        public DefaultExportImageFileNamePolicy(IExportImageParameter parameter)
        {
            _parameter = parameter;

            // null to default
            _fileNameFormat0 = string.IsNullOrWhiteSpace(parameter.FileNameFormat0) ? ExportImageParameter.DefaultFileNameFormat0 : parameter.FileNameFormat0;
            _fileNameFormat1 = string.IsNullOrWhiteSpace(parameter.FileNameFormat1) ? ExportImageParameter.DefaultFileNameFormat1 : parameter.FileNameFormat1;
            _fileNameFormat2 = string.IsNullOrWhiteSpace(parameter.FileNameFormat2) ? _fileNameFormat1 : parameter.FileNameFormat2;

            // error to default
            try
            {
                var source = ExportFileNameFormat.CreateDummyFileNameSource(1, 1);
                ExportFileNameFormat.Format(parameter.FileNameFormat0, source, 1);
            }
            catch
            {
                _fileNameFormat0 = ExportImageParameter.DefaultFileNameFormat0;
            }

            try
            {
                var source = ExportFileNameFormat.CreateDummyFileNameSource(2, 1);
                ExportFileNameFormat.Format(parameter.FileNameFormat1, source, 0);
                ExportFileNameFormat.Format(parameter.FileNameFormat2, source, 0);
            }
            catch
            {
                _fileNameFormat1 = ExportImageParameter.DefaultFileNameFormat1;
                _fileNameFormat2 = ExportImageParameter.DefaultFileNameFormat2;
            }
        }

        public string CreateFileName(IExportPageSource source, int index)
        {
            var format = _parameter.Mode == ExportImageMode.Original
                ? _fileNameFormat0
                : source.Elements.Count >= 2 ? _fileNameFormat2 : _fileNameFormat1;

            var name = ExportFileNameFormat.Format(format, source, index);

            var extension = _parameter.Mode == ExportImageMode.Original
                ? LoosePath.GetExtension(source.Elements[0].Page.EntryLastName).ToLowerInvariant()
                : _parameter.FileFormat == BitmapImageFormat.Png ? ".png" : ".jpg";

            return name + extension;
        }
    }

    public static class ExportImageParameterTools
    {
        public static string GetDefaultFileNameFormat(ExportImageMode mode, int pageCount)
        {
            return mode == ExportImageMode.Original
                ? ExportImageParameter.DefaultFileNameFormat0
                : pageCount >= 2 ? ExportImageParameter.DefaultFileNameFormat2 : ExportImageParameter.DefaultFileNameFormat1;
        }
    }

}