using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class PageViewRecorderConfig : BindableBase
    {
        [DefaultEquality] private bool _isSavePageViewRecord;
        [DefaultEquality] private string? _pageViewRecordFilePath;

        // 履歴を保存するか
        [PropertyMember]
        public bool IsSavePageViewRecord
        {
            get { return _isSavePageViewRecord; }
            set { SetProperty(ref _isSavePageViewRecord, value); }
        }

        // 履歴データの保存場所
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "TSV|*.tsv")]
        public string? PageViewRecordFilePath
        {
            get { return _pageViewRecordFilePath; }
            set { SetProperty(ref _pageViewRecordFilePath, value); }
        }
    }
}
