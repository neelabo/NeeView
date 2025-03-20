using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace NeeLaboratory.Resources
{
    /// <summary>
    /// テキストリソース管理
    /// </summary>
    public class TextResourceManager
    {
        private readonly LanguageResource _languageResource;
        private TextResourceSet _resource = new();
        private TextResourceSet _instantResource = new();

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
        /// テキスト マップ
        /// </summary>
        public Dictionary<string, TextResourceItem> Map => _resource.Map;


        /// <summary>
        /// テキスト取得 インデクサ
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <returns>対応するテキスト。存在しない場合 null</returns>
        public string? this[string name]
        {
            get => GetString(name);
        }

        /// <summary>
        /// テキスト取得
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <returns>対応するテキスト。存在しない場合 null</returns>
        public string? GetString(string name)
        {
            Debug.WriteLineIf(!_resource.IsValid, $"## Resource not loaded: request key = {name}");
            return _resource.GetString(name);
        }

        /// <summary>
        /// 一時的なカルチャ指定を伴うテキスト取得
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <param name="culture">カルチャ</param>
        /// <returns></returns>
        public string? GetCultureString(string name, CultureInfo culture)
        {
            return InstantResourceSet(culture).GetString(name);
        }

        /// <summary>
        /// テキスト取得：パターン別
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <param name="pattern">パターン</param>
        /// <returns></returns>
        public string? GetCaseString(string name, string pattern)
        {
            Debug.WriteLineIf(!_resource.IsValid, $"## Resource not loaded: request key = {name}");
            return _resource.GetCaseString(name, pattern);
        }

        /// <summary>
        /// 一時的なカルチャ指定を伴うテキスト取得
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <param name="culture">カルチャ</param>
        /// <returns></returns>
        public string? GetCaseCultureString(string name, string pattern, CultureInfo culture)
        {
            return InstantResourceSet(culture).GetCaseString(name, pattern);
        }

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
        public void Add(IFileSource  fileSource)
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
        /// 一時言語リソース取得
        /// </summary>
        /// <param name="culture">カルチャ</param>
        /// <returns></returns>
        private TextResourceSet InstantResourceSet(CultureInfo culture)
        {
            var validCulture = _languageResource.ValidateCultureInfo(culture);
            if (_resource.Culture.Equals(validCulture))
            {
                return _resource;
            }
            if (_instantResource.Culture.Equals(validCulture))
            {
                return _instantResource;
            }
            _instantResource = LoadCore(validCulture);
            return _instantResource;
        }

    }
}
