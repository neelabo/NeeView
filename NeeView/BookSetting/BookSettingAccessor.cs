using NeeLaboratory.Generators;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// BookSettingConfig アクセサ
    /// </summary>
    public partial class BookSettingAccessor : IBookSetting, INotifyPropertyChanged
    {
        private readonly BookSettingConfig _setting;


        public BookSettingAccessor(BookSettingConfig setting)
        {
            _setting = setting;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => _setting.PropertyChanged += value;
            remove => _setting.PropertyChanged -= value;
        }


        /// <summary>
        /// 編集可能設定
        /// </summary>
        public bool CanEdit { get; set; } = true;


        public string Page
        {
            get => _setting.Page;
            set => _setting.Page = value;
        }

        public bool IsRecursiveFolder
        {
            get => _setting.IsRecursiveFolder;
            set => _setting.IsRecursiveFolder = value;
        }

        public PageSortMode SortMode
        {
            get => _setting.SortMode;
            set => _setting.SortMode = value;
        }

        public PageReadOrder BookReadOrder
        {
            get => _setting.BookReadOrder;
            set => _setting.BookReadOrder = value;
        }

        public bool IsSupportedDividePage
        {
            get => _setting.IsSupportedDividePage;
            set => _setting.IsSupportedDividePage = value;
        }

        public bool IsSupportedSingleFirstPage
        {
            get => _setting.IsSupportedSingleFirstPage;
            set => _setting.IsSupportedSingleFirstPage = value;
        }

        public bool IsSupportedSingleLastPage
        {
            get => _setting.IsSupportedSingleLastPage;
            set => _setting.IsSupportedSingleLastPage = value;
        }

        public bool IsSupportedWidePage
        {
            get => _setting.IsSupportedWidePage;
            set => _setting.IsSupportedWidePage = value;
        }

        public PageMode PageMode
        {
            get => _setting.PageMode;
            set => _setting.PageMode = value;
        }

        public AutoRotateType AutoRotate
        {
            get => _setting.AutoRotate;
            set => _setting.AutoRotate = value;
        }

        public double BaseScale
        {
            get => _setting.BaseScale;
            set => _setting.BaseScale = value;
        }

        // ページ数での可否
        public bool CanPageSizeSubSetting(int size)
        {
            return CanEdit && Config.Current.GetFramePageSize(_setting.PageMode) == size;
        }

        // 単ページ/見開き表示設定の可否
        public bool CanPageModeSubSetting(PageMode mode)
        {
            return CanEdit && _setting.PageMode == mode;
        }

        // 単ページ/見開き表示設定
        public void SetPageMode(PageMode mode)
        {
            if (!CanEdit) return;
            _setting.PageMode = mode;
        }

        public void TogglePageMode(int direction, bool isLoop)
        {
            SetPageMode(_setting.PageMode.GetToggle(direction, isLoop));
        }


        // 見開き方向設定
        public void SetBookReadOrder(PageReadOrder order)
        {
            if (!CanEdit) return;
            _setting.BookReadOrder = order;
        }

        public void ToggleBookReadOrder()
        {
            SetBookReadOrder(_setting.BookReadOrder.GetToggle());
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void SetIsSupportedSingleFirstPage(bool value)
        {
            if (!CanEdit) return;
            _setting.IsSupportedSingleFirstPage = value;
        }

        public void ToggleIsSupportedSingleFirstPage()
        {
            SetIsSupportedSingleFirstPage(!_setting.IsSupportedSingleFirstPage);
        }

        // 最終ページの単ページ表示ON/OFF 
        public void SetIsSupportedSingleLastPage(bool value)
        {
            if (!CanEdit) return;
            _setting.IsSupportedSingleLastPage = value;
        }

        public void ToggleIsSupportedSingleLastPage()
        {
            SetIsSupportedSingleLastPage(!_setting.IsSupportedSingleLastPage);
        }

        // 横長ページの分割ON/OFF
        public void SetIsSupportedDividePage(bool value)
        {
            if (!CanEdit) return;
            _setting.IsSupportedDividePage = value;
        }

        public void ToggleIsSupportedDividePage()
        {
            SetIsSupportedDividePage(!_setting.IsSupportedDividePage);
        }

        // 横長ページの見開き判定ON/OFF
        public void SetIsSupportedWidePage(bool value)
        {
            if (!CanEdit) return;
            _setting.IsSupportedWidePage = value;
        }

        public void ToggleIsSupportedWidePage()
        {
            SetIsSupportedWidePage(!_setting.IsSupportedWidePage);
        }


        // フォルダー再帰読み込みON/OFF
        public void SetIsRecursiveFolder(bool value)
        {
            if (!CanEdit) return;
            _setting.IsRecursiveFolder = value;
        }

        public void ToggleIsRecursiveFolder()
        {
            SetIsRecursiveFolder(!_setting.IsRecursiveFolder);
        }

        // ページ並び設定切り替え
        public void ToggleSortMode(PageSortModeClass pageSortModeClass)
        {
            if (!CanEdit) return;
            _setting.SortMode = pageSortModeClass.GetTogglePageSortMode(_setting.SortMode);
        }

        // ページ並び設定
        public void SetSortMode(PageSortMode mode)
        {
            if (!CanEdit) return;
            _setting.SortMode = mode;
        }

        // ページ並び設定切り替え
        public void SwitchAutoRotate(AutoRotateType autoRotate)
        {
            if (!CanEdit) return;
            _setting.AutoRotate = _setting.AutoRotate != autoRotate ? autoRotate : AutoRotateType.None;
        }

        // 自動回転
        public void SetAutoRotate(AutoRotateType autoRotate)
        {
            if (!CanEdit) return;
            _setting.AutoRotate = autoRotate;
        }

        // 基底スケール
        public void SetBaseScale(double baseScale)
        {
            if (!CanEdit) return;
            _setting.BaseScale = baseScale;
        }
    }

}
