using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class BookSettingConfig : BindableBase, ICloneable, IBookSetting, IHasAutoRotate
    {
        [DefaultEquality] private string _page = "";
        [DefaultEquality] private PageMode _pageMode = PageMode.SinglePage;
        [DefaultEquality] private PageReadOrder _bookReadOrder = PageReadOrder.RightToLeft;
        [DefaultEquality] private bool _isSupportedDividePage;
        [DefaultEquality] private bool _isSupportedSingleFirstPage;
        [DefaultEquality] private bool _isSupportedSingleLastPage;
        [DefaultEquality] private bool _isSupportedWidePage = true;
        [DefaultEquality] private bool _isRecursiveFolder;
        [DefaultEquality] private PageSortMode _sortMode = PageSortMode.Entry;
        [DefaultEquality] private AutoRotateType _autoRotate;
        [DefaultEquality] private double _baseScale = 1.0;


        // ページ
        [JsonIgnore, PropertyMapIgnore]
        [PropertyMember]
        public string Page
        {
            get { return _page; }
            set { SetProperty(ref _page, value); }
        }

        // 1ページ表示 or 2ページ表示
        [PropertyMember]
        public PageMode PageMode
        {
            get { return _pageMode; }
            set { SetProperty(ref _pageMode, value); }
        }

        // 右開き or 左開き
        [PropertyMember]
        public PageReadOrder BookReadOrder
        {
            get { return _bookReadOrder; }
            set { SetProperty(ref _bookReadOrder, value); }
        }

        // 横長ページ分割 (1ページモード)
        [PropertyMember]
        public bool IsSupportedDividePage
        {
            get { return _isSupportedDividePage; }
            set { SetProperty(ref _isSupportedDividePage, value); }
        }

        // 最初のページを単独表示 
        [PropertyMember]
        public bool IsSupportedSingleFirstPage
        {
            get { return _isSupportedSingleFirstPage; }
            set { SetProperty(ref _isSupportedSingleFirstPage, value); }
        }

        // 最後のページを単独表示
        [PropertyMember]
        public bool IsSupportedSingleLastPage
        {
            get { return _isSupportedSingleLastPage; }
            set { SetProperty(ref _isSupportedSingleLastPage, value); }
        }

        // 横長ページを2ページ分とみなす(2ページモード)
        [PropertyMember]
        public bool IsSupportedWidePage
        {
            get { return _isSupportedWidePage; }
            set { SetProperty(ref _isSupportedWidePage, value); }
        }

        // フォルダーの再帰
        [PropertyMember]
        public bool IsRecursiveFolder
        {
            get { return _isRecursiveFolder; }
            set { SetProperty(ref _isRecursiveFolder, value); }
        }

        // ページ並び順
        [PropertyMember]
        public PageSortMode SortMode
        {
            get { return _sortMode; }
            set { SetProperty(ref _sortMode, value); }
        }

        // 自動回転
        [PropertyMember]
        public AutoRotateType AutoRotate
        {
            get { return _autoRotate; }
            set { SetProperty(ref _autoRotate, value); }
        }

        // 基底スケール
        [PropertyPercent(0.1, 2.0, TickFrequency = 0.01, IsEditable = true)]
        public double BaseScale
        {
            get { return _baseScale; }
            set { SetProperty(ref _baseScale, AppMath.Round(value)); }
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public interface IHasAutoRotate
    {
        AutoRotateType AutoRotate { get; set; }
    }
}
