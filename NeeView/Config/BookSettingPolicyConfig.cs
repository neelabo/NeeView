using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class BookSettingPolicyConfig : BindableBase, ICloneable
    {
        private BookSettingPageSelectMode _page = BookSettingPageSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _pageMode = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _bookReadOrder = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedDividePage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedSingleFirstPage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedSingleLastPage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isSupportedWidePage = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _isRecursiveFolder = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _sortMode = BookSettingSelectMode.RestoreOrDefault;
        private BookSettingSelectMode _autoRotate = BookSettingSelectMode.RestoreOrContinue;
        private BookSettingSelectMode _baseScale = BookSettingSelectMode.RestoreOrDefault;


        [PropertyMember]
        public BookSettingPageSelectMode Page
        {
            get { return _page; }
            set { SetProperty(ref _page, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode PageMode
        {
            get { return _pageMode; }
            set { SetProperty(ref _pageMode, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode BookReadOrder
        {
            get { return _bookReadOrder; }
            set { SetProperty(ref _bookReadOrder, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode IsSupportedDividePage
        {
            get { return _isSupportedDividePage; }
            set { SetProperty(ref _isSupportedDividePage, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode IsSupportedSingleFirstPage
        {
            get { return _isSupportedSingleFirstPage; }
            set { SetProperty(ref _isSupportedSingleFirstPage, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode IsSupportedSingleLastPage
        {
            get { return _isSupportedSingleLastPage; }
            set { SetProperty(ref _isSupportedSingleLastPage, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode IsSupportedWidePage
        {
            get { return _isSupportedWidePage; }
            set { SetProperty(ref _isSupportedWidePage, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode IsRecursiveFolder
        {
            get { return _isRecursiveFolder; }
            set { SetProperty(ref _isRecursiveFolder, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode SortMode
        {
            get { return _sortMode; }
            set { SetProperty(ref _sortMode, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode AutoRotate
        {
            get { return _autoRotate; }
            set { SetProperty(ref _autoRotate, value); }
        }

        [PropertyMember]
        public BookSettingSelectMode BaseScale
        {
            get { return _baseScale; }
            set { SetProperty(ref _baseScale, value); }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

}
