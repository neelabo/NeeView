using NeeView.Effects;
using NeeView.Runtime.LayoutPanel;
using NeeView.Text.Json;
using System.Collections.Generic;
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

    [JsonSerializable(typeof(PanelsConfig))] // TODO
    [JsonSerializable(typeof(LayoutPanelManager.Memento), TypeInfoPropertyName = "LayoutPanelManager_Memento")]
    [JsonSerializable(typeof(Dictionary<string, LayoutPanel.Memento>), TypeInfoPropertyName = "LayoutPanel_DictionaryStringMemento")]
    [JsonSerializable(typeof(Dictionary<string, LayoutPanelWindowManager.Memento>), TypeInfoPropertyName = "LayoutPanelWindowManager_DictionaryStringMemento")]
    [JsonSerializable(typeof(LayoutPanel.Memento), TypeInfoPropertyName = "LayoutPanel_Memento")]
    [JsonSerializable(typeof(LayoutPanelWindowManager.Memento), TypeInfoPropertyName = "LayoutPanelWindowManager_Memento")]

    [JsonSerializable(typeof(PanelListItemProfile))] // <- TODO: record化

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
    [JsonSerializable(typeof(UnsharpMaskConfig))] // TODO: デフォルト値

    [JsonSerializable(typeof(ViewConfig))]
    [JsonSerializable(typeof(MouseConfig))]
    [JsonSerializable(typeof(TouchConfig))]
    [JsonSerializable(typeof(LoupeConfig))]

    [JsonSerializable(typeof(BookConfig))]
    [JsonSerializable(typeof(BookSettingConfig))]
    [JsonSerializable(typeof(BookSettingPolicyConfig))]

    [JsonSerializable(typeof(CommandConfig))]
    [JsonSerializable(typeof(ScriptConfig))]

    [JsonSerializable(typeof(DestinationFolder))]
    [JsonSerializable(typeof(ExternalApp))]
    [JsonSerializable(typeof(BrushSource))]
    [JsonSerializable(typeof(BookMemento))]


    public partial class ConfigJsonSerializerContext : JsonSerializerContext
    {
    }
}
