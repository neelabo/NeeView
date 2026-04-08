using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Windows;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class PdfArchiveConfig : BindableBase
    {
        public static bool IsPdfArchiveSupported => GetPdfRenderer() == PdfRenderer.Pdfium || Windows10Tools.IsWindows10_OrGreater(10240);

        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".pdf");

        [DefaultEquality] private bool _isEnabled = true;
        [DefaultEquality] private Size _renderSize = new(1920, 1080);
        [DefaultEquality] private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();


        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled && IsPdfArchiveSupported; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

        [PropertyMember]
        public Size RenderSize
        {
            get { return _renderSize; }
            set { SetProperty(ref _renderSize, new Size(Math.Max(value.Width, 256), Math.Max(value.Height, 256))); }
        }


        public static PdfRenderer GetPdfRenderer()
        {
#if USE_WINRT
            return Environment.PdfRenderer switch
            {
                PdfWinRTArchive.Identify
                    => PdfRenderer.WinRT,
                _
                    => PdfRenderer.Pdfium,
            };
#else
            return PdfRenderer.Pdfium;
#endif
        }
    }

    public enum PdfRenderer
    {
        Pdfium,
        WinRT,
    }

}
