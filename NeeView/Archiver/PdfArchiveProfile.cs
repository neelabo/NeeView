using NeeLaboratory.ComponentModel;
using System;
using System.Windows;

namespace NeeView
{
    public class PdfArchiveProfile : BindableBase
    {
        // 最大画像サイズで制限したサイズ
        public static Size SizeLimitedRenderSize
        {
            get
            {
                return new Size(
                    Math.Min(Config.Current.Archive.Pdf.RenderSize.Width, Config.Current.Performance.MaximumSize.Width),
                    Math.Min(Config.Current.Archive.Pdf.RenderSize.Height, Config.Current.Performance.MaximumSize.Height));
            }
        }

    }
}
