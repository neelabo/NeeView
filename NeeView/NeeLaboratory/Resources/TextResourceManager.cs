using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace NeeLaboratory.Resources
{
    /// <summary>
    /// テキストリソース管理
    /// </summary>
    public class TextResourceManager : ITextResource
    {
        private readonly LanguageResource _languageResource;
        private TextResourceSet _resource = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="directory">言語フォルダ</param>
        public TextResourceManager(LanguageResource languageResource)
        {
            _languageResource = languageResource;
        }


        /// <summary>
        /// 言語リソース
        /// </summary>
        public LanguageResource LanguageResource => _languageResource ?? throw new InvalidOperationException();

        /// <summary>
        /// 言語カルチャ
        /// </summary>
        public CultureInfo Culture => _resource.Culture;

        /// <summary>
        /// テキストリソース
        /// </summary>
        public TextResourceSet Resource => _resource;

        /// <summary>
        /// テキスト マップ
        /// </summary>
        public Dictionary<string, TextResourceItem> Map => _resource.Map;


        /// <summary>
        /// 言語リソース読み込み
        /// </summary>
        /// <param name="culture">カルチャ</param>
        public void Load(CultureInfo culture)
        {
            _resource = LoadCore(_languageResource.ValidateCultureInfo(culture));
        }

        private TextResourceSet LoadCore(CultureInfo culture)
        {
            return new TextResourceFactory(_languageResource).Load(culture);
        }

        /// <summary>
        /// 言語リソース追加読み込み
        /// </summary>
        /// <param name="fileSource"></param>
        public void Add(IFileSource fileSource)
        {
            _resource.Add(TextResourceFactory.LoadResText(fileSource));
        }

        /// <summary>
        /// 項目設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        public void SetItem(string key, string text)
        {
            _resource.SetItem(key, text);
        }

        /// <summary>
        /// テキスト取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TextResourceString? GetResourceString(string key)
        {
            return _resource.GetResourceString(key);
        }

        /// <summary>
        /// テキスト取得 (パターン依存)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public TextResourceString? GetCaseResourceString(string key, string pattern)
        {
            return _resource.GetCaseResourceString(key, pattern);
        }

        [Conditional("DEBUG")]
        public void DumpNoUsed()
        {
            _resource.DumpNoUsed();
        }

    }
}
