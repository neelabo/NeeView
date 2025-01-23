using NeeView.Data;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace NeeView
{
#if DEBUG
    public static class DebugCommand
    {
        // [開発用] デバッグアクションの実行
        public static void Execute(string? command)
        {
            switch (command)
            {
                case "export-docs":
                    ExportEmbeddedDocuments();
                    Application.Current.Shutdown();
                    break;
            }
        }

        // [開発用] 組み込みドキュメントを出力
        public static void ExportEmbeddedDocuments()
        {
            try
            {
                var optionMap = new OptionMap<CommandLineOption>();
                var cultures = new[] { "en-us", "ja-jp" };
                foreach (var culture in cultures)
                {
                    Properties.TextResources.Initialize(new CultureInfo(culture), true);
                    var markdown = optionMap.GetCommandLineHelpMarkdown();
                    File.WriteAllText(Path.Combine(Environment.UserDataPath, $"CommandLineOptions.{culture}.md"), markdown);

                    var scriptManual = new ScriptManual().CreateScriptManualText();
                    File.WriteAllText(Path.Combine(Environment.UserDataPath, $"ScriptManual.{culture}.html"), scriptManual);

                    var searchOptionManual = SearchOptionManual.CreateSearchOptionManual();
                    File.WriteAllText(Path.Combine(Environment.UserDataPath, $"SearchOptionManual.{culture}.html"), searchOptionManual);

                    var mainMenu = MainMenuManual.CreateMainMenuManual();
                    File.WriteAllText(Path.Combine(Environment.UserDataPath, $"MainMenu.{culture}.html"), mainMenu);

                    var commandList = CommandTable.Current.CreateCommandListHelp();
                    File.WriteAllText(Path.Combine(Environment.UserDataPath, $"CommandList.{culture}.html"), commandList);
                }
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, "").ShowDialog();
            }
        }

    }
#endif
}
