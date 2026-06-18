using System;
using System.Text.RegularExpressions;

namespace NeeLaboratory.Text
{
    public interface IStringMatch
    {
        bool IsMatch(string value);
    }


    public class DefaultStringMatch : IStringMatch
    {
        private readonly string _target;
        private readonly StringComparison _comparisonType;

        public DefaultStringMatch(string target, StringComparison comparisonType = StringComparison.Ordinal)
        {
            _target = target;
            _comparisonType = comparisonType;
        }

        public bool IsMatch(string value)
        {
            return string.Equals(value, _target, _comparisonType);
        }
    }


    public class RegexStringMatch : IStringMatch
    {
        private readonly Regex _regex;

        public RegexStringMatch(string pattern, RegexOptions options = RegexOptions.None)
        {
            _regex = new Regex(pattern, options | RegexOptions.Compiled);
        }

        public RegexStringMatch(Regex regex)
        {
            _regex = regex;
        }

        public bool IsMatch(string value)
        {
            if (value is null) return false;
            return _regex.IsMatch(value);
        }
    }

}
