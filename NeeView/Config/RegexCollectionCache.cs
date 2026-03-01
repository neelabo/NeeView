using NeeLaboratory.Linq;
using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeView
{
    /// <summary>
    /// StringCollectionで定義された正規表現の正規表現インスタンス キャッシュ
    /// </summary>
    public class RegexCollectionCache
    {
        private List<Regex> _regexs = new();
        private StringCollection _sourceCache = new();

        public List<Regex> GetRegexs(StringCollection source)
        {
            if (!source.Equals(_sourceCache))
            {
                _sourceCache = (StringCollection)source.Clone();
                _regexs = _sourceCache.Items.Select(e => ToRegex(e)).WhereNotNull().ToList();
            }
            return _regexs;
        }

        private static Regex? ToRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return null;

            try
            {
                return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Invalid regex pattern: {pattern}", ex);
                return null;
            }
        }
    }
}
