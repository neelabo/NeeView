namespace NeeView
{
    public interface IExportImageParameter : IImageExporterOptions
    {
        ExportImageMode Mode { get; }
        BitmapImageFormat FileFormat { get; }
        string FileNameFormat0 { get; }
        string FileNameFormat1 { get; }
        string FileNameFormat2 { get; }
        ExportImageOverwriteMode OverwriteMode { get; }
        string ExportFolder { get; }
    }
}