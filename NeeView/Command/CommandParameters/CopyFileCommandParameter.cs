using Generator.Equals;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// CopyFileCommand Parameter
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class CopyFileCommandParameter : CommandParameter
    {
        [DefaultEquality] private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


        // 複数ページのときの動作
        [PropertyMember]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }


        #region Obsolete

        // 圧縮ファイルのときの動作
        [Obsolete("no used"), Alternative("nv.Config.System.ArchiveCopyPolicy", 42, ScriptErrorLevel.Warning, IsFullName = true)] // ver.42
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ArchivePolicy ArchivePolicy
        {
            get => default;
            set => ArchivePolicy_Legacy = value;
        }

        [Obsolete, JsonIgnore]
        public ArchivePolicy ArchivePolicy_Legacy { get; private set; }

        // テキストコピーのヒント
        [Obsolete("no used"), Alternative("nv.Config.System.TextCopyPolicy", 42, ScriptErrorLevel.Warning, IsFullName = true)] // ver.42
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public TextCopyPolicy TextCopyPolicy
        {
            get => default;
            set => TextCopyPolicy_Legacy = value;
        }

        [Obsolete, JsonIgnore]
        public TextCopyPolicy TextCopyPolicy_Legacy { get; private set; }

        #endregion Obsolete
    }
}
