using System;

namespace NeeView
{
    public class BookSettingPolicyConfigMap
    {
        private readonly BookSettingPolicyConfig _setting;

        public BookSettingPolicyConfigMap(BookSettingPolicyConfig setting)
        {
            _setting = setting;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:switch ステートメントを式に変換します", Justification = "<保留中>")]
        public BookSettingSelectMode this[BookSettingKey key]
        {
            get
            {
                switch (key)
                {
                    case BookSettingKey.Page: return _setting.Page.ToNormalSelectMode();
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
                    case BookSettingKey.Page: _setting.Page = value.ToPageSelectMode(); break;
                    case BookSettingKey.PageMode: _setting.PageMode = value; break;
                    case BookSettingKey.BookReadOrder: _setting.BookReadOrder = value; break;
                    case BookSettingKey.IsSupportedDividePage: _setting.IsSupportedDividePage = value; break;
                    case BookSettingKey.IsSupportedSingleFirstPage: _setting.IsSupportedSingleFirstPage = value; break;
                    case BookSettingKey.IsSupportedSingleLastPage: _setting.IsSupportedSingleLastPage = value; break;
                    case BookSettingKey.IsSupportedWidePage: _setting.IsSupportedWidePage = value; break;
                    case BookSettingKey.IsRecursiveFolder: _setting.IsRecursiveFolder = value; break;
                    case BookSettingKey.SortMode: _setting.SortMode = value; break;
                    case BookSettingKey.AutoRotate: _setting.AutoRotate = value; break;
                    case BookSettingKey.BaseScale: _setting.BaseScale = value; break;
                    case BookSettingKey.EffectProfileId: _setting.EffectProfileId = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

    }

}
