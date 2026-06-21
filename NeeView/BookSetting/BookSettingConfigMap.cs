using System;

namespace NeeView
{
    public class BookSettingConfigMap
    {
        private readonly BookSettingConfig _setting;

        public BookSettingConfigMap(BookSettingConfig setting)
        {
            _setting = setting;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:switch ステートメントを式に変換します", Justification = "<保留中>")]
        public object this[BookSettingKey key]
        {
            get
            {
                switch (key)
                {
                    case BookSettingKey.Page: return _setting.Page;
                    case BookSettingKey.PageMode: return _setting.PageMode;
                    case BookSettingKey.BookReadOrder: return _setting.BookReadOrder;
                    case BookSettingKey.IsSupportedDividePage: return _setting.IsSupportedDividePage;
                    case BookSettingKey.IsSupportedSingleFirstPage: return _setting.IsSupportedSingleFirstPage;
                    case BookSettingKey.IsSupportedSingleLastPage: return _setting.IsSupportedSingleLastPage;
                    case BookSettingKey.IsSupportedWidePage: return _setting.IsSupportedWidePage;
                    case BookSettingKey.IsRecursiveFolder: return _setting.IsRecursiveFolder;
                    case BookSettingKey.SortMode: return _setting.SortMode;
                    case BookSettingKey.AutoRotate: return _setting.AutoRotate;
                    case BookSettingKey.BaseScale: return _setting.BaseScale;
                    case BookSettingKey.EffectProfileId: return _setting.EffectProfileId;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (key)
                {
                    case BookSettingKey.Page: _setting.Page = (string)value; break;
                    case BookSettingKey.PageMode: _setting.PageMode = (PageMode)value; break;
                    case BookSettingKey.BookReadOrder: _setting.BookReadOrder = (PageReadOrder)value; break;
                    case BookSettingKey.IsSupportedDividePage: _setting.IsSupportedDividePage = (bool)value; break;
                    case BookSettingKey.IsSupportedSingleFirstPage: _setting.IsSupportedSingleFirstPage = (bool)value; break;
                    case BookSettingKey.IsSupportedSingleLastPage: _setting.IsSupportedSingleLastPage = (bool)value; break;
                    case BookSettingKey.IsSupportedWidePage: _setting.IsSupportedWidePage = (bool)value; break;
                    case BookSettingKey.IsRecursiveFolder: _setting.IsRecursiveFolder = (bool)value; break;
                    case BookSettingKey.SortMode: _setting.SortMode = (PageSortMode)value; break;
                    case BookSettingKey.AutoRotate: _setting.AutoRotate = (AutoRotateType)value; break;
                    case BookSettingKey.BaseScale: _setting.BaseScale = (double)value; break;
                    case BookSettingKey.EffectProfileId: _setting.EffectProfileId = (int)value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

    }

}
