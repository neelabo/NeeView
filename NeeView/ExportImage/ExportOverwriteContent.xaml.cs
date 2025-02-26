using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ExportOverwriteContent.xaml の相互作用ロジック
    /// </summary>
    public partial class ExportOverwriteContent : UserControl
    {
        public ExportOverwriteContent()
        {
            InitializeComponent();
        }

        public ExportOverwriteContent(PreviewContent source, PreviewContent destination) : this()
        {
            this.SourcePreviewContentControl.Source = source;
            this.DestinationPreviewContentControl.Source = destination;
        }
    }
}
