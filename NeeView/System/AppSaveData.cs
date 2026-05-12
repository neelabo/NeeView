using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;

namespace NeeView
{
    [LocalDebug]
    public partial class AppSaveData
    {
        private static readonly Lazy<AppSaveData> _current = new();
        public static AppSaveData Current => _current.Value;


        private bool _isUserSettingsLocked;


        public static void Ready()
        {
            // 現在のブックを履歴に保存
            PageFrameBoxPresenter.Current.ForceSaveBookMemento();
            PageFrameBoxPresenter.Current.SaveLastBookMemento();

            // 現在の本棚の場所を保存
            BookshelfFolderList.Current.SaveLastFolderPath();
            BookmarkFolderList.Current.SaveLastFolderPath();

            if (App.Current.MainWindow is not null)
            {
                // メインウィンドウに対して
                // パネルレイアウトの保存
                CustomLayoutPanelManager.Current?.Store();
                CustomLayoutPanelManager.Current?.SetIsStoreEnabled(false);

                // メインビューの保存
                MainViewManager.Current?.Store();
                MainViewManager.Current?.SetIsStoreEnabled(false);

                // ウィンドウ座標の保存
                MainWindow.Current.StoreWindowPlacement();
            }
        }

        public static void SaveAll()
        {
            // 保存準備
            Ready();

            // 保存
            SaveDataSync.Current.SaveAll(false);
        }

        public FlagScope UserSettingsLockScope()
        {
            return new FlagScope(e => _isUserSettingsLocked = e);
        }

        public void LoadUserSetting()
        {
            if (_isUserSettingsLocked)
            {
                return;
            }

            if (SaveData.Current.IsLatestUserSettingFileStamp())
            {
                return;
            }

            LocalDebug.WriteLine($"UserSetting file is updated.");
            var setting = SaveData.Current.LoadUserSetting(false);
            try
            {
                Config.Current.Window.FreezeWindowState = true;
                SaveData.Current.SetUserSettingFileStamp(setting.FileStamp);
                UserSettingTools.Restore(setting);
            }
            finally
            {
                Config.Current.Window.FreezeWindowState = false;
            }
        }
    }
}
