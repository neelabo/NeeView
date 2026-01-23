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

        public static void Save(string path)
        {
            Save(path, CreateUserSetting(AppSettings.Current.TrimSaveData));
        }

        public static void Save(string path, UserSetting setting)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(setting, GetSerializerOptions());
            File.WriteAllBytes(path, json);
        }

        public static byte[] LoadBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public static UserSetting? Load(string path)
        {
            using var stream = File.OpenRead(path);
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
