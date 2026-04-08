using Generator.Equals;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

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
        [DefaultEquality] private string _fileNameFormat0 = ExportImageParameter.DefaultFileNameFormat0;
        [DefaultEquality] private string _fileNameFormat1 = ExportImageParameter.DefaultFileNameFormat1;
        [DefaultEquality] private string _fileNameFormat2 = ExportImageParameter.DefaultFileNameFormat2;
        [DefaultEquality] private BitmapImageFormat _fileFormat;
        [DefaultEquality] private int _qualityLevel = 80;
        [DefaultEquality] private bool _isShowToast = true;
        [DefaultEquality] private ExportImageOverwriteMode _overwriteMode = ExportImageOverwriteMode.Confirm;


        [PropertyMember(Name = "ExportImageParameter.Mode")]
        public ExportImageMode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        [PropertyMember(Name = "ExportImageParameter.HasBackground")]
        public bool HasBackground
        {
            get => _hasBackground;
            set => SetProperty(ref _hasBackground, value);
        }

        [PropertyMember(Name = "ExportImageParameter.IsOriginalSize")]
        public bool IsOriginalSize
        {
            get { return _isOriginalSize; }
            set { SetProperty(ref _isOriginalSize, value); }
        }

        [PropertyMember(Name = "ExportImageParameter.IsDotKeep")]
        public bool IsDotKeep
        {
            get { return _isDotKeep; }
            set { SetProperty(ref _isDotKeep, value); }
        }

        [PropertyPath(Name = "ExportImageParameter.ExportFolder", FileDialogType = FileDialogType.Directory)]
        public string ExportFolder
        {
            get => _exportFolder ?? "";
            set => SetProperty(ref _exportFolder, value);
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

        [PropertyMember(Name = "ExportImageParameter.FileFormat")]
        public BitmapImageFormat FileFormat
        {
            get => _fileFormat;
            set => SetProperty(ref _fileFormat, value);
        }

        [PropertyRange(5, 100, TickFrequency = 5, Name = "ExportImageParameter.QualityLevel")]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, value);
        }

        [PropertyMember(Name = "ExportImageParameter.IsShowToast")]
        public bool IsShowToast
        {
            get { return _isShowToast; }
            set { SetProperty(ref _isShowToast, value); }
        }

        [PropertyMember(Name = "ExportImageParameter.OverwriteMode")]
        public ExportImageOverwriteMode OverwriteMode
        {
            get { return _overwriteMode; }
            set { SetProperty(ref _overwriteMode, value); }
        }


        #region Obsolete

        [Obsolete, Alternative("FileNameFormat0,1,2", 46, ScriptErrorLevel.Warning)] // ver.46
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        [PropertyMember(Name = "ExportImageParameter.FileNameMode")]
        public ExportImageFileNameMode? FileNameMode
        {
            get => ExportImageFileNameMode.Original;
            set
            {
                if (value is null)
                {
                    return;
                }
                else if (value == ExportImageFileNameMode.BookPageNumber)
                { 
                    FileNameFormat0 = "{Book}_{Page:000}";
                    FileNameFormat1 = "{Book}_{Page:000}";
                    FileNameFormat2 = "{Book}_{Page1:000}-{Page2:000}";
                }
                else
                {
                    FileNameFormat0 = ExportImageParameter.DefaultFileNameFormat0;
                    FileNameFormat1 = ExportImageParameter.DefaultFileNameFormat1;
                    FileNameFormat2 = ExportImageParameter.DefaultFileNameFormat1;
                }
            }
        }

        #endregion
    }


    public enum ExportImageOverwriteMode
    {
        Confirm,
        
        AddNumber,

        Disallow,
    }
}
