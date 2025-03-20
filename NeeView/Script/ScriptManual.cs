using NeeView.Text.SimpleHtmlBuilder;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeeView
{
    public class ScriptManual
    {
        private static readonly string _manualTemplate = """
            <h1>@_ScriptManual.Title</h1>

            @_VersionTag

            <ul>
                <li><a href="#s1">@_ScriptManual.S1</a></li>
                <li><a href="#s2">@_ScriptManual.S2</a></li>
                <li><a href="#s3">@_ScriptManual.S3</a></li>
                <li><a href="#s4">@_ScriptManual.S4</a></li>
                <li><a href="#s5">@_ScriptManual.S5</a></li>
                <li><a href="#ConfigList">@_ScriptManual.S6</a></li>
                <li><a href="#CommandList">@_ScriptManual.S7</a></li>
                <li><a href="#ObsoleteList">@_ScriptManual.S8</a></li>
                <li><a href="#s9">@_ScriptManual.S9</a></li>
            </ul>

            <h2 id="s1">@_ScriptManual.S1</h2>
            <p>
                @_ScriptManual.S1.P1
            </p>
            <p>
                @_ScriptManual.S1.P2

                <ul>
                    <li>@_ScriptManual.S1.P2.I1</li>
                    <li>@_ScriptManual.S1.P2.I2</li>
                    <li>@_ScriptManual.S1.P2.I3</li>
                    <li>@_ScriptManual.S1.P2.I4</li>
                </ul>
            </p>
            <p>
                @_ScriptManual.S1.P3

                <table>
                    <tr><th>@_ScriptManual.S1.P3.T00</th><th>@_ScriptManual.S1.P3.T01</th></tr>
                    <tr><td>&#64;args</td><td>@_ScriptManual.S1.P3.T61</td></tr>
                    <tr><td>&#64;description</td><td>@_ScriptManual.S1.P3.T21</td></tr>
                    <tr><td>&#64;mouseGesture</td><td>@_ScriptManual.S1.P3.T41</td></tr>
                    <tr><td>&#64;name</td><td>@_ScriptManual.S1.P3.T11</td></tr>
                    <tr><td>&#64;shortCutKey</td><td>@_ScriptManual.S1.P3.T31</td></tr>
                    <tr><td>&#64;touchGesture</td><td>@_ScriptManual.S1.P3.T51</td></tr>
                </table>
            </p>

            <h2 id="s2">@_ScriptManual.S2</h2>
            <p>
                @_ScriptManual.S2.P1

                <ul>
                    <li>@_ScriptManual.S2.P1.I1</li>
                    <li>@_ScriptManual.S2.P1.I2</li>
                </ul>
            </p>

            <h2 id="s3">@_ScriptManual.S3</h2>
            <p>
                @_ScriptManual.S3.P1

                <table>
                    <tr><td>OnStartup.nvjs</td><td>@ScriptOnStartupCommand.Remarks</td></tr>
                    <tr><td>OnBookLoaded.nvjs</td><td>@ScriptOnBookLoadedCommand.Remarks</td></tr>
                    <tr><td>OnPageChanged.nvjs</td><td>@ScriptOnPageChangedCommand.Remarks</td></tr>
                    <tr><td>OnWindowStateChanged.nvjs</td><td>@ScriptOnWindowStateChangedCommand.Remarks</td></tr>
                </table>
            </p>

            <h2 id="s4">@_ScriptManual.S4</h2>
            <p>
                @_ScriptManual.S4.P1
            </p>

            <h4>@_ScriptManual.S4.T</h4>
            <p>
                <table>
                    <tr><td>cls</td><td>@_ScriptManual.S4.T.T01</td></tr>
                    <tr><td>exit</td><td>@_ScriptManual.S4.T.T11</td></tr>
                    <tr><td>help, ?</td><td>@_ScriptManual.S4.T.T21</td></tr>
                </table>
            </p>
            """;

        private static readonly string _exampleTemplate = """
            <h2 id="s9" class="sub">@_ScriptManual.S9</h2>

            @_ScriptManual.S9.P1

            <h3>@_ScriptManual.S91</h3>
            <p>
              OpenMsPaint.nvjs
              <pre><code class="example">@[/Resources/Scripts/OpenMsPaint.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S92</h3>
            <p>
              OpenNeeView.nvjs
              <pre><code class="example">@[/Resources/Scripts/OpenNeeView.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S93</h3>
            <p>
              ToggleUnsharpMask.nvjs
              <pre><code class="example">@[/Resources/Scripts/ToggleUnsharpMask.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S94</h3>
            <p>
              OnBookLoaded.nvjs
              <pre><code class="example">@[/Resources/Scripts/OnBookLoaded.ReadOrder.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S95</h3>
            <p>
              OnBookLoaded.nvjs
              <pre><code class="example">@[/Resources/Scripts/OnBookLoaded.Media.nvjs]</code></pre>
            </p>
            <p>
              ToggleFullScreenAndMediaPlay.nvjs
              <pre><code class="example">@[/Resources/Scripts/ToggleFullScreenAndMediaPlay.nvjs]</code></pre>
            </p>
            """;

        private static readonly List<Type> _typeCollection;


        static ScriptManual()
        {
            _typeCollection = DocumentableTypeCollector.Collect(typeof(CommandHost));
        }


        public void OpenScriptManual()
        {
            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "ScriptManual.html");

            // create html file
            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.Write(CreateScriptManualText());
            }

            // open in browser
            ExternalProcess.Start(fileName);
        }

        private string GetScriptManualText()
        {
            return ResourceService.Replace(_manualTemplate);
        }

        private string GetScriptExampleText()
        {
            return ResourceService.Replace(_exampleTemplate);
        }

        public string CreateScriptManualText()
        {
            var builder = new StringBuilder();

            builder.AppendLine(HtmlHelpUtility.CreateHeader(ResourceService.GetString("@_ScriptManual.Title")));
            builder.AppendLine($"<body>");

            builder.AppendLine(GetScriptManualText());

            AppendScriptReference(builder);

            AppendConfigList(builder);

            AppendCommandList(builder);

            AppendObsoleteList(builder);

            builder.AppendLine(GetScriptExampleText());

            builder.AppendLine("</body>");
            builder.AppendLine(HtmlHelpUtility.CreateFooter());

            return builder.ToString();
        }

        private static StringBuilder AppendScriptReference(StringBuilder builder)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"<h2 id=\"s5\">{ResourceService.GetString("@_ScriptManual.S5")}</h2>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<p>{ResourceService.GetString("@_ScriptManual.S5.P1")}</p>");

            var htmlBuilder = new HtmlReferenceBuilder(builder);

            htmlBuilder.CreateMethodTable(typeof(JavaScriptEngine), null);

            htmlBuilder.Append(typeof(CommandHost), "[Root Instance] nv");

            foreach (var classType in _typeCollection.Where(e => !e.IsEnum).OrderBy(e => e.Name))
            {
                htmlBuilder.Append(classType);
            }

            foreach (var enumType in _typeCollection.Where(e => e.IsEnum).OrderBy(e => e.Name))
            {
                htmlBuilder.Append(enumType);
            }

            return htmlBuilder.ToStringBuilder();
        }

        private static StringBuilder AppendConfigList(StringBuilder builder)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"<h2 id=\"ConfigList\">{ResourceService.GetString("@_ScriptManual.S6")}</h2>");
            builder.AppendLine("<table>");
            builder.AppendLine("<tr>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<th class=\"nowrap\">{Properties.TextResources.GetString("Word.Name")}</th>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<th class=\"td-middle\">{Properties.TextResources.GetString("Word.Type")}</th>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"<th class=\"td-middle\">{Properties.TextResources.GetString("Word.Summary")}</th>");
            builder.AppendLine("</tr>");
            builder.AppendLine(new ConfigMap(null).Map.CreateHelpHtml("nv.Config"));
            builder.AppendLine("</table>");
            return builder;
        }

        private StringBuilder AppendCommandList(StringBuilder builder)
        {
            var executeMethodArgTypes = new Type[] { typeof(object), typeof(CommandContext) };

            builder.AppendLine(CultureInfo.InvariantCulture, $"<h2 id=\"CommandList\">{ResourceService.GetString("@_ScriptManual.S7")}</h2>");

            foreach (var group in CommandTable.Current.Values.GroupBy(e => e.Group))
            {
                builder.AppendLine($"<h3>{group.Key}</h3>");
                builder.AppendLine("<table>");
                builder.AppendLine("<tr>");
                builder.AppendLine(CultureInfo.InvariantCulture, $"<th>{Properties.TextResources.GetString("Word.CommandName")}</th>");
                builder.AppendLine(CultureInfo.InvariantCulture, $"<th class=\"td-middle\">{Properties.TextResources.GetString("Word.Argument")}</th>");
                builder.AppendLine(CultureInfo.InvariantCulture, $"<th class=\"td-middle\">{Properties.TextResources.GetString("Word.CommandParameter")}</th>");
                builder.AppendLine(CultureInfo.InvariantCulture, $"<th class=\"td-middle\">{Properties.TextResources.GetString("Word.Summary")}</th></tr>");
                builder.AppendLine("</tr>");

                foreach (var command in group.OrderBy(e => e.Order))
                {
                    string argument = "";
                    {
                        var type = command.GetType();
                        var info = type.GetMethod(nameof(command.Execute), executeMethodArgTypes) ?? throw new InvalidOperationException();
                        var attribute = (MethodArgumentAttribute?)Attribute.GetCustomAttributes(info, typeof(MethodArgumentAttribute)).FirstOrDefault();
                        if (attribute != null)
                        {
                            var tokens = MethodArgumentAttributeExtensions.GetMethodNote(info, attribute)?.Split('|');
                            int index = 0;
                            argument += "<dl>";
                            while (tokens is not null && index < tokens.Length)
                            {
                                var dt = tokens.ElementAtOrDefault(index++);
                                var dd = tokens.ElementAtOrDefault(index++);
                                argument += $"<dt>{dt}</dt><dd>{dd}</dd>";
                            }
                            argument += "</dl>";
                        }
                    }

                    string properties = "";
                    if (command.Parameter != null)
                    {
                        var type = command.Parameter.GetType();
                        var title = "";

                        if (command.Share != null)
                        {
                            properties = "<p style=\"color:red\">" + string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("CommandParameter.Share"), command.Share.Name) + "</p>";
                        }

                        foreach (PropertyInfo info in type.GetProperties())
                        {
                            var attribute = (PropertyMemberAttribute?)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
                            if (attribute != null && attribute.IsVisible)
                            {
                                var titleString = PropertyMemberAttributeExtensions.GetPropertyTitle(info, attribute);
                                if (titleString != null)
                                {
                                    title = titleString + " / ";
                                }

                                var enums = "";
                                if (info.PropertyType.IsEnum)
                                {
                                    enums = string.Join(" / ", info.PropertyType.VisibleAliasNameDictionary().Select(e => $"\"{e.Key}\": {e.Value}")) + "<br/>";
                                }

                                var propertyName = PropertyMemberAttributeExtensions.GetPropertyName(info, attribute).TrimEnd(Properties.TextResources.GetString("Word.Period").ToArray()) + Properties.TextResources.GetString("Word.Period");
                                var text = title + propertyName;

                                var propertyTips = PropertyMemberAttributeExtensions.GetPropertyTips(info, attribute);
                                if (propertyTips != null)
                                {
                                    text = text + " " + propertyTips;
                                }

                                properties = properties + $"<dt><b>{info.Name}</b>: {info.PropertyType.ToManualString()}</dt><dd>{enums + text}<dd/>";
                            }
                        }
                        if (!string.IsNullOrEmpty(properties))
                        {
                            properties = "<dl>" + properties + "</dl>";
                        }
                    }

                    builder.AppendLine("<tr>");
                    builder.AppendLine(CultureInfo.InvariantCulture, $"<td class=\"nowrap\">{command.Name}</td>");
                    builder.AppendLine(CultureInfo.InvariantCulture, $"</td><td>{argument}</td>");
                    builder.AppendLine(CultureInfo.InvariantCulture, $"<td>{properties}</td>");
                    builder.AppendLine(CultureInfo.InvariantCulture, $"<td><b>{command.Text}</b><p class=\"remarks\">{command.Remarks}</p></td>");
                    builder.AppendLine("</tr>");
                }
                builder.AppendLine("</table>");
            }

            return builder;
        }


        private static StringBuilder AppendObsoleteList(StringBuilder builder)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"<h2 id=\"ObsoleteList\">{ResourceService.GetString("@_ScriptManual.S8")}</h2>");

            // Obsolete levels
            builder.AppendLine(ResourceService.Replace($"<h4>@Word.Severity</h4>"));
            var obsoleteLevels = new TagNode("p")
                .AddNode(new TagNode("table")
                    .AddNode(new TagNode("tr")
                        .AddNode(new TagNode("td").AddText($"{ScriptErrorLevel.Error}"))
                        .AddNode(new TagNode("td").AddText($"@ScriptErrorLevel.Error.Severity")))
                    .AddNode(new TagNode("tr")
                        .AddNode(new TagNode("td").AddText($"{ScriptErrorLevel.Warning}"))
                        .AddNode(new TagNode("td").AddText($"@ScriptErrorLevel.Warning.Severity")))
                    .AddNode(new TagNode("tr")
                        .AddNode(new TagNode("td").AddText($"{ScriptErrorLevel.Info}"))
                        .AddNode(new TagNode("td").AddText($"@ScriptErrorLevel.Info.Severity")))
                );
            builder.AppendLine(obsoleteLevels.ToIndentString());

            var commandHost = new CommandHost();
            var root = ScriptNodeTreeBuilder.Create(commandHost, "nv");

            var nodeItems = root.GetUnitEnumerator(null)
                .Where(e => e.Node.Obsolete != null || e.Node.Alternative != null)
                .Select(e => new ScriptMemberAlternative(e.FullName, e.Node.Obsolete, e.Node.Alternative) { AlternativeMessage = e.Alternative });

            var classItems = _typeCollection.Where(e => !e.IsEnum)
                .SelectMany(e => new ScriptClassInfo(e).Members)
                .Where(e => e.HasObsolete || e.HasAlternative)
                .Select(e => new ScriptMemberAlternative(e.Name, e.ObsoleteAttribute, e.AlternativeAttribute));

            var groups = nodeItems.Concat(classItems)
                .GroupBy(e => e.Version)
                .OrderBy(e => e.Key);

            // ver.40 and later
            foreach (var group in groups.Where(e => e.Key >= 40).OrderByDescending(e => e.Key))
            {
                builder.Append(CultureInfo.InvariantCulture, $"<h3>Version {group.Key}.0</h3>");

                var table = new TagNode("table")
                    .AddNode(new TagNode("tr")
                        .AddNode(new TagNode("th").AddText($"@Word.Severity"))
                        .AddNode(new TagNode("th").AddText($"@Word.Name"))
                        .AddNode(new TagNode("th", "nowrap").AddText($"@Word.Category"))
                        .AddNode(new TagNode("th", "td-middle").AddText($"@Word.Alternative")));

                foreach (var item in group.OrderByDescending(e => e.ErrorLevel).ThenBy(e => e.Name))
                {
                    table.AddNode(new TagNode("tr")
                        .AddNode(new TagNode("td").AddText(item.ErrorLevel.ToString()))
                        .AddNode(new TagNode("td").AddText(item.Name))
                        .AddNode(new TagNode("td", "nowrap").AddText(item.HasObsolete ? "@Word.Obsolete" : "@Word.Changed"))
                        .AddNode(new TagNode("td").AddText(item.AlternativeMessage)));
                }

                builder.AppendLine(table.ToIndentString());
            }

            return builder;
        }
    }
}
