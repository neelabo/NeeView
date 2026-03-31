using NeeView.Properties;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 画像出力の処理フロー
    /// </summary>
    public static class ExportImageProcedure
    {
        public static async Task Run(IExportImageParameter parameter, bool showToast, CancellationToken token)
        {
            var source = ExportImageSourceFactory.Create();
            using var service = new ExportImageService(source, parameter);
            service.ThrowIfCannotExport();

            try
            {
                var path = CreateFileWriteSettings(service, ExportImageOverwritePolicyFactory.Create(parameter.OverwriteMode));

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


        private static string CreateFileWriteSettings(ExportImageService service, IExportOverwritePolicy overwritePolicy)
        {
            var filename = service.CreateFileName();

            // 出力フォルダー指定がなければ、ダイアログで出力ファイル名を確定する
            if (string.IsNullOrWhiteSpace(service.ExportFolder))
            {
                var dialog = new ExportImageSeveFileDialog(service.ExportFolder, filename, service.Mode == ExportImageMode.View);
                var result = dialog.ShowDialog(MainWindow.Current);
                if (result != true)
                {
                    throw new OperationCanceledException();
                }
                return dialog.FileName;
            }

            var resolver = new FileExportOverwriteResolver(service.ExportFolder);
            var name = overwritePolicy.Resolve(filename, resolver, service);
            return resolver.GetFullPath(name);
        }
    }

}