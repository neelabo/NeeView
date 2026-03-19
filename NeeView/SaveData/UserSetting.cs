using System.Text.Json.Serialization;

namespace NeeView
{
    [Memento]
    public class UserSetting
    {
        public FormatVersion? Format { get; set; }

        public Config? Config { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MenuNode? ContextMenu { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SusiePluginCollection? SusiePlugins { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DragActionCollection? DragActions { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
