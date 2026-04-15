using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // 外部アプリ起動
    public class ExternalAppUtility
    {
        private const string NeeViewKeyLegacy = "$NeeView";
        private const string NeeViewKey = "{NeeView}";

        private static readonly string[] _keywords = [
            OpenExternalAppCommandParameter.FileKeyLegacy,
            OpenExternalAppCommandParameter.FileKey,
            OpenExternalAppCommandParameter.UriKeyLegacy,
            OpenExternalAppCommandParameter.UriKey];

        // コマンドパラメータ文字列のバリデート
        public static string ValidateApplicationParam(string source)
        {
            if (source == null) source = "";
            source = source.Trim();

            if (_keywords.All(e => !source.Contains(e)))
            {
                source = source + $" \"{OpenExternalAppCommandParameter.FileKey}\"";
            }
            return source;
        }

        /// <summary>
        /// 外部アプリ実行
        /// </summary>
        /// <param name="pages">実行するページ群</param>
        /// <param name="options">実行オプション</param>
        /// <param name="token">キャンセルトークン</param>
        public async Task CallAsync(IEnumerable<Page> pages, IExternalApp options, CancellationToken token)
        {
            try
            {
                var files = await PageUtility.CreateRealizedFilePathListAsync(pages, options.ArchivePolicy, token);
                Call(files, options);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                new MessageDialog(TextResources.GetString("OpenApplicationErrorDialog.Title"), ex.Message).ShowDialog();
            }
        }

        /// <summary>
        /// 外部アプリ実行
        /// </summary>
        /// <param name="paths">実行するファイルパス群</param>
        /// <param name="options">実行オプション</param>
        /// <param name="token">キャンセルトークン</param>
        public void Call(IEnumerable<string> paths, IExternalApp options)
        {
            // options.MultiPagePolicy は無視される。paths に渡される時点で処理済みであること。
            foreach (var path in paths.Distinct())
            {
                CallProcess(path, options);
            }
        }


        // 外部アプリの実行(コア)
        private static void CallProcess(string fileName, IExternalApp options)
        {
            string param = ReplaceKeyword(ValidateApplicationParam(options.Parameter), fileName);

            var processOptions = new ExternalProcessOptions() { IsThrowException = true, WorkingDirectory = options.WorkingDirectory };

            if (string.IsNullOrWhiteSpace(options.Command))
            {
                var sentence = $"\"{param}\"";
                Debug.WriteLine($"CallProcess: {sentence}");
                try
                {
                    ExternalProcess.Start(param, null, processOptions);
                }
                catch (Exception ex)
                {
                    var message = $"{ex.Message}: {sentence}";
                    throw new InvalidOperationException(message, ex);
                }
            }
            else
            {
                var command = options.Command;
                command = command.Replace(NeeViewKeyLegacy, Environment.AssemblyLocation, StringComparison.Ordinal); // 互換性のため、$NeeView も {NeeView} も同じ意味で置換される。
                command = command.Replace(NeeViewKey, Environment.AssemblyLocation, StringComparison.Ordinal);
                
                if (command.Contains(Environment.AssemblyLocation))
                {
                    SaveDataSync.Current.SaveAll(true);
                }
                
                var sentence = $"\"{command}\" {param}";
                Debug.WriteLine($"CallProcess: {sentence}");
                try
                {
                    ExternalProcess.Start(command, param, processOptions);
                }
                catch (Exception ex)
                {
                    var message = $"{ex.Message}: {sentence}";
                    throw new InvalidOperationException(message, ex);
                }
            }
            return;
        }

        private static string ReplaceKeyword(string s, string fileName)
        {
            var uriData = Uri.EscapeDataString(fileName);

            s = s.Replace(OpenExternalAppCommandParameter.UriKeyLegacy, uriData, StringComparison.Ordinal); // 互換性のため、$Uri と {Uri} も同じ意味で置換される。
            s = s.Replace(OpenExternalAppCommandParameter.UriKey, uriData, StringComparison.Ordinal);
            s = s.Replace(OpenExternalAppCommandParameter.FileKeyLegacy, fileName, StringComparison.Ordinal); // 互換性のため、$File と {File} も同じ意味で置換される。
            s = s.Replace(OpenExternalAppCommandParameter.FileKey, fileName, StringComparison.Ordinal);
            return s;
        }

        public static async Task OpenExternalAppAsync(IEnumerable<ArchiveEntry> entries, IExternalApp parameter, CancellationToken token)
        {
            var items = await ArchiveEntryUtility.RealizeArchiveEntry(entries, token);
            var external = new ExternalAppUtility();
            external.Call(items, parameter);
            GC.KeepAlive(entries);
        }

        public static async Task<bool> TryOpenExternalAppAsync(IEnumerable<ArchiveEntry> entries, IExternalApp parameter, CancellationToken token)
        {
            return await ExceptionHandling.WithToastAsync((token) => OpenExternalAppAsync(entries.ToList(), parameter, token), TextResources.GetString("OpenApplicationErrorDialog.Title"), token);
        }
    }
}
