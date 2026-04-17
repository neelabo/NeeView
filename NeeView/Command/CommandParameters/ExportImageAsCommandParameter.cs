using Generator.Equals;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// ExportImageAs Command Parameter (Obsolete)
    /// </summary>
    [Equatable(Explicit = true)]
    [WordNodeMember(IsEnabled = false)]
    public partial class ExportImageAsCommandParameter : CommandParameter
    {
        [Obsolete("no used"), Alternative(null, 46, ScriptErrorLevel.Warning)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        [DefaultEquality]
        public string ExportFolder
        {
            get => "";
            set => ExportFolderLegacy = value;
        }

        [Obsolete("no used"), Alternative(null, 46, ScriptErrorLevel.Warning)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        [PropertyRange(5, 100, TickFrequency = 5)]
        [DefaultEquality]
        public int QualityLevel
        {
            get => default;
            set => QualityLevelLegacy = value;
        }

        public string? ExportFolderLegacy { get; private set; }
        public int QualityLevelLegacy { get; private set; }

    }
}
