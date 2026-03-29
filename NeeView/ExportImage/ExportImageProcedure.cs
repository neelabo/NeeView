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
        public static async Task RunDialogFlow(IExportImageParameter parameter, bool showToast, CancellationToken token)
        {
            var source = ExportImageSourceFactory.Create();
            using var service = new ExportImageService(source, parameter);
            service.ThrowIfCannotExport();

            var filename = service.CreateFileName();
            var file = ValidateFileName(service, filename, ExportImageOverwritePolicyFactory.Create(parameter.OverwriteMode));

            try
            {
                await service.ExportAsync(file.FilePath, file.AllowOverwrite, token);

                if (showToast)
                {
                    var toast = new Toast(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ExportImage.Message.Success"), filename));
                    ToastService.Current.Show(toast);
                }
            }
            catch (Exception ex)
            {
                var toast = new Toast(ex.Message, TextResources.GetString("Word.Error"), ToastIcon.Error);
                ToastService.Current.Show(toast);
            }
        }


        private static FileWriteSettings ValidateFileName(ExportImageService exporter, string filename, IExportOverwritePolicy overwritePolicy)
        {
            // 出力フォルダー指定がなければ、ダイアログで出力ファイル名を確定する
            if (string.IsNullOrWhiteSpace(exporter.ExportFolder))
            {
                var dialog = new ExportImageSeveFileDialog(exporter.ExportFolder, filename, exporter.Mode == ExportImageMode.View);
                var result = dialog.ShowDialog(MainWindow.Current);
                if (result != true)
                {
                    throw new OperationCanceledException();
                }
                return new(dialog.FileName, true);
            }

            return overwritePolicy.Resolve(exporter, filename);
        }
    }

}