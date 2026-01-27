using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeeView.Text
{
    public class ReplaceStringChangedEventArgs : EventArgs
    {
        public string Key { get; private set; }

        public ReplaceStringChangedEventArgs(string key)
        {
            Key = key;
        }
    }


    /// <summary>
    /// キーワード置換
    /// </summary>
    public class ReplaceString
    {
        private class ReplaceStringUnit
        {
            public Regex Regex { get; private set; }
            public string ReplaceString { get; set; }

            public ReplaceStringUnit(string key, string replaceString)
            {
                Regex = new Regex(Regex.Escape(key) + "\\b");
                ReplaceString = replaceString;
            }

            public string Replace(string s)
            {
                return Regex.Replace(s, ReplaceString);
            }

            public override string? ToString()
            {
                return Regex?.ToString() ?? base.ToString();
            }
        }


        private readonly Dictionary<string, ReplaceStringUnit> _map;

        public ReplaceString()
        {
            _map = new Dictionary<string, ReplaceStringUnit>();
        }


        public event EventHandler<ReplaceStringChangedEventArgs>? Changed;


        public void Set(string key, string replaceString)
        {
            if (_map.TryGetValue(key, out var value))
            {
                if (value.ReplaceString != replaceString)
                {
                    value.ReplaceString = replaceString;
                    Changed?.Invoke(this, new ReplaceStringChangedEventArgs(key));
                }
            }
            else
            {
                _map[key] = new ReplaceStringUnit(key, replaceString);
                Changed?.Invoke(this, new ReplaceStringChangedEventArgs(key));
            }
        }

        public bool Remove(string key)
        {
            return _map.Remove(key);
        }

        public string Replace(string src)
        {
            return Replace(src, _map.Keys);
        }

        public string Replace(string src, IEnumerable<string> keys)
        {
            if (string.IsNullOrEmpty(src) || keys is null)
            {
                return src;
            }

            var s = src;
            foreach (var key in keys)
            {
                if (_map.TryGetValue(key, out var value))
                {
                    s = value.Replace(s);
                }
            }
            return s;
        }
    }

}
