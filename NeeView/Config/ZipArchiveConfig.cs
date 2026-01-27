using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ZipArchiveConfig : BindableBase
    {
        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".zip");


        private bool _isEnabled = true;
        private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();
        private bool _isWriteAccessEnabled = false;
        private ZipEncoding _encoding = ZipEncoding.Local;


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

        [PropertyMember]
        public bool IsFileWriteAccessEnabled
        {
            get { return _isWriteAccessEnabled; }
            set { SetProperty(ref _isWriteAccessEnabled, value); }
        }

        [PropertyMember]
        public ZipEncoding Encoding
        {
            get { return _encoding; }
            set { SetProperty(ref _encoding, value); }
        }
    }


    public enum ZipEncoding
    {
        Local,
        UTF8,
        Auto,
    }
}
