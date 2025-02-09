using System;

namespace NeeView
{
    /// <summary>
    /// サムネイルキャッシュ：ヘッダ
    /// </summary>
    public class ThumbnailCacheHeader
    {
        public static ThumbnailCacheHeader None { get; } = new("", 0, null, 0);


        public ThumbnailCacheHeader(string name, long length, string? appendix, int generateHash)
        {
            Key = appendix != null ? name + ":" + appendix : name;
            Size = length;
            AccessTime = DateTime.Now;
            GenerateHash = generateHash;
        }

        /// <summary>
        /// キャッシュのキー(ファイルパス)
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// アクセス日付
        /// </summary>
        public DateTime AccessTime { get; private set; }

        /// <summary>
        /// サムネイル画像生成パラメータ一致チェック用ハッシュ
        /// </summary>
        public int GenerateHash { get; private set; }

        /// <summary>
        /// 有効判定
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Key);
        }
    }
}
