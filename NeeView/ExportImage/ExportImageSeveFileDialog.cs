using Microsoft.Win32;
using NeeView.Properties;
using System;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public class ExportImageSeveFileDialog
    {
        public ExportImageSeveFileDialog(string? initialDirectory, string fileName, bool canSelectFormat, bool overwritePrompt)
        {
            InitialDirectory = initialDirectory;
            FileName = fileName;
            CanSelectFormat = canSelectFormat;
            OverwritePrompt = overwritePrompt;
        }


        public string? InitialDirectory { get; set; }

        public string FileName { get; set; }

        public bool CanSelectFormat { get; set; }

        public bool OverwritePrompt { get;  set; }


        public bool? ShowDialog(Window owner)
        {
            try
            {
                var dialog = CreateSaveFileDialog(FileName, ValidateDirectoryPath(InitialDirectory), CanSelectFormat, OverwritePrompt);
                var result = dialog.ShowDialog(owner);
                FileName = dialog.FileName;
                return result;
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
                return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            }
        }

        private static SaveFileDialog CreateSaveFileDialog(string filename, string directory, bool canSelectFormat, bool overwritePrompt)
        {
            var dialog = new SaveFileDialog();

            dialog.InitialDirectory = directory;
            dialog.OverwritePrompt = overwritePrompt;

            dialog.AddExtension = true;

            var defaultExt = LoosePath.GetExtension(filename);
            dialog.DefaultExt = defaultExt;

            var fileName = LoosePath.ValidFileName(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(filename), defaultExt));
            dialog.FileName = fileName;

            if (canSelectFormat)
            {
                var pngExt = new string[] { ".png" };
                var jpgExt = new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" };

                string filter = "PNG|*.png|JPEG|*.jpg;*.jpeg;*.jpe;*.jfif";

                if (pngExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 1;
                }
                else if (jpgExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 2;
                }

                dialog.Filter = filter + "|All|*.*";
            }

            return dialog;
        }
    }
}
