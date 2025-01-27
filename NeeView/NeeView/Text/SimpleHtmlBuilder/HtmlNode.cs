using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Simple HTML generation assistant
namespace NeeView.Text.SimpleHtmlBuilder
{
    public delegate string TextEvaluator(string text);

    public abstract class HtmlNode
    {
        public static TextEvaluator DefaultTextEvaluator { get; set; } = e => e;

        public abstract StringBuilder Append(StringBuilder builder);

        public abstract StringBuilder AppendIndentLine(StringBuilder builder, int depth);
    }

    public class TagNode : HtmlNode
    {
        private readonly string _name;
        private List<string>? _attributes;
        private List<HtmlNode>? _nodes;

        public TagNode(string name)
        {
            _name = name;
        }

        public TagNode(string name, string classValue)
        {
            _name = name;
            AddAttribute("class", classValue);
        }

        public TagNode AddAttribute(string name, string value)
        {
            _attributes ??= new List<string>();
            _attributes.Add($"{name}=\"{value}\"");
            return this;
        }

        public TagNode AddNode(HtmlNode node)
        {
            _nodes ??= new List<HtmlNode>();
            _nodes.Add(node);
            return this;
        }

        public TagNode AddText(string text)
        {
            return AddNode(new TextNode(DefaultTextEvaluator(text)));
        }

        public TagNode AddText(string text, TextEvaluator textEvaluator)
        {
            return AddNode(new TextNode(textEvaluator(text)));
        }

        public override StringBuilder Append(StringBuilder builder)
        {
            var tagWithAttribute = _name + (_attributes is null ? "" : " " + string.Join(" ", _attributes));
            var tag = _name;

            if (_nodes is null)
            {
                builder.Append(CultureInfo.InvariantCulture, $"<{tagWithAttribute}>");
            }
            else
            {
                builder.Append(CultureInfo.InvariantCulture, $"<{tagWithAttribute}>");
                foreach (var node in _nodes)
                {
                    node.Append(builder);
                }
                builder.Append(CultureInfo.InvariantCulture, $"</{tag}>");
            }
            return builder;
        }

        public override StringBuilder AppendIndentLine(StringBuilder builder, int depth)
        {
            var tagWithAttribute = _name + (_attributes is null ? "" : " " + string.Join(" ", _attributes));
            var tag = _name;
            
            var indent = new string(' ', depth * 2);

            if (_nodes is null)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"{indent}<{tagWithAttribute}/>");
            }
            else if (_nodes.Count == 1)
            {
                builder.Append(CultureInfo.InvariantCulture, $"{indent}<{tagWithAttribute}>");
                _nodes.First().Append(builder);
                builder.AppendLine(CultureInfo.InvariantCulture, $"</{tag}>");
            }
            else {
                builder.AppendLine(CultureInfo.InvariantCulture, $"{indent}<{tagWithAttribute}>");
                foreach (var node in _nodes)
                {
                    node.AppendIndentLine(builder, depth + 1);
                }
                builder.AppendLine(CultureInfo.InvariantCulture, $"{indent}</{tag}>");
            }
            return builder;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            Append(builder);
            return builder.ToString();
        }

        public string ToIndentString()
        {
            var builder = new StringBuilder();
            AppendIndentLine(builder, 0);
            return builder.ToString();
        }
    }


    public class TextNode : HtmlNode
    {
        private readonly string _text;

        public TextNode(string text)
        {
            _text = text;
        }

        public override StringBuilder Append(StringBuilder builder)
        {
            return builder.Append(_text);
        }

        public override StringBuilder AppendIndentLine(StringBuilder builder, int depth)
        {
            var indent = new string(' ', depth * 2);
            builder.AppendLine(CultureInfo.InvariantCulture, $"{indent}{_text}");
            return builder;
        }

        public override string ToString()
        {
            return _text;
        }
    }
}


