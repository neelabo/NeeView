using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NeeLaboratory.Resources
{
    /// <summary>
    /// 言語リソース
    /// </summary>
    public class LanguageResource
    {
        protected const string _ext = ".restext";

        private readonly CultureInfo _defaultCulture;
        private List<CultureInfo> _cultures = new();


        public LanguageResource()
        {
            _defaultCulture = CultureInfo.GetCultureInfo("en");
        }


        /// <summary>
        /// 既定のカルチャ
        /// </summary>
        public CultureInfo DefaultCulture => _defaultCulture;

        /// <summary>
        /// 選択可能なカルチャリスト
        /// </summary>
        public IReadOnlyList<CultureInfo> Cultures => _cultures;


        public void Clear()
        {
            _cultures = new();
        }

        public void AddCulture(CultureInfo culture)
        {
            if (_cultures.Contains(culture)) return;
            _cultures.Add(culture);
        }

        public void SetCultures(IEnumerable<CultureInfo> cultures)
        {
            _cultures = cultures.ToList();
        }

        /// <summary>
        /// リソースが存在するカルチャに変換する
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public CultureInfo ValidateCultureInfo(CultureInfo culture)
        {
            if (culture.Equals(CultureInfo.InvariantCulture)) return _defaultCulture;

            var result = _cultures?.FirstOrDefault(e => culture.Equals(e));
            return result ?? ValidateCultureInfo(culture.Parent);
        }

        /// <summary>
        /// カルチャからリソースファイル名を作成する
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        protected string CreateResTextFileName(CultureInfo culture)
        {
            return culture.Name + _ext;
        }

        /// <summary>
        /// カルチャからファイルソースを作成する
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual IFileSource CreateFileSource(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
