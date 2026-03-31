using Microsoft.Win32;
using NeeView.Properties;
using System;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public class ExportBookSaveFileDialog
    {
        private static string _lastFolder = "";


        public ExportBookSaveFileDialog(string? initialDirectory, string fileName, ExportBookType bookType)
        {
            InitialDirectory = initialDirectory;
            FileName = fileName;
            ExportBookType = bookType;
        }


        public string? InitialDirectory { get; set; }

        public string FileName { get; set; }

        public ExportBookType ExportBookType { get; set; }


        public bool? ShowDialog(Window owner)
        {
            try
            {
                if (ExportBookType == ExportBookType.Folder)
                {
                    var dialog = CreateSaveFolderDialog(ValidateDirectoryPath(InitialDirectory), FileName);
                    var result = dialog.ShowDialog(owner);
                    FileName = dialog.FolderName;

                    if (result == true)
                    {
                        _lastFolder = System.IO.Path.GetDirectoryName(dialog.FolderName) ?? "";
                    }

                    return result;
                }
                else
                {
                    var dialog = CreateSaveFileDialog(ValidateDirectoryPath(InitialDirectory), FileName);
                    var result = dialog.ShowDialog(owner);
                    FileName = dialog.FileName;

                    if (result == true)
                    {
                        _lastFolder = System.IO.Path.GetDirectoryName(dialog.FileName) ?? "";
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                new MessageDialog($"{TextResources.GetString("ImageExportErrorDialog.Message")}\n{TextResources.GetString("Word.Cause")}: {ex.Message}", TextResources.GetString("ImageExportErrorDialog.Title")).ShowDialog();
                return false;
            }
        }

        private static string ValidateDirectoryPath(string? path)
        {
            if (System.IO.Directory.Exists(path))
            {
                return path;
            }
            else
            {
                return ""; // System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            }
        }

        private static SaveFileDialog CreateSaveFileDialog(string directory, string filename)
        {
            var dialog = new SaveFileDialog();

            dialog.InitialDirectory = string.IsNullOrEmpty(directory) ? _lastFolder : directory;
            dialog.DefaultDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);

            dialog.Title = $"{TextResources.GetString("ExportBookAsCommand")} ({TextResources.GetString("ExportBookType.Zip")})";

            dialog.OverwritePrompt = true;

            dialog.AddExtension = true;

            var defaultExt = LoosePath.GetExtension(filename);
            dialog.DefaultExt = defaultExt;

            var fileName = LoosePath.ValidFileName(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(filename), defaultExt));
            dialog.FileName = fileName;

            dialog.Filter = "ZIP|*.zip|All|*.*";
            dialog.FilterIndex = 1;

            return dialog;
        }

        public static OpenFolderDialog CreateSaveFolderDialog(string directory, string filename)
        {
            var dialog = new OpenFolderDialog();

            dialog.InitialDirectory = string.IsNullOrEmpty(directory) ? _lastFolder : directory;
            dialog.DefaultDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);

            dialog.Title = $"{TextResources.GetString("ExportBookAsCommand")} ({TextResources.GetString("ExportBookType.Folder")})";

            dialog.FolderName = filename;

            return dialog;
        }
    }
}
