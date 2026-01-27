using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageSvgConfig : BindableBase
    {
        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".svg");


        private bool _isEnabled = true;
        private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();

        // support SVG
        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }
    }
}
