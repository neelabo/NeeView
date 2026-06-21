namespace NeeView
{
    public interface IBookPageViewSetting
    {
        PageReadOrder BookReadOrder { get; set; }
        bool IsSupportedDividePage { get; set; }
        bool IsSupportedSingleFirstPage { get; set; }
        bool IsSupportedSingleLastPage { get; set; }
        bool IsSupportedWidePage { get; set; }
        PageMode PageMode { get; set; }
        AutoRotateType AutoRotate { get; set; }
        double BaseScale { get; set; }
        int EffectProfileId { get; set; }

        bool IsEquals(IBookPageViewSetting? other)
        {
            return other is not null &&
                   PageMode == other.PageMode &&
                   BookReadOrder == other.BookReadOrder &&
                   IsSupportedDividePage == other.IsSupportedDividePage &&
                   IsSupportedSingleFirstPage == other.IsSupportedSingleFirstPage &&
                   IsSupportedSingleLastPage == other.IsSupportedSingleLastPage &&
                   IsSupportedWidePage == other.IsSupportedWidePage &&
                   AutoRotate == other.AutoRotate &&
                   BaseScale == other.BaseScale &&
                   EffectProfileId == other.EffectProfileId;
        }
    }
}
