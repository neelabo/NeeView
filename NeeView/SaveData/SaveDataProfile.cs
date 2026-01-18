using System.IO;


namespace NeeView
{
    public static class SaveDataProfile
    {
        public const string UserSettingFileName = "UserSetting.json";
        public const string HistoryFileName = "History.json";
        public const string BookmarkFileName = "Bookmark.json";
        public const string PagemarkFileName = "Pagemark.json";
        public const string FolderConfigFileName = "Foldres.json";
        public const string CustomThemeFolder = "Themes";
        public const string PlaylistsFolder = "Playlists";
        public const string ScriptsFolder = "Scripts";

        public static string DefaultHistoryFilePath => Path.Combine(Environment.LocalApplicationDataPath, HistoryFileName);
        public static string DefaultBookmarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, BookmarkFileName);
        public static string DefaultPagemarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, PagemarkFileName);
        public static string DefaultFolderConfigFilePath => Path.Combine(Environment.LocalApplicationDataPath, FolderConfigFileName);
        public static string DefaultCustomThemeFolder => Environment.GetUserDataPath(CustomThemeFolder);
        public static string DefaultPlaylistsFolder => Environment.GetUserDataPath(PlaylistsFolder);
        public static string DefaultScriptsFolder => Environment.GetUserDataPath(ScriptsFolder);
    }
}
