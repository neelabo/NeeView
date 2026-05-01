using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Windows;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class SevenZipArchiveConfig : BindableBase
    {
        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".7z;.cb7;.cbr;.cbt;.cbz;.lzh;.rar;.zip");


        [DefaultEquality] private bool _isEnabled = true;
        [DefaultEquality] private string _x86DllPath = "";
        [DefaultEquality] private string _x64DllPath = "";
        [DefaultEquality] private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();


        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyPath(Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X86DllPath
        {
            get { return _x86DllPath; }
            set { SetProperty(ref _x86DllPath, value); }
        }

        [PropertyPath(Filter = "DLL|*.dll", DefaultFileName = "7z.dll")]
        public string X64DllPath
        {
            get { return _x64DllPath; }
            set { SetProperty(ref _x64DllPath, value); }
        }

        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

    }
}
