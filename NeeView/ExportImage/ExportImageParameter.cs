using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// 設定のみ
    /// </summary>
    public class ExportImageParameter : BindableBase, IExportImageParameter
    {
        private ExportImageMode _mode;
        private bool _hasBackground;
        private bool _isOriginalSize = true;
        private bool _isDotKeep;
        private int _qualityLevel = 80;
        private BitmapImageFormat _fileFormat;
        private ExportImageFileNameMode _fileNameMode;
        private ExportImageOverwriteMode _overwriteMode = ExportImageOverwriteMode.Confirm;
        private string? _exportFolder;


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
            _fileNameMode = parameter.FileNameMode;
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
        public ExportImageFileNameMode FileNameMode
        {
            get => _fileNameMode;
            set => SetProperty(ref _fileNameMode, value);
        }

        [PropertyMember]
        public ExportImageOverwriteMode OverwriteMode
        {
            get { return _overwriteMode; }
            set { SetProperty(ref _overwriteMode, value); }
        }

        // 一時記憶用？
        [PropertyMember]
        public string ExportFolder
        {
            get => _exportFolder ?? "";
            set => SetProperty(ref _exportFolder, value);
        }
    }
}