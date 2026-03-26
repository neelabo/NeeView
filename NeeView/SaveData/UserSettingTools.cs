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
        private static JsonSerializerOptions? _serializeOptions;
        private static JsonSerializerOptions? _deserializeOptions;

        public static FormatVersion? UserSettingFormat { get; private set; }


        public static UserSetting CreateUserSetting(bool trim)
        {
            // 情報の確定
            MainWindow.Current.StoreWindowPlacement();
            MainViewManager.Current.Store();
            CustomLayoutPanelManager.Current.Store();
            PictureProfile.Current.StoreFileTypeToDiff();

            var settings = new UserSetting()
            {
                Format = new FormatVersion(Environment.SolutionName),
                Config = Config.Current,
                ContextMenu = ContextMenuSource.Current.CreateContextMenuNode(),
                SusiePlugins = SusiePluginManager.Current.CreateSusiePluginCollection(),
                DragActions = DragActionTable.Current.CreateDragActionCollection(trim),
                Commands = CommandTable.Current.CreateCommandCollectionMemento(trim),
            };

            if (trim)
            {
                if (settings.SusiePlugins.Count == 0)
                    settings.SusiePlugins = null;

                if (settings.DragActions.Count == 0)
                    settings.DragActions = null;

                if (settings.Commands.Count == 0)
                    settings.Commands = null;
            }

            return settings;
        }

        public static void Save(string path, string? backupFileName)
        {
            Save(path, CreateUserSetting(AppSettings.Current.TrimSaveData), backupFileName);
        }

        public static void Save(string path, UserSetting setting, string? backupFileName)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(setting, GetSerializeOptions());
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
                var boot = new BootSetting();

                using var doc = JsonDocument.Parse(bytes);

                if (!doc.RootElement.TryGetProperty("Config"u8, out var config))
                {
                    return null;
                }

                if (config.TryGetProperty("System"u8, out var system))
                {
                    if (system.TryGetProperty("Language"u8, out var language))
                    {
                        boot.Language = language.GetString() ?? "en";
                    }
                }

                if (config.TryGetProperty("StartUp"u8, out var startup))
                {
                    if (startup.TryGetProperty("IsSplashScreenEnabled"u8, out var isSplashScreenEnabled))
                    {
                        boot.IsSplashScreenEnabled = isSplashScreenEnabled.GetBoolean();
                    }

                    if (startup.TryGetProperty("IsMultiBootEnabled"u8, out var isMultiBootEnabled))
                    {
                        boot.IsMultiBootEnabled = isMultiBootEnabled.GetBoolean();
                    }
                }

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
            return JsonSerializer.Deserialize<UserSetting>(stream, GetDeserializeOptions())?.Validate();
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

        /// <summary>
        /// 設定デシリアライズ用オプション
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerOptions GetDeserializeOptions()
        {
            _deserializeOptions ??= CreateDeserializeOptions();
            return _deserializeOptions;
        }


        /// <summary>
        /// 設定シリアライズ用オプション
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerOptions GetSerializeOptions()
        {
            _serializeOptions ??= CreateSerializeOptions();
            return _serializeOptions;
        }

        public static JsonSerializerOptions CreateCommonSerializerOptions()
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

            return options;
        }

        private static JsonSerializerOptions CreateDeserializeOptions()
        {
            var options = CreateCommonSerializerOptions();

            return options;
        }

        private static JsonSerializerOptions CreateSerializeOptions()
        {
            var options = CreateCommonSerializerOptions();

            if (!AppSettings.Current.TrimSaveData)
            {
                return options;
            }

            options.Converters.Add(new DiffJsonConverter<Config>());

            options.Converters.Add(new DiffJsonConverter<SystemConfig>());
            options.Converters.Add(new DiffJsonConverter<StartUpConfig>());
            options.Converters.Add(new DiffJsonConverter<PerformanceConfig>());

            options.Converters.Add(new DiffJsonConverter<ImageConfig>());
            options.Converters.Add(new DiffJsonConverter<ImageStandardConfig>());
            options.Converters.Add(new DiffJsonConverter<ImageSvgConfig>());

            options.Converters.Add(new DiffJsonConverter<ArchiveConfig>());
            options.Converters.Add(new DiffJsonConverter<ZipArchiveConfig>());
            options.Converters.Add(new DiffJsonConverter<SevenZipArchiveConfig>());
            options.Converters.Add(new DiffJsonConverter<PdfArchiveConfig>());
            options.Converters.Add(new DiffJsonConverter<MediaArchiveConfig>());

            options.Converters.Add(new DiffJsonConverter<SusieConfig>());

            options.Converters.Add(new DiffJsonConverter<HistoryConfig>());
            options.Converters.Add(new DiffJsonConverter<PageViewRecorderConfig>());
            options.Converters.Add(new DiffJsonConverter<BookmarkConfig>());
            options.Converters.Add(new DiffJsonConverter<PlaylistConfig>());

            options.Converters.Add(new DiffJsonConverter<WindowConfig>());
            options.Converters.Add(new DiffJsonConverter<ThemeConfig>());
            options.Converters.Add(new DiffJsonConverter<FontsConfig>());
            options.Converters.Add(new DiffJsonConverter<BackgroundConfig>());
            options.Converters.Add(new DiffJsonConverter<WindowTitleConfig>());
            options.Converters.Add(new DiffJsonConverter<PageTitleConfig>());
            options.Converters.Add(new DiffJsonConverter<AutoHideConfig>());
            options.Converters.Add(new DiffJsonConverter<NoticeConfig>());

            options.Converters.Add(new DiffJsonConverter<MenuBarConfig>());
            options.Converters.Add(new DiffJsonConverter<SliderConfig>());
            options.Converters.Add(new DiffJsonConverter<FilmStripConfig>());
            options.Converters.Add(new DiffJsonConverter<MainViewConfig>());

            options.Converters.Add(new DiffJsonConverter<PanelsConfig>());
            options.Converters.Add(new DiffJsonConverter<LayoutPanelMemento>());
            options.Converters.Add(new DiffJsonConverter<LayoutPanelManagerMemento>());
            options.Converters.Add(new DiffJsonConverter<LayoutPanelWindowManagerMemento>());

            options.Converters.Add(new DiffJsonConverter<PanelListItemProfile>());
            options.Converters.Add(new DiffJsonConverter<NormalItemProfile>());
            options.Converters.Add(new DiffJsonConverter<ContentItemProfile>());
            options.Converters.Add(new DiffJsonConverter<BannerItemProfile>());
            options.Converters.Add(new DiffJsonConverter<ThumbnailItemProfile>());

            options.Converters.Add(new DiffJsonConverter<BookshelfConfig>());
            options.Converters.Add(new DiffJsonConverter<InformationConfig>());
            options.Converters.Add(new DiffJsonConverter<NavigatorConfig>());
            options.Converters.Add(new DiffJsonConverter<PageListConfig>());

            options.Converters.Add(new DiffJsonConverter<ThumbnailConfig>());
            options.Converters.Add(new DiffJsonConverter<SlideShowConfig>());

            options.Converters.Add(new DiffJsonConverter<ImageEffectConfig>());

            options.Converters.Add(new DiffJsonConverter<LevelEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<HsvEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<ColorSelectEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<BlurEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<BloomEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<MonochromeEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<ColorToneEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<SharpenEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<EmbossedEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<PixelateEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<MagnifyEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<RippleEffectUnit>());
            options.Converters.Add(new DiffJsonConverter<SwirlEffectUnit>());

            options.Converters.Add(new DiffJsonConverter<ImageCustomSizeConfig>());
            options.Converters.Add(new DiffJsonConverter<ImageTrimConfig>());
            options.Converters.Add(new DiffJsonConverter<ImageDotKeepConfig>());
            options.Converters.Add(new DiffJsonConverter<ImageGridConfig>());
            options.Converters.Add(new DiffJsonConverter<ImageResizeFilterConfig>());
            options.Converters.Add(new DiffJsonConverter<UnsharpMaskConfig>());

            options.Converters.Add(new DiffJsonConverter<ViewConfig>());
            options.Converters.Add(new DiffJsonConverter<MouseConfig>());
            options.Converters.Add(new DiffJsonConverter<TouchConfig>());
            options.Converters.Add(new DiffJsonConverter<LoupeConfig>());

            options.Converters.Add(new DiffJsonConverter<BookConfig>());
            options.Converters.Add(new DiffJsonConverter<BookSettingConfig>());
            options.Converters.Add(new DiffJsonConverter<BookSettingPolicyConfig>());

            options.Converters.Add(new DiffJsonConverter<CommandConfig>());
            options.Converters.Add(new DiffJsonConverter<ScriptConfig>());

            options.Converters.Add(new DiffJsonConverter<PrintModelMemento>());

            options.Converters.Add(new DiffJsonConverter<DestinationFolder>());
            options.Converters.Add(new DiffJsonConverter<ExternalApp>());
            options.Converters.Add(new DiffJsonConverter<BrushSource>());

            options.Converters.Add(new DiffJsonConverter<MenuNode>());
            options.Converters.Add(new DiffJsonConverter<SusiePluginMemento>());
            options.Converters.Add(new DiffJsonConverter<CommandElementMemento>());

            options.Converters.Add(new DiffJsonConverter<MoveScaleDragActionParameter>());
            options.Converters.Add(new DiffJsonConverter<MoveDragActionParameter>());
            options.Converters.Add(new DiffJsonConverter<SensitiveDragActionParameter>());

            options.Converters.Add(new DiffJsonConverter<ReversibleCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<MoveSizePageCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<TogglePageModeCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ToggleStretchModeCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<StretchModeCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ViewScrollCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ViewPresetScrollCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ViewScaleCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ViewRotateCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<MovePlaylistItemInBookCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ScrollPageCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<FocusMainViewCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ExportImageAsCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ExportImageCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<OpenExternalAppCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<OpenExternalAppAsCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<OpenBookExternalAppAsCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<CopyFileCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ViewScrollNTypeCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ScriptCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ImportBackupCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ExportBackupCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<CopyToFolderAsCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<MoveToFolderAsCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<CopyBookToFolderAsCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<MoveBookToFolderAsCommandParameter>());
            options.Converters.Add(new DiffJsonConverter<ToggleBookmarkCommandParameter>());
          
            return options;
        }
    }
}
