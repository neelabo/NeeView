using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// 設定のみ
    /// </summary>
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ExportImageParameter : BindableBase, IExportImageParameter
    {
        [DefaultEquality] private ExportImageMode _mode;
        [DefaultEquality] private bool _hasBackground;
        [DefaultEquality] private bool _isOriginalSize = true;
        [DefaultEquality] private bool _isDotKeep;
        [DefaultEquality] private int _qualityLevel = 80;
        [DefaultEquality] private BitmapImageFormat _fileFormat = BitmapImageFormat.Png;
        [DefaultEquality] private string _fileNameFormat0 = DefaultFileNameFormat0;
        [DefaultEquality] private string _fileNameFormat1 = DefaultFileNameFormat1;
        [DefaultEquality] private string _fileNameFormat2 = DefaultFileNameFormat2;
        [DefaultEquality] private ExportImageOverwriteMode _overwriteMode = ExportImageOverwriteMode.Confirm;
        [DefaultEquality] private string _exportFolder = "";


        public const string DefaultFileNameFormat0 = "{Name}";
        public const string DefaultFileNameFormat1 = "{Index:000}";
        public const string DefaultFileNameFormat2 = "";
        //public const string OldPattern1 ="{Book}_{Page:000}";
        //public const string OldPattern2 ="{Book}_{Page1:000}-{Page2:000}";

        public ExportImageParameter()
        {
        }

        public ExportImageParameter(IExportImageParameter parameter)
        {
            _mode = parameter.Mode;
            _hasBackground = parameter.HasBackground;
            _isOriginalSize = parameter.IsOriginalSize;
            _isDotKeep = parameter.IsDotKeep;
            _exportFolder = parameter.ExportFolder;
            _qualityLevel = parameter.QualityLevel;
            _fileFormat = parameter.FileFormat;
            _overwriteMode = parameter.OverwriteMode;
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

        [PropertyRange(5, 100, TickFrequency = 5)]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, value);
        }

        [PropertyMember]
        public ExportImageMode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        [PropertyMember]
        public BitmapImageFormat FileFormat
        {
            get => _fileFormat;
            set => SetProperty(ref _fileFormat, value);
        }

        [PropertyMember]
        public string FileNameFormat0
        {
            get { return _fileNameFormat0; }
            set { SetProperty(ref _fileNameFormat0, value); }
        }

        [PropertyMember]
        public string FileNameFormat1
        {
            get { return _fileNameFormat1; }
            set { SetProperty(ref _fileNameFormat1, value); }
        }

        [PropertyMember]
        public string FileNameFormat2
        {
            get { return _fileNameFormat2; }
            set { SetProperty(ref _fileNameFormat2, value); }
        }

        [PropertyMember]
        public ExportImageOverwriteMode OverwriteMode
        {
            get { return _overwriteMode; }
            set { SetProperty(ref _overwriteMode, value); }
        }

        [PropertyMember]
        public string ExportFolder
        {
            get => _exportFolder ?? "";
            set => SetProperty(ref _exportFolder, value);
        }
    }
}
