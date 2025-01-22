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
                writer.WriteLine(CreateMainMenuManual());
            }

            ExternalProcess.Start(fileName);
        }

        public static string CreateMainMenuManual()
        {
            var sb = new StringBuilder();

            var title = "NeeView " + ResourceService.GetString("@Word.MainMenu");
            sb.AppendLine(HtmlHelpUtility.CreateHeader(title));

            var menuTree = MenuTree.CreateDefault();

            var node = new TagNode("body");
            node.AddNode(new TagNode("h1").AddText(title));

            if (menuTree.Children is null) throw new InvalidOperationException("menuTree.Children must not be null");
            foreach (var group in menuTree.Children)
            {
                node.AddNode(new TagNode("h3").AddText(group.DispLabel));

                var table = new TagNode("table", "table-slim");
                if (group.Children is null) throw new InvalidOperationException("group.Children must not be null");
                foreach (var item in group.GetTable(0))
                {
                    table.AddNode(new TagNode("tr")
                        .AddNode(new TagNode("td").AddText(item.Element.DispLabel))
                        .AddNode(new TagNode("td").AddText(item.Element.Note)));
                }
                node.AddNode(table);
            }

            sb.AppendLine(node.ToString());
            sb.AppendLine(HtmlHelpUtility.CreateFooter());

            return sb.ToString();
        }
    }
}


