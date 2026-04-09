using Microsoft.Win32;
using NeeView.Properties;
using System;
using System.Windows;

namespace NeeView
{
    public class ExportBookSaveFileDialog
    {
        public ExportBookSaveFileDialog(string initialDirectory, string defaultDirectory, string fileName, ExportBookType bookType)
        {
            InitialDirectory = ValidateDirectoryPath(initialDirectory);
            DefaultDirectory = ValidateDirectoryPath(defaultDirectory);
            FileName = fileName;
            ExportBookType = bookType;
        }


        public string InitialDirectory { get; }
        public string DefaultDirectory { get; }
        public string FileName { get; private set; }
        public ExportBookType ExportBookType { get; }


        public bool? ShowDialog(Window owner)
        {
            try
            {
                if (ExportBookType == ExportBookType.Folder)
                {
                    var dialog = CreateSaveFolderDialog(InitialDirectory, DefaultDirectory, FileName);
                    var result = dialog.ShowDialog(owner);
                    FileName = dialog.FolderName;
                    return result;
                }
                else
                {
                    var dialog = CreateSaveFileDialog(InitialDirectory, DefaultDirectory, FileName);
                    var result = dialog.ShowDialog(owner);
                    FileName = dialog.FileName;
                    return result;
                }
            }
            catch (Exception ex)
            {
                new MessageDialog(TextResources.GetString("ImageExportErrorDialog.Title"), $"{TextResources.GetString("ImageExportErrorDialog.Message")}\n{TextResources.GetString("Word.Cause")}: {ex.Message}").ShowDialog();
                return false;
            }
        }

        private static string ValidateDirectoryPath(string? path)
        {
            if (FileIO.DirectoryExists(path))
            {
                return path;
            }
            else
            {
                return ""; // System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            }
        }

        private static SaveFileDialog CreateSaveFileDialog(string initialDirectory, string defaultDirectory, string filename)
        {
            var dialog = new SaveFileDialog();

            dialog.InitialDirectory = initialDirectory;
            dialog.DefaultDirectory = defaultDirectory;

            dialog.Title = $"{TextResources.GetString("ExportBookAsCommand")} ({TextResources.GetString("Word.Zip")})";

            dialog.OverwritePrompt = false;
            dialog.AddExtension = true;

            dialog.FileName = filename;
            dialog.DefaultExt = ".zip";
            dialog.Filter = "ZIP|*.zip|All|*.*";
            dialog.FilterIndex = 1;

            return dialog;
        }

        public static OpenFolderDialog CreateSaveFolderDialog(string initialDirectory, string defaultDirectory, string filename)
        {
            var dialog = new OpenFolderDialog();

            dialog.InitialDirectory = initialDirectory;
            dialog.DefaultDirectory = defaultDirectory;

            dialog.Title = $"{TextResources.GetString("ExportBookAsCommand")} ({TextResources.GetString("Word.Folder")})";

            dialog.FolderName = filename;

            return dialog;
        }
    }
}
