namespace NeeView
{
    public interface IExternalApp
    {
        /// <summary>
        /// コマンド
        /// </summary>
        string? Command { get; set; }

        /// <summary>
        /// コマンドパラメーター
        /// {File} = 渡されるファイルパス
        /// </summary>
        string Parameter { get; set; }

        /// <summary>
        /// 作業フォルダー
        /// </summary>
        string? WorkingDirectory { get; set; }

        /// <summary>
        /// 圧縮ファイルのときの動作
        /// </summary>
        ArchivePolicy ArchivePolicy { get; set; }
    }

}
