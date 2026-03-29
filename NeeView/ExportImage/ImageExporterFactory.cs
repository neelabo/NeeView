using System;

namespace NeeView
{
    public static class ImageExporterFactory
    {
        public static IImageExporter CreateExporter(ExportImageSource source, ExportImageMode mode)
        {
            return mode switch
            {
                ExportImageMode.Original
                    => new OriginalImageExporter(source),
                ExportImageMode.View
                    => new ViewImageExporter(source),
                _
                    => throw new InvalidOperationException(),
            };
        }
    }
}