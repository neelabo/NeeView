using NeeLaboratory.Generators;
using NeeView.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    /// <summary>
    /// プロパティで構成されたアクセスマップ
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PropertyMap : PropertyMapNode, INotifyPropertyChanged, IEnumerable<KeyValuePair<string, PropertyMapNode>>
    {
        private static readonly PropertyMapConverter _defaultConverter;
        private static readonly PropertyMapOptions _defaultOptions;

        static PropertyMap()
        {
            _defaultConverter = new PropertyMapDefaultConverter();

            _defaultOptions = new PropertyMapOptions();
            _defaultOptions.Converters.Add(new PropertyMapEnumConverter());
            _defaultOptions.Converters.Add(new PropertyMapSizeConverter());
            _defaultOptions.Converters.Add(new PropertyMapPointConverter());
            _defaultOptions.Converters.Add(new PropertyMapColorConverter());
            _defaultOptions.Converters.Add(new PropertyMapFileTypeCollectionConverter());
            _defaultOptions.Converters.Add(new PropertyMapStringCollectionConverter());
        }


        private readonly object _source;
        private readonly Dictionary<string, PropertyMapNode> _items;
        private readonly PropertyMapOptions _options;
        private readonly IAccessDiagnostics _accessDiagnostics;

        public PropertyMap(string name, object source, IAccessDiagnostics? accessDiagnostics)
            : this(name, null, null, source, accessDiagnostics, "", null)
        {
        }

        public PropertyMap(string name, ObsoleteAttribute? obsolete, AlternativeAttribute? alternative, object source, IAccessDiagnostics? accessDiagnostics, string label, PropertyMapOptions? options)
            : base(name, obsolete, alternative)
        {
            _source = source;
            _accessDiagnostics = accessDiagnostics ?? new DefaultAccessDiagnostics();
            _options = options ?? _defaultOptions;

            var type = _source.GetType();

            _items = new Dictionary<string, PropertyMapNode>();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(e => e.Name))
            {
                if (property.GetCustomAttribute(typeof(PropertyMapIgnoreAttribute)) != null) continue;

                var nameAttribute = (PropertyMapNameAttribute?)property.GetCustomAttribute(typeof(PropertyMapNameAttribute));
                var key = nameAttribute?.Name ?? property.Name;
                var converter = _options.Converters.FirstOrDefault(e => e.CanConvert(property.PropertyType));
                var itemName = name + "." + key;

                var obsoleteAttribute = (ObsoleteAttribute?)property.GetCustomAttribute(typeof(ObsoleteAttribute));
                var alternativeAttribute = property.GetCustomAttribute<AlternativeAttribute>();

                if (converter == null && property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var propertyValue = property.GetValue(_source) ?? throw new InvalidOperationException();
                    var labelAttribute = (PropertyMapLabelAttribute?)property.GetCustomAttribute(typeof(PropertyMapLabelAttribute));
                    var newLabel = labelAttribute != null ? label + TextResources.GetString(labelAttribute.Label) + ": " : label;
                    _items.Add(key, new PropertyMap(itemName, obsoleteAttribute, alternativeAttribute, propertyValue, _accessDiagnostics, newLabel, options));
                }
                else
                {
                    _items.Add(key, new PropertyMapSource(itemName, obsoleteAttribute, alternativeAttribute, source, property, converter ?? _defaultConverter, label));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public object? this[string key]
        {
            get { return GetValue(_items[key]); }
            set { SetValue(_items[key], value); RaisePropertyChanged(key); }
        }

        internal bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        internal PropertyMapNode GetNode(string key)
        {
            return _items[key];
        }

        internal object? GetValue(PropertyMapNode node)
        {
            if (node.IsObsolete)
            {
                var errorLevel = node.Alternative?.ErrorLevel ?? ScriptErrorLevel.Error;
                _accessDiagnostics.Throw(new NotSupportedException(node.CreateObsoleteMessage()), errorLevel);
            }

            if (node is PropertyMapSource source)
            {
                return AppDispatcher.Invoke(() => source.Read(_options));
            }
            else
            {
                return node;
            }
        }

        internal void SetValue(PropertyMapNode node, object? value)
        {
            if (node.IsObsolete)
            {
                var errorLevel = node.Alternative?.ErrorLevel ?? ScriptErrorLevel.Error;
                _accessDiagnostics.Throw(new NotSupportedException(node.CreateObsoleteMessage()), errorLevel);
            }

            if (node is PropertyMapSource source)
            {
                AppDispatcher.Invoke(() => source.Write(value, _options));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }



        /// <summary>
        /// 外部からのプロパティの追加
        /// </summary>
        internal void AddProperty(object source, string propertyName, string? memberName = null)
        {
            var type = source.GetType();
            var property = type.GetProperty(propertyName);
            if (property is null) throw new ArgumentException("not support property name", nameof(propertyName));
            var converter = _options.Converters.FirstOrDefault(e => e.CanConvert(property.PropertyType)) ?? _defaultConverter;
            var key = memberName ?? propertyName;
            _items.Add(key, new PropertyMapSource(Name + "." + key, null, null, source, property, converter, null));
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = new WordNode(name);
            if (_items.Any())
            {
                node.Children = new List<WordNode>();
                foreach (var item in _items)
                {
                    if (item.Value.IsObsolete) continue;

                    switch (item.Value)
                    {
                        case PropertyMap propertyMap:
                            node.Children.Add(propertyMap.CreateWordNode(item.Key));
                            break;

                        default:
                            node.Children.Add(new WordNode(item.Key));
                            break;
                    }
                }
            }
            return node;
        }

        internal string CreateHelpHtml(string prefix)
        {
            string s = "";

            foreach (var item in _items)
            {
                var name = prefix + "." + item.Key;
                if (item.Value.IsObsolete)
                {
                }
                else if (item.Value is PropertyMap subMap)
                {
                    s += subMap.CreateHelpHtml(name);
                }
                else
                {
                    string type = "";
                    string description = "";
                    if (item.Value is PropertyMapSource valueItem)
                    {
                        (type, description) = valueItem.CreateHelpHtml();
                    }
                    s += $"<tr><td>{name}</td><td>{type}</td><td>{description}</td></tr>\r\n";
                }
            }

            return s;
        }

        public IEnumerator<KeyValuePair<string, PropertyMapNode>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
