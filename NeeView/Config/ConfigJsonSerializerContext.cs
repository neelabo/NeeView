using NeeView.Effects;
using NeeView.Runtime.LayoutPanel;
using NeeView.Text.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonSourceGenerationOptions(
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        WriteIndented = true,
        IgnoreReadOnlyProperties = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = new[] {
            typeof(JsonEnumFuzzyConverter),
            typeof(JsonColorConverter),
            typeof(JsonSizeConverter),
            typeof(JsonPointConverter),
            typeof(JsonTimeSpanConverter),
            typeof(JsonGridLengthConverter)}
        )]

    [JsonSerializable(typeof(Config))]

    [JsonSerializable(typeof(SystemConfig))]
    [JsonSerializable(typeof(StartUpConfig))]
    [JsonSerializable(typeof(PerformanceConfig))]

    [JsonSerializable(typeof(ImageConfig))]
    [JsonSerializable(typeof(ImageStandardConfig))]
    [JsonSerializable(typeof(ImageSvgConfig))]

    [JsonSerializable(typeof(ArchiveConfig))]
    [JsonSerializable(typeof(ZipArchiveConfig))]
    [JsonSerializable(typeof(SevenZipArchiveConfig))]
    [JsonSerializable(typeof(PdfArchiveConfig))]
    [JsonSerializable(typeof(MediaArchiveConfig))]

    [JsonSerializable(typeof(SusieConfig))]

    [JsonSerializable(typeof(HistoryConfig))]
    [JsonSerializable(typeof(PageViewRecorderConfig))]
    [JsonSerializable(typeof(BookmarkConfig))]
    [JsonSerializable(typeof(PlaylistConfig))]

    [JsonSerializable(typeof(WindowConfig))]
    [JsonSerializable(typeof(ThemeConfig))]
    [JsonSerializable(typeof(FontsConfig))]
    [JsonSerializable(typeof(BackgroundConfig))]
    [JsonSerializable(typeof(WindowTitleConfig))]
    [JsonSerializable(typeof(PageTitleConfig))]
    [JsonSerializable(typeof(AutoHideConfig))]
    [JsonSerializable(typeof(NoticeConfig))]

    [JsonSerializable(typeof(MenuBarConfig))]
    [JsonSerializable(typeof(SliderConfig))]
    [JsonSerializable(typeof(FilmStripConfig))]
    [JsonSerializable(typeof(MainViewConfig))]

    [JsonSerializable(typeof(PanelsConfig))]
    [JsonSerializable(typeof(LayoutPanelManagerMemento))]
    [JsonSerializable(typeof(LayoutPanelMemento))]
    [JsonSerializable(typeof(LayoutPanelWindowManagerMemento))]
    [JsonSerializable(typeof(LayoutDockPanelLayout))]

    [JsonSerializable(typeof(PanelListItemProfile))]
    [JsonSerializable(typeof(NormalItemProfile))]
    [JsonSerializable(typeof(ContentItemProfile))]
    [JsonSerializable(typeof(BannerItemProfile))]
    [JsonSerializable(typeof(ThumbnailItemProfile))]

    [JsonSerializable(typeof(BookshelfConfig))]
    [JsonSerializable(typeof(InformationConfig))]
    [JsonSerializable(typeof(NavigatorConfig))]
    [JsonSerializable(typeof(PageListConfig))]

    [JsonSerializable(typeof(ThumbnailConfig))]
    [JsonSerializable(typeof(SlideShowConfig))]

    [JsonSerializable(typeof(ImageEffectConfig))]
    [JsonSerializable(typeof(LevelEffectUnit))]
    [JsonSerializable(typeof(HsvEffectUnit))]
    [JsonSerializable(typeof(ColorSelectEffectUnit))]
    [JsonSerializable(typeof(BlurEffectUnit))]
    [JsonSerializable(typeof(BloomEffectUnit))]
    [JsonSerializable(typeof(MonochromeEffectUnit))]
    [JsonSerializable(typeof(ColorToneEffectUnit))]
    [JsonSerializable(typeof(SharpenEffectUnit))]
    [JsonSerializable(typeof(EmbossedEffectUnit))]
    [JsonSerializable(typeof(PixelateEffectUnit))]
    [JsonSerializable(typeof(MagnifyEffectUnit))]
    [JsonSerializable(typeof(RippleEffectUnit))]
    [JsonSerializable(typeof(SwirlEffectUnit))]

    [JsonSerializable(typeof(ImageCustomSizeConfig))]
    [JsonSerializable(typeof(ImageTrimConfig))]
    [JsonSerializable(typeof(ImageDotKeepConfig))]
    [JsonSerializable(typeof(ImageGridConfig))]
    [JsonSerializable(typeof(ImageResizeFilterConfig))]
    [JsonSerializable(typeof(UnsharpMaskConfig))]

    [JsonSerializable(typeof(ViewConfig))]
    [JsonSerializable(typeof(MouseConfig))]
    [JsonSerializable(typeof(TouchConfig))]
    [JsonSerializable(typeof(LoupeConfig))]

    [JsonSerializable(typeof(BookConfig))]
    [JsonSerializable(typeof(BookSettingConfig))]
    [JsonSerializable(typeof(BookSettingPolicyConfig))]

    [JsonSerializable(typeof(CommandConfig))]
    [JsonSerializable(typeof(ScriptConfig))]

    [JsonSerializable(typeof(PrintModelMemento))]

    [JsonSerializable(typeof(DestinationFolder))]
    [JsonSerializable(typeof(ExternalApp))]
    [JsonSerializable(typeof(BrushSource))]

    [JsonSerializable(typeof(MenuNode))]
    [JsonSerializable(typeof(SusiePluginMemento))]
    [JsonSerializable(typeof(CommandElementMemento))]

    [JsonSerializable(typeof(ReversibleCommandParameter))]
    [JsonSerializable(typeof(MoveSizePageCommandParameter))]
    [JsonSerializable(typeof(TogglePageModeCommandParameter))]
    [JsonSerializable(typeof(ToggleStretchModeCommandParameter))]
    [JsonSerializable(typeof(StretchModeCommandParameter))]
    [JsonSerializable(typeof(ViewScrollCommandParameter))]
    [JsonSerializable(typeof(ViewPresetScrollCommandParameter))]
    [JsonSerializable(typeof(ViewScaleCommandParameter))]
    [JsonSerializable(typeof(ViewRotateCommandParameter))]
    [JsonSerializable(typeof(MovePlaylistItemInBookCommandParameter))]
    [JsonSerializable(typeof(ScrollPageCommandParameter))]
    [JsonSerializable(typeof(FocusMainViewCommandParameter))]
    [JsonSerializable(typeof(ExportImageAsCommandParameter))]
    [JsonSerializable(typeof(ExportImageCommandParameter))]
    [JsonSerializable(typeof(OpenExternalAppCommandParameter))]
    [JsonSerializable(typeof(OpenExternalAppAsCommandParameter))]
    [JsonSerializable(typeof(OpenBookExternalAppAsCommandParameter))]
    [JsonSerializable(typeof(CopyFileCommandParameter))]
    [JsonSerializable(typeof(ViewScrollNTypeCommandParameter))]
    [JsonSerializable(typeof(ScriptCommandParameter))]
    [JsonSerializable(typeof(ImportBackupCommandParameter))]
    [JsonSerializable(typeof(ExportBackupCommandParameter))]
    [JsonSerializable(typeof(CopyToFolderAsCommandParameter))]
    [JsonSerializable(typeof(MoveToFolderAsCommandParameter))]
    [JsonSerializable(typeof(CopyBookToFolderAsCommandParameter))]
    [JsonSerializable(typeof(MoveBookToFolderAsCommandParameter))]
    [JsonSerializable(typeof(ToggleBookmarkCommandParameter))]

    public partial class ConfigJsonSerializerContext : JsonSerializerContext
    {
    }
}
