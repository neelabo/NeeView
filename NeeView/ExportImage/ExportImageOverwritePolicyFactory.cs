using System;

namespace NeeView
{
    public static class ExportImageOverwritePolicyFactory
    {
        public static IExportOverwritePolicy Create(ExportImageOverwriteMode mode)
        {
            return mode switch
            {
                ExportImageOverwriteMode.Confirm
                    => new ConfirmExportOverwritePolicy(),
                ExportImageOverwriteMode.AddNumber
                    => new AddNumberExportOverwritePolicy(),
                ExportImageOverwriteMode.Disallow
                    => new DisallowExportOverwritePolicy(),
                _
                    => throw new NotSupportedException($"Unsupported overwrite mode: {mode}"),
            };
        }
    }
}