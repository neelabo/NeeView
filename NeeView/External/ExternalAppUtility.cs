using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // 外部アプリ起動
    public class ExternalAppUtility
    {
        // コマンドパラメータ文字列のバリデート
        public static string ValidateApplicationParam(string source)
        {
            if (source == null) source = "";
            source = source.Trim();
            return source.Contains(OpenExternalAppCommandParameter.KeyFile, StringComparison.Ordinal) ? source : (source + $" \"{OpenExternalAppCommandParameter.KeyFile}\"");
        }

        /// <summary>
        /// 外部アプリ実行
        /// </summary>
        /// <param name="pages">実行するページ群</param>
        /// <param name="options">実行オプション</param>
        /// <param name="token">キャンセルトークン</param>
        public async ValueTask CallAsync(IEnumerable<Page> pages, IExternalApp options, CancellationToken token)
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
                new MessageDialog(ex.Message, Properties.TextResources.GetString("OpenApplicationErrorDialog.Title")).ShowDialog();
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
                var command = options.Command.Replace("$NeeView", Environment.AssemblyLocation, StringComparison.Ordinal);
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

        private static string ReplaceKeyword(string s, string filenName)
        {
            var uriData = Uri.EscapeDataString(filenName);

            s = s.Replace(OpenExternalAppCommandParameter.KeyUri, uriData, StringComparison.Ordinal);
            s = s.Replace(OpenExternalAppCommandParameter.KeyFile, filenName, StringComparison.Ordinal);
            return s;
        }
    }
}
