using Microsoft.Win32;
using NeeView.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows.Controls
{
    public class FileNameBox : Control
    {
        static FileNameBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileNameBox), new FrameworkPropertyMetadata(typeof(FileNameBox)));
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var textBox = this.GetTemplateChild("PART_PathTextBox") as TextBox ?? throw new InvalidOperationException();
            textBox.PreviewDragOver += PathTextBox_PreviewDragOver;
            textBox.Drop += PathTextBox_Drop;

            var button = this.GetTemplateChild("PART_ButtonOpenDialog") as Button ?? throw new InvalidOperationException();
            button.Click += ButtonOpenDialog_Click;

            UpdateEmptyMessage();
        }


        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(FileNameBox), new PropertyMetadata(null));

        public static readonly DependencyProperty DefaultDirectoryProperty =
            DependencyProperty.Register(
            "DefaultDirectory",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDefaultDirectoryChanged)));

        public string DefaultDirectory
        {
            get { return (string)GetValue(DefaultDirectoryProperty); }
            set { SetValue(DefaultDirectoryProperty, value); }
        }

        private static void OnDefaultDirectoryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
            "IsValid",
            typeof(bool),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsValidChanged)));

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        private static void OnIsValidChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        public FileDialogType FileDialogType
        {
            get { return (FileDialogType)GetValue(FileDialogTypeProperty); }
            set { SetValue(FileDialogTypeProperty, value); }
        }

        public static readonly DependencyProperty FileDialogTypeProperty =
            DependencyProperty.Register("FileDialogType", typeof(FileDialogType), typeof(FileNameBox), new PropertyMetadata(FileDialogType.OpenFile, OnFileDialogTypeChanged));

        private static void OnFileDialogTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileNameBox control)
            {
                control.UpdateEmptyMessage();
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTitleChanged)));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private static void OnTitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
            "Filter",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFilterChanged)));

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        private static void OnFilterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register(
            "Note",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNoteChanged)));

        public string Note
        {
            get { return (string)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        private static void OnNoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileNameBox control)
            {
                control.UpdateEmptyMessage();
            }
        }


        public string EmptyMessage
        {
            get { return (string)GetValue(EmptyMessageProperty); }
            set { SetValue(EmptyMessageProperty, value); }
        }

        public static readonly DependencyProperty EmptyMessageProperty =
            DependencyProperty.Register("EmptyMessage", typeof(string), typeof(FileNameBox), new PropertyMetadata(""));


        public ImageSource? FolderIcon
        {
            get { return (ImageSource)GetValue(FolderIconProperty); }
            set { SetValue(FolderIconProperty, value); }
        }

        public static readonly DependencyProperty FolderIconProperty =
            DependencyProperty.Register("FolderIcon", typeof(ImageSource), typeof(FileNameBox), new PropertyMetadata(null));


        private void UpdateEmptyMessage()
        {
            EmptyMessage = Note ?? (FileDialogType == FileDialogType.Directory ? TextResources.GetString("FileNameBox.Directory.Message") : TextResources.GetString("FileNameBox.File.Message"));
        }

        private void ButtonOpenDialog_Click(object sender, RoutedEventArgs e)
        {
            var path = Text ?? "";
            var owner = Window.GetWindow(this);

            // check path chars
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            var invalidCharsIndex = path.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                path = "";
            }

            if (FileDialogType == FileDialogType.Directory)
            {
                var dialog = new OpenFolderDialog();
                dialog.Title = Title ?? TextResources.GetString("FileNameBox.SelectDirectory");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    dialog.InitialDirectory = LoosePath.GetDirectoryName(path);
                    dialog.FolderName = LoosePath.GetFileName(path);
                }

                var result = dialog.ShowDialog(owner);
                if (result == true)
                {
                    path = dialog.FolderName;
                }
            }
            else if (FileDialogType == FileDialogType.SaveFile)
            {
                var dialog = new SaveFileDialog();
                dialog.Title = Title ?? TextResources.GetString("FileNameBox.SelectFile");
                dialog.InitialDirectory = string.IsNullOrWhiteSpace(path) ? null : Path.GetDirectoryName(path);
                dialog.FileName = string.IsNullOrWhiteSpace(path) ? DefaultText : Path.GetFileName(path);
                dialog.Filter = Filter;
                dialog.OverwritePrompt = false;
                dialog.CreatePrompt = false;

                var result = dialog.ShowDialog(owner);
                if (result == true)
                {
                    path = dialog.FileName;
                }
            }
            else
            {
                var dialog = new OpenFileDialog();
                dialog.Title = Title ?? TextResources.GetString("FileNameBox.SelectFile");
                dialog.InitialDirectory = string.IsNullOrWhiteSpace(path) ? null : Path.GetDirectoryName(path);
                dialog.FileName = string.IsNullOrWhiteSpace(path) ? DefaultText : Path.GetFileName(path);
                dialog.Filter = Filter;

                var result = dialog.ShowDialog(owner);
                if (result == true)
                {
                    path = dialog.FileName;
                }
            }

            Text = path;
        }

        private void PathTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void PathTextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetNormalizedFileDrop() is not string[] dropFiles) return;
            if (dropFiles.Length == 0) return;

            if (FileDialogType == FileDialogType.Directory)
            {
                if (Directory.Exists(dropFiles[0]))
                {
                    Text = dropFiles[0];
                }
                else
                {
                    Text = Path.GetDirectoryName(dropFiles[0]) ?? "";
                }
            }
            else
            {
                Text = dropFiles[0];
            }
        }

    }
}
