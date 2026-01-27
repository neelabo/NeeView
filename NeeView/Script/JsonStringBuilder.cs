using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// object to json string builder
    /// NeeView.Jint special
    /// </summary>
    public class JsonStringBuilder
    {
        private static readonly int _limitDepth = 0;

        private readonly IndentStringBuilder _builder = new();


        public JsonStringBuilder()
        {
        }

        public JsonStringBuilder(object source)
        {
            AppendObject(source);
        }


        public override string ToString()
        {
            return _builder.ToString();
        }

        public JsonStringBuilder AppendObject(object? source)
        {
            AppendObject(_builder, source, 0);
            return this;
        }

        private IndentStringBuilder AppendObject(IndentStringBuilder builder, object? source, int depth)
        {
            if (source is null)
            {
                return builder.Append("null");
            }

            var type = source.GetType();

            var obsolete = type.GetCustomAttribute<ObsoleteAttribute>();
            if (obsolete != null)
            {
                return builder.Append("This is obsolete.");
            }

            if (source is Enum enm)
            {
                return builder.Append(Convert.ToInt32(enm, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
            }
            else if (source is bool boolean)
            {
                return builder.Append(boolean ? "true" : "false");
            }
            else if (source is string str)
            {
                return builder.Append(_builder.Indent > 0 ? "\"" + JavaScriptStringEncode(str) + "\"" : str);
            }
            else if (source is object[] objects)
            {
                return AppendCollection(builder, objects, depth);
            }
            else if (source is IDictionary<string, object> genericDictionary) // for ExpandoObject
            {
                return AppendGenericDictionary(builder, genericDictionary, depth);
            }
            else if (source is IDictionary dictionary)
            {
                return AppendDictionary(builder, dictionary, depth);
            }
            else if (source is IList && source is IEnumerable collection)
            {
                return AppendCollection(builder, collection, depth);
            }
            else if (source is PropertyMap propertyMap)
            {
                return AppendDictionary(builder, propertyMap.Where(e => !e.Value.IsObsolete).ToDictionary(e => e.Key, e => propertyMap.GetValue(e.Value)), depth);
            }
            else if (source is CommandAccessorMap commandMap)
            {
                return AppendDictionary(builder, commandMap.Where(e => e.Value is not ObsoleteCommandAccessor).ToDictionary(e => e.Key, e => e.Value), depth);
            }
            else if (type.IsClass && !IsDelegate(type))
            {
                try
                {
                    var dic = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .OrderBy(e => e.Name)
                        .Where(e => e.CanRead && e.GetCustomAttribute<ObsoleteAttribute>() is null)
                        .ToDictionary(e => e.Name, e => e.GetValue(source));
                    return AppendDictionary(builder, dic, depth);
                }
                catch
                {
                }
            }

            return builder.Append(source?.ToString());

        }

        private static bool IsDelegate(Type type)
        {
            return type.IsSubclassOf(typeof(Delegate)) || type.Equals(typeof(Delegate));
        }


        private static string JavaScriptStringEncode(string src)
        {
            var builder = new StringBuilder();
            foreach (var c in src)
            {
                switch (c)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\"':
                        builder.Append("\\\"");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }

        private IndentStringBuilder AppendCollection(IndentStringBuilder builder, IEnumerable source, int depth)
        {
            var section = new CollectionSection(builder, '[', ']');

            if (depth > _limitDepth)
            {
                section.Omit();
                return builder;
            }

            section.Open();

            foreach (var item in source)
            {
                section.Increment();
                AppendObject(builder, item, depth);
            }

            section.Close();

            return builder;
        }


        private IndentStringBuilder AppendGenericDictionary(IndentStringBuilder builder, IDictionary<string, object> source, int depth)
        {
            var section = new CollectionSection(builder, '{', '}');

            if (depth > _limitDepth)
            {
                section.Omit();
                return builder;
            }

            section.Open();

            foreach (var item in source)
            {
                section.Increment();
                AppendKeyValuePair(builder, item, depth);
            }

            section.Close();

            return builder;
        }


        IndentStringBuilder AppendKeyValuePair(IndentStringBuilder builder, KeyValuePair<string, object> source, int depth)
        {
            builder.Append("\"" + source.Key + "\": ");
            AppendObject(builder, source.Value, depth + 1);
            return builder;
        }


        private IndentStringBuilder AppendDictionary(IndentStringBuilder builder, IDictionary source, int depth)
        {
            var section = new CollectionSection(builder, '{', '}');

            if (depth > _limitDepth)
            {
                section.Omit();
                return builder;
            }

            section.Open();

            foreach (DictionaryEntry item in source)
            {
                var valueType = item.Value?.GetType();
                if (valueType != null)
                {
                    if (valueType.GetCustomAttribute<ObsoleteAttribute>() != null)
                    {
                        continue;
                    }
                }

                section.Increment();
                AppendKeyValuePair(builder, item, depth);
            }

            section.Close();

            return builder;
        }

        private IndentStringBuilder AppendKeyValuePair(IndentStringBuilder builder, DictionaryEntry source, int depth)
        {
            builder.Append("\"" + source.Key + "\": ");
            AppendObject(builder, source.Value, depth + 1);
            return builder;
        }


        private class CollectionSection
        {
            private readonly IndentStringBuilder _builder;
            private readonly char _openChar;
            private readonly char _closeChar;
            private int _count;

            public CollectionSection(IndentStringBuilder builder, char openChar, char closeChar)
            {
                _builder = builder;
                _openChar = openChar;
                _closeChar = closeChar;
            }

            public void Omit()
            {
                _builder.Append($"{_openChar}…{_closeChar}");
            }

            public void Open()
            {
                _builder.Append(_openChar.ToString());
            }

            public void Close()
            {
                if (_count != 0)
                {
                    _builder.IndentDown();
                }

                _builder.Append(_closeChar.ToString());
            }

            public void Increment()
            {
                if (_count == 0)
                {
                    _builder.IndentUp();
                }
                else
                {
                    _builder.Append(",").AppendLine();
                }
                _count++;
            }
        }
    }
}
