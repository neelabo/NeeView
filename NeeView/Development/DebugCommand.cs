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
                var culture = Properties.TextResources.Culture.Name.ToLower();
                if (culture == "ja") culture = "ja-jp";
                if (culture == "en") culture = "en-us";

                var optionMap = new OptionMap<CommandLineOption>();
                var markdown = optionMap.GetCommandLineHelpMarkdown();
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"CommandLineOptions.{culture}.md"), markdown);

                var scriptManual = new ScriptManual().CreateScriptManualText();
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"ScriptManual.{culture}.html"), scriptManual);

                var searchOptionManual = SearchOptionManual.CreateSearchOptionManual();
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"SearchOptionManual.{culture}.html"), searchOptionManual);

                var mainMenu = MainMenuManual.CreateMainMenuManual();
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"MainMenu.{culture}.html"), mainMenu);

                var memento = CommandTable.Current.CreateCommandCollectionMemento(false);
                CommandTable.Current.RestoreCommandCollection(InputScheme.TypeA);
                var commandList = CommandTable.Current.CreateCommandListHelp();
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"CommandList.{culture}.html"), commandList);
                CommandTable.Current.RestoreCommandCollection(memento);
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, "").ShowDialog();
            }
        }

    }
#endif
}
