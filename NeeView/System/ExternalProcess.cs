using NeeView.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class ExternalProcessOptions
    {
        public bool IsThrowException { get; set; }
        public string? WorkingDirectory { get; set; }
    }

    public static partial class ExternalProcess
    {
        [GeneratedRegex(@"^\s*http[s]?:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex _httpPrefix { get; }

        [GeneratedRegex(@"\.htm[l]?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex _htmlPostfix { get; }

        public static void Start(string filename, string? args = null, ExternalProcessOptions? options = null)
        {
            options = options ?? new ExternalProcessOptions();

            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = options.WorkingDirectory ?? startInfo.WorkingDirectory;

            if (string.IsNullOrWhiteSpace(filename))
            {
                startInfo.FileName = args;
            }
            else
            {
                startInfo.FileName = filename;
                startInfo.Arguments = args;
            }

            if (string.IsNullOrWhiteSpace(startInfo.FileName))
            {
                return;
            }

            if (!Config.Current.System.IsNetworkEnabled && _httpPrefix.IsMatch(startInfo.FileName))
            {
                var dialog = new MessageDialog(TextResources.GetString("ExternalProcess.ConfirmBrowserDialog.Message"), TextResources.GetString("ExternalProcess.ConfirmBrowserDialog.Title"));
                dialog.Commands.AddRange(UICommands.OKCancel);
                var result = dialog.ShowDialog();
                if (!result.IsPossible)
                {
                    return;
                }
            }

            if (Config.Current.System.WebBrowser != null && string.IsNullOrEmpty(startInfo.Arguments) && IsBrowserContent(startInfo.FileName))
            {
                startInfo.Arguments = "\"" + startInfo.FileName + "\"";
                startInfo.FileName = Config.Current.System.WebBrowser;
            }

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                if (options.IsThrowException)
                {
                    throw;
                }
                else
                {

                    ToastService.Current.Show(new Toast(ex.Message + "\r\n\r\n" + startInfo.FileName, TextResources.GetString("Word.Error"), ToastIcon.Error));
                }
            }
        }


        public static void OpenWithTextEditor(string path)
        {
            string winDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows);
            var textEditor = Config.Current.System.TextEditor ?? Path.Combine(winDir, "System32", "notepad.exe");
            Start(textEditor, $"\"{path}\"");
        }


        private static bool IsBrowserContent(string path)
        {
            var isResult = _httpPrefix.IsMatch(path) || _htmlPostfix.IsMatch(path);
            return isResult;
        }

        public static void OpenWithFileManager(string path, bool isFolder = false, ExternalProcessOptions? options = null)
        {
            var systemConfig = Config.Current.System;
            var fileManager = systemConfig.FileManager ?? GetExplorerPath();
            var args = ValidateArgs(isFolder ? systemConfig.FileManagerFolderArgs : systemConfig.FileManagerFileArgs, path);
            Start(fileManager, args, options);
        }

        private static string GetExplorerPath()
        {
            var winDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows);
            return Path.Combine(winDir, "explorer.exe");
        }

        private static string ValidateArgs(string source, string file)
        {
            var text = source.Replace("$File", file, StringComparison.Ordinal);
            return string.IsNullOrWhiteSpace(text) ? file : text;
        }
    }
}
