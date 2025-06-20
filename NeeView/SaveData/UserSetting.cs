using System.Text.Json.Serialization;

namespace NeeView
{
    [Memento]
    public class UserSetting
    {
        public FormatVersion? Format { get; set; }

        public Config? Config { get; set; }

        public MenuNode? ContextMenu { get; set; }

        public SusiePluginCollection? SusiePlugins { get; set; }

        public DragActionCollection? DragActions { get; set; }

        public CommandCollection? Commands { get; set; }

        /// <summary>
        /// 再読み込みチェック用ファイルスタンプ
        /// </summary>
        [JsonIgnore]
        public FileStamp? FileStamp { get; private set; }


        public UserSetting EnsureConfig()
        {
            if (Config is null)
            {
                Config = new Config();
            }
            return this;
        }

        public void SetFileStamp(FileStamp? fileStamp)
        {
            FileStamp = fileStamp;
        }
    }

}
