using NeeView.Text.SimpleHtmlBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace NeeView
{
    public static class MainMenuManual
    {
        public static void OpenMainMenuManual()
        {
            System.IO.Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "MainMenuList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(CreateMainMenuManual(false));
            }

            ExternalProcess.Start(fileName);
        }

        public static string CreateMainMenuManual(bool version)
        {
            var sb = new StringBuilder();

            var title = "NeeView " + ResourceService.GetString("@Word.MainMenu");
            sb.AppendLine(HtmlHelpUtility.CreateHeader(title));

            var menuTree = MenuTree.CreateDefault();

            var node = new TagNode("body");
            node.AddNode(new TagNode("h1").AddText(title));

            if (version)
            {
                node.AddNode(new TagNode("p").AddText("Version " + Environment.ApplicationVersion));
            }

            foreach (var group in menuTree.Root)
            {
                node.AddNode(new TagNode("h3").AddText(group.Value.DisplayLabel));

                var table = new TagNode("table");
                foreach (var item in MenuTreeTools.GetMenuTable(group, 0))
                {
                    table.AddNode(new TagNode("tr")
                        .AddNode(new TagNode("td").AddText(item.Element.Value.DisplayLabel))
                        .AddNode(new TagNode("td").AddText(item.Element.Value.Note)));
                }
                node.AddNode(table);
            }

            sb.AppendLine(node.ToIndentString());
            sb.AppendLine(HtmlHelpUtility.CreateFooter());

            return sb.ToString();
        }
    }
}


