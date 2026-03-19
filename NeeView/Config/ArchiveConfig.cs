using Generator.Equals;
using NeeLaboratory.ComponentModel;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class ArchiveConfig : BindableBase
    {
        public ZipArchiveConfig Zip { get; set; } = new ZipArchiveConfig();

        public SevenZipArchiveConfig SevenZip { get; set; } = new SevenZipArchiveConfig();

        public PdfArchiveConfig Pdf { get; set; } = new PdfArchiveConfig();

        public MediaArchiveConfig Media { get; set; } = new MediaArchiveConfig();
    }
}
