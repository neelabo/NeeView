using NeeView;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace NeeLaboratory.Resources
{
    /// <summary>
    /// テキストリソース
    /// </summary>
    public class TextResourceSet
    {
        private readonly CultureInfo _culture;
        private readonly Dictionary<string, TextResourceItem> _map;


        public TextResourceSet()
        {
            _culture = CultureInfo.InvariantCulture;
            _map = new();
        }

        public TextResourceSet(CultureInfo culture, Dictionary<string, TextResourceItem> map)
        {
            _culture = culture;
            _map = map;
        }


        public CultureInfo Culture => _culture;

        public Dictionary<string, TextResourceItem> Map => _map;

        public bool IsValid => !_culture.Equals(CultureInfo.InvariantCulture);


        public string? this[string name]
        {
            get { return GetString(name); }
        }

        public string? GetString(string name)
        {
            return _map.TryGetValue(name, out var value) ? value.Text : null;
        }

        public string? GetCaseString(string name, string pattern)
        {
            return _map.TryGetValue(name, out var value) ? value.GetCaseText(pattern) : null;
        }

        public void Add(Dictionary<string, TextResourceItem> map)
        {
            foreach (var item in map)
            {
#if DEBUG
                // 参照でないものは要注意
                if (item.Value.Text[0] != '@')
                {
                    Debug.WriteLine($"Warning: {item.Key}={item.Value.Text}");
                }
#endif

                // すでに存在している場合はそのまま
                if (_map.ContainsKey(item.Key))
                {
                    Debug.WriteLine($"TextResource: {item.Key} already exists.");
                    continue;
                }

                _map[item.Key] = item.Value;

                // 単純な置き換えの場合はここで置き換える
                if (item.Value.Text[0] == '@' && item.Value.Text[1] != '[')
                {
                    var sourceKey = item.Value.Text[1..];
                    Debug.Assert(_map.ContainsKey(sourceKey));
                    _map[item.Key] = _map.TryGetValue(sourceKey, out var value) ? value : item.Value;
                }
                else
                {
                    _map[item.Key] = item.Value;
                }
            }
        }

        public void SetItem(string key, string text)
        {
            _map[key] = new TextResourceItem(text);
        }
    }
}
