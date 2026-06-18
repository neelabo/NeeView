using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class RegexCache
    {
        private string? _source;
        private Regex? _regex;

        public RegexCache(RegexOptions options)
        {
            Options = options;
        }

        public RegexOptions Options { get; }

        public Regex? GetRegex(string source)
        {
            if (_regex is null || source != _source)
            {
                _source = source;
                _regex = ToRegex(_source, Options);
            }
            return _regex;
        }

        private static Regex? ToRegex(string pattern, RegexOptions options)
        {
            if (string.IsNullOrEmpty(pattern)) return null;

            try
            {
                return new Regex(pattern, options | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Invalid regex pattern: {pattern}", ex);
                return null;
            }
        }
    }
}
