using NeeView.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 画像出力の処理フロー
    /// </summary>
    public static class ExportImageProcedure
    {
        public static async Task Run(IExportImageParameter parameter, string? filename, bool showToast, CancellationToken token)
        {
            var source = ExportImageSourceFactory.Create();
            using var service = new ExportImageService(source, parameter);
            service.ThrowIfCannotExport();

            try
            {
                var path = CreateOutputPath(service, ExportImageOverwritePolicyFactory.Create(parameter.OverwriteMode), filename);

                await service.ExportAsync(path, true, token);

                if (showToast)
                {
                    var toast = new Toast(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ExportImage.Message.Success"), path));
                    ToastService.Current.Show(toast);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                var toast = new Toast(ex.Message, TextResources.GetString("Word.Error"), ToastIcon.Error);
                ToastService.Current.Show(toast);
            }
        }


        private static string CreateOutputPath(ExportImageService service, IExportOverwritePolicy overwritePolicy, string? filename)
        {
            var parameter = service.Parameter;
            filename = filename ??service.CreateFileName();

            // 出力フォルダー指定がなければ、ダイアログで出力ファイル名を確定する
            if (string.IsNullOrWhiteSpace(parameter.ExportFolder))
            {
                var canSelectFormat = parameter.Mode == ExportImageMode.View;

                var dialog = new ExportImageSeveFileDialog(parameter.ExportFolder, filename, canSelectFormat, false);
                var result = dialog.ShowDialog(MainWindow.Current);
                if (result != true)
                {
                    throw new OperationCanceledException();
                }

                parameter.ExportFolder = Path.GetDirectoryName(dialog.FileName) ?? throw new DirectoryNotFoundException();
                filename = Path.GetFileName(dialog.FileName);
            }

            var resolver = new FileExportOverwriteResolver(parameter.ExportFolder);
            var name = overwritePolicy.Resolve(filename, resolver, service);
            return resolver.GetFullPath(name);
        }
    }

}