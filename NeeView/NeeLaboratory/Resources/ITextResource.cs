namespace NeeLaboratory.Resources
{
    public interface ITextResource
    {
        /// <summary>
        /// テキスト取得
        /// </summary>
        /// <param name="key">リソース キー</param>
        /// <returns></returns>
        TextResourceString? GetResourceString(string key);

        /// <summary>
        /// テキスト取得 (パターン)
        /// </summary>
        /// <remarks>
        /// フォーマットストリングで使用されます。
        /// 例えば、引数の数値をパターンとし、複数形表現によってテキストを切り替えます。
        /// </remarks>
        /// <param name="key">リソース キー</param>
        /// <param name="pattern">パターン</param>
        /// <returns></returns>
        TextResourceString? GetCaseResourceString(string key, string pattern);
    }
}
