using NeeLaboratory.IO.Search;
using System;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// 検索ボックス コンポーネント
    /// </summary>
    public interface ISearchBoxComponent
    {
        /// <summary>
        /// 検索履歴
        /// </summary>
        HistoryStringCollection? History { get; }

        /// <summary>
        /// インクリメンタルサーチ フラグ
        /// </summary>
        bool IsIncrementalSearchEnabled { get; }

        /// <summary>
        /// 検索実行
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        void Search(string keyword);

        /// <summary>
        /// 検索キーワード解析
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        SearchKeywordAnalyzeResult Analyze(string keyword);
    }


    public readonly record struct SearchKeywordAnalyzeResult(List<SearchKey> Keys, Exception? Exception)
    {
        public SearchKeywordAnalyzeResult(IEnumerable<SearchKey> keys) : this(new List<SearchKey>(keys), null)
        {
        }

        public SearchKeywordAnalyzeResult(Exception exception) : this(new List<SearchKey>(), exception)
        {
        }

        public bool IsSuccess => Exception is null;
    }

}
