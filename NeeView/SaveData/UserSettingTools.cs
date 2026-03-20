using NeeView.Effects;
using NeeView.Runtime.LayoutPanel;
using NeeView.Text.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NeeView
{
    public static class UserSettingTools
    {
        private static JsonSerializerOptions? _serializerOptions;

        public static FormatVersion? UserSettingFormat { get; private set; }


        public static UserSetting CreateUserSetting(bool trim)
        {
            // 情報の確定
            MainWindow.Current.StoreWindowPlacement();
            MainViewManager.Current.Store();
            CustomLayoutPanelManager.Current.Store();

            return new UserSetting()
            {
                Format = new FormatVersion(Environment.SolutionName),
                Config = Config.Current,
                ContextMenu = ContextMenuSource.Current.CreateContextMenuNode(),
                SusiePlugins = SusiePluginManager.Current.CreateSusiePluginCollection(),
                DragActions = DragActionTable.Current.CreateDragActionCollection(trim),
                Commands = CommandTable.Current.CreateCommandCollectionMemento(trim),
            };
        }

        public static void Save(string path, string? backupFileName)
        {
            Save(path, CreateUserSetting(AppSettings.Current.TrimSaveData), backupFileName);
        }

        public static void Save(string path, UserSetting setting, string? backupFileName)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(setting, GetSerializerOptions());
            FileIO.WriteAllBytesDurable(path, json, backupFileName);
        }

        public static byte[] LoadBytes(string path)
        {
            return FileIO.ReadAllBytesShared(path);
        }

        public static UserSetting? Load(string path)
        {
            using var stream = FileIO.OpenReadShared(path);
            return Load(stream);
        }

        public static BootSetting? LoadBootSetting(byte[] bytes)
        {
            try
            {
                var doc = JsonDocument.Parse(bytes);
                var config = doc.RootElement.GetProperty("Config"u8);
                var startup = config.GetProperty("StartUp"u8);
                var boot = new BootSetting();
                boot.Language = config.GetProperty("System"u8).GetProperty("Language"u8).GetString() ?? "en";
                boot.IsSplashScreenEnabled = startup.GetProperty("IsSplashScreenEnabled"u8).GetBoolean();
                boot.IsMultiBootEnabled = startup.GetProperty("IsMultiBootEnabled"u8).GetBoolean();
                return boot;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load BootSetting: " + ex.Message);
                return null;
            }
        }

        public static UserSetting? Load(Stream stream)
        {
            return JsonSerializer.Deserialize<UserSetting>(stream, GetSerializerOptions())?.Validate();
        }

        public static JsonSerializerOptions GetSerializerOptions()
        {
            _serializerOptions ??= CreateSerializerOptions();
            return _serializerOptions;
        }

        public static JsonSerializerOptions CreateSerializerOptions()
        {
            var options = new JsonSerializerOptions();

            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;
            options.IgnoreReadOnlyProperties = true;

            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.AllowTrailingCommas = true;

            options.Converters.Add(new JsonEnumFuzzyConverter());
            options.Converters.Add(new JsonColorConverter());
            options.Converters.Add(new JsonSizeConverter());
            options.Converters.Add(new JsonPointConverter());
            options.Converters.Add(new JsonTimeSpanConverter());
            options.Converters.Add(new JsonGridLengthConverter());

            options.Converters.Add(new DiffJsonConverter<SystemConfig>(ConfigJsonSerializerContext.Default.SystemConfig));
            options.Converters.Add(new DiffJsonConverter<StartUpConfig>(ConfigJsonSerializerContext.Default.StartUpConfig));
            options.Converters.Add(new DiffJsonConverter<PerformanceConfig>(ConfigJsonSerializerContext.Default.PerformanceConfig));

            options.Converters.Add(new DiffJsonConverter<ImageConfig>(ConfigJsonSerializerContext.Default.ImageConfig));
            options.Converters.Add(new DiffJsonConverter<ImageStandardConfig>(ConfigJsonSerializerContext.Default.ImageStandardConfig));
            options.Converters.Add(new DiffJsonConverter<ImageSvgConfig>(ConfigJsonSerializerContext.Default.ImageSvgConfig));

            options.Converters.Add(new DiffJsonConverter<ArchiveConfig>(ConfigJsonSerializerContext.Default.ArchiveConfig));
            options.Converters.Add(new DiffJsonConverter<ZipArchiveConfig>(ConfigJsonSerializerContext.Default.ZipArchiveConfig));
            options.Converters.Add(new DiffJsonConverter<SevenZipArchiveConfig>(ConfigJsonSerializerContext.Default.SevenZipArchiveConfig));
            options.Converters.Add(new DiffJsonConverter<PdfArchiveConfig>(ConfigJsonSerializerContext.Default.PdfArchiveConfig));
            options.Converters.Add(new DiffJsonConverter<MediaArchiveConfig>(ConfigJsonSerializerContext.Default.MediaArchiveConfig));

            options.Converters.Add(new DiffJsonConverter<SusieConfig>(ConfigJsonSerializerContext.Default.SusieConfig));

            options.Converters.Add(new DiffJsonConverter<HistoryConfig>(ConfigJsonSerializerContext.Default.HistoryConfig));
            options.Converters.Add(new DiffJsonConverter<PageViewRecorderConfig>(ConfigJsonSerializerContext.Default.PageViewRecorderConfig));
            options.Converters.Add(new DiffJsonConverter<BookmarkConfig>(ConfigJsonSerializerContext.Default.BookmarkConfig));
            options.Converters.Add(new DiffJsonConverter<PlaylistConfig>(ConfigJsonSerializerContext.Default.PlaylistConfig));

            options.Converters.Add(new DiffJsonConverter<WindowConfig>(ConfigJsonSerializerContext.Default.WindowConfig));
            options.Converters.Add(new DiffJsonConverter<ThemeConfig>(ConfigJsonSerializerContext.Default.ThemeConfig));
            options.Converters.Add(new DiffJsonConverter<FontsConfig>(ConfigJsonSerializerContext.Default.FontsConfig));
            options.Converters.Add(new DiffJsonConverter<BackgroundConfig>(ConfigJsonSerializerContext.Default.BackgroundConfig));
            options.Converters.Add(new DiffJsonConverter<WindowTitleConfig>(ConfigJsonSerializerContext.Default.WindowTitleConfig));
            options.Converters.Add(new DiffJsonConverter<PageTitleConfig>(ConfigJsonSerializerContext.Default.PageTitleConfig));
            options.Converters.Add(new DiffJsonConverter<AutoHideConfig>(ConfigJsonSerializerContext.Default.AutoHideConfig));
            options.Converters.Add(new DiffJsonConverter<NoticeConfig>(ConfigJsonSerializerContext.Default.NoticeConfig));

            options.Converters.Add(new DiffJsonConverter<MenuBarConfig>(ConfigJsonSerializerContext.Default.MenuBarConfig));
            options.Converters.Add(new DiffJsonConverter<SliderConfig>(ConfigJsonSerializerContext.Default.SliderConfig));
            options.Converters.Add(new DiffJsonConverter<FilmStripConfig>(ConfigJsonSerializerContext.Default.FilmStripConfig));
            options.Converters.Add(new DiffJsonConverter<MainViewConfig>(ConfigJsonSerializerContext.Default.MainViewConfig));

            options.Converters.Add(new DiffJsonConverter<PanelsConfig>(ConfigJsonSerializerContext.Default.PanelsConfig));
            options.Converters.Add(new DiffJsonConverter<LayoutPanel.Memento>(ConfigJsonSerializerContext.Default.LayoutPanel_Memento));
            options.Converters.Add(new DiffJsonConverter<LayoutPanelWindowManager.Memento>(ConfigJsonSerializerContext.Default.LayoutPanelWindowManager_Memento));
            options.Converters.Add(new DiffJsonConverter<LayoutDockPanelContent.PanelLayout>(ConfigJsonSerializerContext.Default.PanelLayout));

            options.Converters.Add(new DiffJsonConverter<PanelListItemProfile>(ConfigJsonSerializerContext.Default.PanelListItemProfile));
            options.Converters.Add(new DiffJsonConverter<NormalItemProfile>(ConfigJsonSerializerContext.Default.NormalItemProfile));
            options.Converters.Add(new DiffJsonConverter<ContentItemProfile>(ConfigJsonSerializerContext.Default.ContentItemProfile));
            options.Converters.Add(new DiffJsonConverter<BannerItemProfile>(ConfigJsonSerializerContext.Default.BannerItemProfile));
            options.Converters.Add(new DiffJsonConverter<ThumbnailItemProfile>(ConfigJsonSerializerContext.Default.ThumbnailItemProfile));

            options.Converters.Add(new DiffJsonConverter<BookshelfConfig>(ConfigJsonSerializerContext.Default.BookshelfConfig));
            options.Converters.Add(new DiffJsonConverter<InformationConfig>(ConfigJsonSerializerContext.Default.InformationConfig));
            options.Converters.Add(new DiffJsonConverter<NavigatorConfig>(ConfigJsonSerializerContext.Default.NavigatorConfig));
            options.Converters.Add(new DiffJsonConverter<PageListConfig>(ConfigJsonSerializerContext.Default.PageListConfig));

            options.Converters.Add(new DiffJsonConverter<ThumbnailConfig>(ConfigJsonSerializerContext.Default.ThumbnailConfig));
            options.Converters.Add(new DiffJsonConverter<SlideShowConfig>(ConfigJsonSerializerContext.Default.SlideShowConfig));

            options.Converters.Add(new DiffJsonConverter<ImageEffectConfig>(ConfigJsonSerializerContext.Default.ImageEffectConfig));

            options.Converters.Add(new DiffJsonConverter<LevelEffectUnit>(ConfigJsonSerializerContext.Default.LevelEffectUnit));
            options.Converters.Add(new DiffJsonConverter<HsvEffectUnit>(ConfigJsonSerializerContext.Default.HsvEffectUnit));
            options.Converters.Add(new DiffJsonConverter<ColorSelectEffectUnit>(ConfigJsonSerializerContext.Default.ColorSelectEffectUnit));
            options.Converters.Add(new DiffJsonConverter<BlurEffectUnit>(ConfigJsonSerializerContext.Default.BlurEffectUnit));
            options.Converters.Add(new DiffJsonConverter<BloomEffectUnit>(ConfigJsonSerializerContext.Default.BloomEffectUnit));
            options.Converters.Add(new DiffJsonConverter<MonochromeEffectUnit>(ConfigJsonSerializerContext.Default.MonochromeEffectUnit));
            options.Converters.Add(new DiffJsonConverter<ColorToneEffectUnit>(ConfigJsonSerializerContext.Default.ColorToneEffectUnit));
            options.Converters.Add(new DiffJsonConverter<SharpenEffectUnit>(ConfigJsonSerializerContext.Default.SharpenEffectUnit));
            options.Converters.Add(new DiffJsonConverter<EmbossedEffectUnit>(ConfigJsonSerializerContext.Default.EmbossedEffectUnit));
            options.Converters.Add(new DiffJsonConverter<PixelateEffectUnit>(ConfigJsonSerializerContext.Default.PixelateEffectUnit));
            options.Converters.Add(new DiffJsonConverter<MagnifyEffectUnit>(ConfigJsonSerializerContext.Default.MagnifyEffectUnit));
            options.Converters.Add(new DiffJsonConverter<RippleEffectUnit>(ConfigJsonSerializerContext.Default.RippleEffectUnit));
            options.Converters.Add(new DiffJsonConverter<SwirlEffectUnit>(ConfigJsonSerializerContext.Default.SwirlEffectUnit));

            options.Converters.Add(new DiffJsonConverter<ImageCustomSizeConfig>(ConfigJsonSerializerContext.Default.ImageCustomSizeConfig));
            options.Converters.Add(new DiffJsonConverter<ImageTrimConfig>(ConfigJsonSerializerContext.Default.ImageTrimConfig));
            options.Converters.Add(new DiffJsonConverter<ImageDotKeepConfig>(ConfigJsonSerializerContext.Default.ImageDotKeepConfig));
            options.Converters.Add(new DiffJsonConverter<ImageGridConfig>(ConfigJsonSerializerContext.Default.ImageGridConfig));
            options.Converters.Add(new DiffJsonConverter<ImageResizeFilterConfig>(ConfigJsonSerializerContext.Default.ImageResizeFilterConfig));
            options.Converters.Add(new DiffJsonConverter<UnsharpMaskConfig>(ConfigJsonSerializerContext.Default.UnsharpMaskConfig));

            options.Converters.Add(new DiffJsonConverter<ViewConfig>(ConfigJsonSerializerContext.Default.ViewConfig));
            options.Converters.Add(new DiffJsonConverter<MouseConfig>(ConfigJsonSerializerContext.Default.MouseConfig));
            options.Converters.Add(new DiffJsonConverter<TouchConfig>(ConfigJsonSerializerContext.Default.TouchConfig));
            options.Converters.Add(new DiffJsonConverter<LoupeConfig>(ConfigJsonSerializerContext.Default.LoupeConfig));

            options.Converters.Add(new DiffJsonConverter<BookConfig>(ConfigJsonSerializerContext.Default.BookConfig));
            options.Converters.Add(new DiffJsonConverter<BookSettingConfig>(ConfigJsonSerializerContext.Default.BookSettingConfig));
            options.Converters.Add(new DiffJsonConverter<BookSettingPolicyConfig>(ConfigJsonSerializerContext.Default.BookSettingPolicyConfig));

            options.Converters.Add(new DiffJsonConverter<CommandConfig>(ConfigJsonSerializerContext.Default.CommandConfig));
            options.Converters.Add(new DiffJsonConverter<ScriptConfig>(ConfigJsonSerializerContext.Default.ScriptConfig));

            options.Converters.Add(new DiffJsonConverter<DestinationFolder>(ConfigJsonSerializerContext.Default.DestinationFolder));
            options.Converters.Add(new DiffJsonConverter<ExternalApp>(ConfigJsonSerializerContext.Default.ExternalApp));
            options.Converters.Add(new DiffJsonConverter<BrushSource>(ConfigJsonSerializerContext.Default.BrushSource));
            options.Converters.Add(new DiffJsonConverter<BookMemento>(ConfigJsonSerializerContext.Default.BookMemento));

            return options;
        }

        public static void Restore(UserSetting setting, bool replaceConfig = false)
        {
            if (setting == null) return;

            // コンフィグ反映
            if (setting.Config != null)
            {
                if (replaceConfig)
                {
                    Config.SetCurrent(setting.Config);
                    UserSettingFormat = setting.Format;
                }
                else
                {
                    ObjectMerge.Merge(Config.Current, setting.Config);
                }
            }

            // レイアウト反映
            CustomLayoutPanelManager.RestoreMaybe();

            // コマンド設定反映
            CommandTable.Current.RestoreCommandCollection(setting.Commands, true);

            // ドラッグアクション反映
            DragActionTable.Current.RestoreDragActionCollection(setting.DragActions);

            // コンテキストメニュー設定反映
            ContextMenuSource.Current.Restore(setting.ContextMenu);

            // SusiePlugins反映
            SusiePluginManager.Current.RestoreSusiePluginCollection(setting.SusiePlugins);
        }
    }


}
