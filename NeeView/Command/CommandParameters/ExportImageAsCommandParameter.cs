using Generator.Equals;
using NeeLaboratory;
using NeeView;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// ExportImageAs Command Parameter
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class ExportImageAsCommandParameter : CommandParameter
    {
        [DefaultEquality] private string? _exportFolder;
        [DefaultEquality] private int _qualityLevel = 80;

        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string ExportFolder
        {
            get => _exportFolder ?? "";
            set => SetProperty(ref _exportFolder, value);
        }

        [PropertyRange(5, 100, TickFrequency = 5)]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, value.Clamp(5, 100));
        }
    }
}
