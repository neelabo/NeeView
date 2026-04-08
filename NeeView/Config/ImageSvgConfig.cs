using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ImageSvgConfig : BindableBase
    {
        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".svg");

        [DefaultEquality] private bool _isEnabled = true;
        [DefaultEquality] private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();

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
