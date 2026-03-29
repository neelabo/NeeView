namespace NeeView
{
    public interface IExportImageParameter : IImageExporterOptions
    {
        ExportImageMode Mode { get; }
        BitmapImageFormat FileFormat { get; }
        ExportImageFileNameMode FileNameMode { get; }
        ExportImageOverwriteMode OverwriteMode { get; }
        string? ExportFolder { get; }
    }
}