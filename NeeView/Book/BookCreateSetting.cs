namespace NeeView
{
    /// <summary>
    /// Book生成設定
    /// </summary>
    public class BookCreateSetting
    {
        /// <summary>
        /// 開始ページ
        /// </summary>
        public BookStartPage StartPage { get; set; } = new BookStartPage(BookStartPageType.FirstPage);

        /// <summary>
        /// フォルダー再帰
        /// </summary>
        public bool IsRecursiveFolder { get; set; }

        /// <summary>
        /// 圧縮ファイルの再帰モード
        /// </summary>
        public ArchiveEntryCollectionMode ArchiveRecursiveMode { get; set; }

        /// <summary>
        /// ページ収集モード
        /// </summary>
        public BookPageCollectMode BookPageCollectMode { get; set; }

        /// <summary>
        /// ページの並び順
        /// </summary>
        public PageSortMode SortMode { get; set; }

        /// <summary>
        /// ページの並び順用 乱数シード
        /// </summary>
        public int SortSeed { get; set; }

        /// <summary>
        /// キャッシュ無効
        /// </summary>
        public bool IsIgnoreCache { get; set; }

        /// <summary>
        /// 新規ブック
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// ロード設定フラグ
        /// </summary>
        public BookLoadOption LoadOption { get; set; }

        /// <summary>
        /// アーカイバ指定
        /// </summary>
        public ArchiveHint ArchiveHint { get; set; } = ArchiveHint.None;

        /// <summary>
        /// 保護されたアーカイブの復号化
        /// </summary>
        public bool Decrypt { get; set; } = true;
    }

}
