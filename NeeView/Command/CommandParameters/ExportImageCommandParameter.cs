using Generator.Equals;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class ExportImageCommandParameter : CommandParameter, IExportImageParameter
    {
        [DefaultEquality] private ExportImageMode _mode;
        [DefaultEquality] private bool _hasBackground;
        [DefaultEquality] private bool _isOriginalSize = true;
        [DefaultEquality] private bool _isDotKeep;
        [DefaultEquality] private string? _exportFolder;
        [DefaultEquality] private ExportImageFileNameMode _fileNameMode;
        [DefaultEquality] private BitmapImageFormat _fileFormat;
        [DefaultEquality] private int _qualityLevel = 80;
        [DefaultEquality] private bool _isShowToast = true;
        [DefaultEquality] private ExportImageOverwriteMode _overwriteMode = ExportImageOverwriteMode.Confirm;


        [PropertyMember]
        public ExportImageMode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        [PropertyMember]
        public bool HasBackground
        {
            get => _hasBackground;
            set => SetProperty(ref _hasBackground, value);
        }

        [PropertyMember]
        public bool IsOriginalSize
        {
            get { return _isOriginalSize; }
            set { SetProperty(ref _isOriginalSize, value); }
        }

        [PropertyMember]
        public bool IsDotKeep
        {
            get { return _isDotKeep; }
            set { SetProperty(ref _isDotKeep, value); }
        }

        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string ExportFolder
        {
            get => _exportFolder ?? "";
            set => SetProperty(ref _exportFolder, value);
        }

        [PropertyMember]
        public ExportImageFileNameMode FileNameMode
        {
            get => _fileNameMode;
            set => SetProperty(ref _fileNameMode, value);
        }

        [PropertyMember]
        public BitmapImageFormat FileFormat
        {
            get => _fileFormat;
            set => SetProperty(ref _fileFormat, value);
        }

        [PropertyRange(5, 100, TickFrequency = 5)]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, value);
        }

        [PropertyMember]
        public bool IsShowToast
        {
            get { return _isShowToast; }
            set { SetProperty(ref _isShowToast, value); }
        }

        [PropertyMember]
        public ExportImageOverwriteMode OverwriteMode
        {
            get { return _overwriteMode; }
            set { SetProperty(ref _overwriteMode, value); }
        }
    }


    public enum ExportImageOverwriteMode
    {
        Confirm,

        AddNumber,

        Invalid,
    }
}
