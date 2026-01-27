using NeeView.Data;
using NeeView.Properties;
using System;
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
                var culture = TextResources.Culture.Name.ToLower();
                if (culture == "ja") culture = "ja-jp";
                if (culture == "en") culture = "en-us";

                TextResources.Resource.SetItem("_VersionTag", $"<p>Version {Environment.ApplicationVersion}</p>");

                var optionMap = new OptionMap<CommandLineOption>();
                var markdown = optionMap.GetCommandLineHelpMarkdown(true);
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"CommandLineOptions.{culture}.md"), markdown);

                var scriptManual = new ScriptManual().CreateScriptManualText();
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"ScriptManual.{culture}.html"), scriptManual);

                var searchOptionManual = SearchOptionManual.CreateSearchOptionManual();
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"SearchOptionManual.{culture}.html"), searchOptionManual);

                var mainMenu = MainMenuManual.CreateMainMenuManual(true);
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"MainMenu.{culture}.html"), mainMenu);

                var memento = CommandTable.Current.CreateCommandCollectionMemento(false);
                CommandTable.Current.RestoreCommandCollection(InputScheme.TypeA);
                var commandList = CommandTable.Current.CreateCommandListHelp(true);
                File.WriteAllText(Path.Combine(Environment.UserDataPath, $"CommandList.{culture}.html"), commandList);
                CommandTable.Current.RestoreCommandCollection(memento, true);
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, "").ShowDialog();
            }
        }

    }
#endif
}
