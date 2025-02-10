using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeeView.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionBaseAttribute : Attribute
    {
        public string? HelpText;
    }



    /// <summary>
    /// 
    /// </summary>
    public partial class OptionMap<T>
        where T : class, new()
    {
        [GeneratedRegex(@"[|<>-]")]
        private static partial Regex _escapeMarkdownRegex { get; }

        private readonly string _usage = "NeeView.exe [Options...] [File or Folder...]";

        private readonly static string[] _samples =
        {
            "NeeView.exe -s E:\\Pictures",
            "NeeView.exe -o \"E:\\Pictures?search=foobar\"",
            "NeeView.exe --window=full",
            "NeeView.exe --setting=\"C:\\MySetting.json\" --new-window=off",
        };

        // options
        private readonly List<OptionMemberElement> _elements;

        // values
        private readonly OptionValuesElement? _values;


        public OptionMap()
        {
            var type = typeof(T);

            _elements = new List<OptionMemberElement>();

            foreach (PropertyInfo info in type.GetProperties())
            {
                var attribute = (OptionBaseAttribute?)Attribute.GetCustomAttributes(info, typeof(OptionBaseAttribute)).FirstOrDefault();
                if (attribute != null)
                {
                    switch (attribute)
                    {
                        case OptionMemberAttribute memberAttribute:
                            _elements.Add(new OptionMemberElement(info, memberAttribute));
                            break;
                        case OptionValuesAttribute:
                            _values = new OptionValuesElement(info);
                            break;
                    }
                }
            }
            
            _elements.Sort();
        }

        public OptionMemberElement? GetElement(string key)
        {
            var word = key.TrimStart('-');

            if (key.StartsWith("--", StringComparison.Ordinal))
            {
                return _elements.FirstOrDefault(e => e.LongName == word);
            }
            else
            {
                return _elements.FirstOrDefault(e => e.ShortName == word);
            }
        }

        public string GetCommandLineHelpText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Usage: " + _usage);
            sb.AppendLine();
            foreach (var element in _elements.Where(e => e.IsVisible))
            {
                sb.AppendLine($"{CreateParameterKeyFormat(element)}\n                {element.HelpText}\n");
            }
            sb.AppendLine($"--\n                {Properties.TextResources.GetString("AppOption.Terminator")}");
            sb.AppendLine();
            sb.AppendLine("Examples:");
            foreach (var sample in _samples)
            {
                sb.AppendLine($"    {sample}");
            }
            return sb.ToString();
        }

#if DEBUG
        public string GetCommandLineHelpMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# " + Properties.TextResources.GetString("BootOptionDialog.Title"));
            sb.AppendLine();

            sb.AppendLine("### Usage");
            sb.AppendLine();
            sb.AppendLine("    > " + _usage);
            sb.AppendLine();

            sb.AppendLine("### Options");
            sb.AppendLine();
            sb.AppendLine($"Option|Description");
            sb.AppendLine($"--|--");
            foreach (var element in _elements.Where(e => e.IsVisible))
            {
                sb.AppendLine(EscapeMarkdown(CreateParameterKeyFormat(element)) + "|" + element.HelpText);
            }
            sb.AppendLine($"\\-\\-|{Properties.TextResources.GetString("AppOption.Terminator")}");
            sb.AppendLine();

            sb.AppendLine("### Examples");
            sb.AppendLine();
            foreach (var sample in _samples)
            {
                sb.AppendLine($"`> {sample}`");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string EscapeMarkdown(string text)
        {
            return _escapeMarkdownRegex.Replace(text, m => "\\" + m.Value);
        }

#endif // DEBUG

        private string CreateParameterKeyFormat(OptionMemberElement element)
        {
            // key
            var keys = new List<string?> { element.ShortName != null ? "-" + element.ShortName : null, element.LongName != null ? "--" + element.LongName : null };
            var key = string.Join(", ", keys.Where(e => e != null));

            // key value
            string keyValue = element.GetValuePrototype();
            if (!element.HasParameter)
            {
                keyValue = "";
            }
            else if (element.Default == null)
            {
                keyValue = $"=<{keyValue}>";
            }
            else
            {
                keyValue = $"[={keyValue}]";
            }

            return key + keyValue;
        }

        public T ParseArguments(string[] args)
        {
            bool isOptionTerminated = false;

            var options = new Dictionary<string, string?>();
            var values = new List<string>();

            for (int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                var next = i + 1 < args.Length ? args[i + 1] : null;

                // option terminator
                if (arg == "--")
                {
                    isOptionTerminated = true;
                }
                // option
                else if (!isOptionTerminated && arg.StartsWith("-", StringComparison.Ordinal))
                {
                    var tokens = arg.Split(new char[] { '=' }, 2);
                    var value = tokens.Length >= 2 ? tokens[1] : null;

                    var keys = OptionMap<T>.GetKeys(tokens[0]);

                    foreach (var key in keys)
                    {
                        bool isLast = keys.Last() == key;

                        var element = GetElement(key);
                        if (element == null)
                        {
                            var message = string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("OptionArgumentException.Unknown"), key) + "\n\n" + GetCommandLineHelpText();
                            throw new ArgumentException(message);
                        }

                        if (!isLast)
                        {
                            options.Add(key, null);
                        }
                        else if (value != null)
                        {
                            options.Add(key, value);
                        }
                        else if (next == null || next.StartsWith("-", StringComparison.Ordinal) || !element.RequireParameter)
                        {
                            options.Add(key, null);
                        }
                        else
                        {
                            options.Add(key, next);
                            i++;
                        }
                    }
                }
                // value
                else
                {
                    values.Add(arg);
                }
            }

            // マッピング
            var target = new T();
            Mapping(target, options, values);

            return target;
        }

        private static List<string> GetKeys(string keys)
        {
            if (keys.StartsWith("--", StringComparison.Ordinal))
            {
                return new List<string>() { keys };
            }
            else if (keys.StartsWith("-", StringComparison.Ordinal))
            {
                return keys.TrimStart('-').Select(e => "-" + e).ToList();
            }
            else
            {
                return new List<string>() { keys };
            }
        }

        private void Mapping(T source, Dictionary<string, string?> options, List<string> values)
        {
            foreach (var item in options)
            {
                Debug.WriteLine($"Option: {item.Key} = {item.Value}");

                var element = GetElement(item.Key);
                if (element == null) throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("OptionArgumentException.Unknown"), item.Key));

                var value = item.Value ?? element.Default;
                if (value == null) throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("OptionArgumentException.Empty"), item.Key));

                try
                {
                    element.SetValue(source, value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("OptionArgumentException.Failed"), item.Key, value));
                }
            }

            foreach (var value in values)
            {
                Debug.WriteLine($"Value: {value}");
            }

            _values?.SetValues(source, values);
        }
    }
}
