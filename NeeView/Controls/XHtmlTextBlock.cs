using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

namespace NeeView
{
    public class XHtmlTextBlock : TextBlock
    {
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(XHtmlTextBlock), new PropertyMetadata(null, SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is XHtmlTextBlock control)
            {
                control.Refresh();
            }
        }


        /// <summary>
        /// SourceをXHTMLとして解釈する
        /// </summary>
        public bool IsXHtml
        {
            get { return (bool)GetValue(IsXHtmlProperty); }
            set { SetValue(IsXHtmlProperty, value); }
        }

        public static readonly DependencyProperty IsXHtmlProperty =
            DependencyProperty.Register("IsXHtml", typeof(bool), typeof(XHtmlTextBlock), new PropertyMetadata(false, IsXHtmlChanged));

        private static void IsXHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is XHtmlTextBlock control)
            {
                control.Refresh();
            }
        }


        private void Refresh()
        {
            this.Inlines.Clear();

            if (string.IsNullOrWhiteSpace(Source)) return;

            try
            {
                var xhtml = "<xhtml>" + (IsXHtml ? Source : System.Security.SecurityElement.Escape(Source)) + "</xhtml>";
                var document = XDocument.Parse(xhtml);
                if (document?.Root is null) throw new InvalidOperationException();
                var inlines = ConvertFromChildren(document.Root);
                this.Inlines.AddRange(inlines);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.Inlines.Add(new Run(Source));
            }
        }

        private IEnumerable<Inline> ConvertFromChildren(XElement root)
        {
            return root.Nodes().Select(e => ConvertFrom(e));
        }

        private Inline ConvertFrom(XNode root)
        {
            if (root is XElement element)
            {
                switch (element.Name.ToString())
                {
                    case "a":
                        var hyperlink = new Hyperlink();
                        try
                        {
                            var href = element.Attribute("href");
                            if (href is null) throw new InvalidOperationException();
                            hyperlink.NavigateUri = new Uri(href.Value);
                        }
                        catch (Exception ex)
                        {
                            throw new XmlException($"Wrong \"<a href=...>\" format.", ex);
                        }
                        hyperlink.Inlines.AddRange(ConvertFromChildren(element));
                        hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                        return hyperlink;
                    case "b":
                        var bold = new Bold();
                        bold.Inlines.AddRange(ConvertFromChildren(element));
                        return bold;
                    case "i":
                        var italic = new Italic();
                        italic.Inlines.AddRange(ConvertFromChildren(element));
                        return italic;
                    case "u":
                        var underline = new Underline();
                        underline.Inlines.AddRange(ConvertFromChildren(element));
                        return underline;
                    case "br":
                        return new LineBreak();
                    default:
                        throw new XmlException($"Not support tag: {element.Name}");
                }
            }
            else if (root is XText text)
            {
                var run = new Run(text.Value);
                return run;
            }
            else
            {
                throw new XmlException($"Not support node type: {root.NodeType}");
            }
        }

        private static void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ExternalProcess.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
