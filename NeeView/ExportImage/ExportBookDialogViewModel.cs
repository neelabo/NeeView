using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class ExportBookDialogViewModel : BindableBase
    {
        private readonly ExportBookParameter _parameter;
        private readonly ExportImagePreview _preview;
        private string _bookName;
        private List<DestinationFolder>? _destinationFolderList;
        private DestinationFolder? _selectedDestinationFolder;


        public ExportBookDialogViewModel(ExportBookParameter parameter, string bookName)
        {
            _parameter = parameter;
            _bookName = bookName;

            var source = ExportImageSourceFactory.Create();
            _preview = new ExportImagePreview(_parameter, source);

            _parameter.OverwriteMode = _parameter.OverwriteMode == ExportImageOverwriteMode.Confirm ? ExportImageOverwriteMode.Invalid : _parameter.OverwriteMode;

            var overwriteMap = AliasNameExtensions.GetAliasNameDictionary<ExportImageOverwriteMode>()
                .Where(e => e.Key != ExportImageOverwriteMode.Confirm)
                .ToDictionary(e => (Enum)e.Key, e => e.Value);

            this.ExportBookDocument = new PropertyDocument();
            this.ExportBookDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.FileNameMode)));
            this.ExportBookDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.OverwriteMode), new() { EnumMap = overwriteMap }));
            this.ExportBookDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.Mode)));

            var viewDocument = new PropertyDocument();

            this.ExportBookDocument.AddProperty(PropertyDocumentElement.Create(viewDocument, new PropertyMemberElementOptions()
            {
                VisibilityValue = new Setting.VisibilityPropertyValue(new Binding(nameof(_parameter.Mode))
                {
                    Source = _parameter,
                    Converter = new ValueToVisibilityConverter<ExportImageMode>() { Visible = ExportImageMode.View }
                })
            }));

            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.HasBackground)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.IsOriginalSize)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.IsDotKeep)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.FileFormat)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.QualityLevel)));

            viewDocument.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            this.ExportBookTypeDocument = new PropertyDocument();
            this.ExportBookTypeDocument.AddProperty(_parameter, nameof(_parameter.BookType));

            _parameter.SubscribePropertyChanged(nameof(_parameter.BookType),
                (s, e) => RaisePropertyChanged(nameof(BookType)));

            _preview.SubscribePropertyChanged(nameof(_preview.Preview),
                (s, e) => RaisePropertyChanged(nameof(Preview)));

            _preview.SubscribePropertyChanged(nameof(_preview.PreviewWidth),
                (s, e) => RaisePropertyChanged(nameof(PreviewWidth)));

            _preview.SubscribePropertyChanged(nameof(_preview.PreviewHeight),
                (s, e) => RaisePropertyChanged(nameof(PreviewHeight)));

            _preview.SubscribePropertyChanged(nameof(_preview.ImageFormatNote),
                (s, e) => RaisePropertyChanged(nameof(ImageFormatNote)));

            UpdateDestinationFolderList();
        }


        public PropertyDocument ExportBookDocument { get; set; }
        public PropertyDocument ExportBookTypeDocument { get; set; }

        public string BookName
        {
            get { return _bookName; }
            set { _bookName = value; }
        }

        public ExportBookType BookType
        {
            get { return _parameter.BookType; }
            set { _parameter.BookType = value; }
        }

        public FrameworkElement? Preview
        {
            get { return _preview.Preview; }
        }

        public double PreviewWidth
        {
            get { return _preview.PreviewWidth; }
        }

        public double PreviewHeight
        {
            get { return _preview.PreviewHeight; }
        }

        public string ImageFormatNote
        {
            get { return _preview.ImageFormatNote; }
        }

        public List<DestinationFolder>? DestinationFolderList
        {
            get { return _destinationFolderList; }
            set { SetProperty(ref _destinationFolderList, value); }
        }

        public DestinationFolder? SelectedDestinationFolder
        {
            get { return _selectedDestinationFolder; }
            set { SetProperty(ref _selectedDestinationFolder, value); }
        }


        public void UpdateDestinationFolderList()
        {
            var oldSelect = _selectedDestinationFolder;

            var list = new List<DestinationFolder> { new DestinationFolder(TextResources.GetString("Word.None"), "") };
            list.AddRange(Config.Current.System.DestinationFolderCollection);
            DestinationFolderList = list;

            SelectedDestinationFolder = list.FirstOrDefault(e => e.Path == _parameter.ExportFolder) ?? list.First();
        }

        public async ValueTask<bool?> ShowSelectSaveFileDialogAsync(Window owner, CancellationToken token)
        {
            var initialFileName = _parameter.BookType == ExportBookType.Zip
                ? _bookName + ".zip"
                : "";

            var initialDirectory = SelectedDestinationFolder?.IsValid() == true
                ? SelectedDestinationFolder.Path
                : _parameter.ExportFolder;

            var defaultDirectory = "";

            var dialog = new ExportBookSaveFileDialog(initialDirectory, defaultDirectory, initialFileName, _parameter.BookType);

            var result = dialog.ShowDialog(owner);
            if (result != true)
            {
                return result;
            }

            _parameter.ExportBookPath = dialog.FileName;
            _parameter.ExportFolder = LoosePath.GetDirectoryName(dialog.FileName);

            Debug.Assert(_parameter.OverwriteMode != ExportImageOverwriteMode.Confirm);
            if (_parameter.OverwriteMode == ExportImageOverwriteMode.Confirm)
            {
                _parameter.OverwriteMode = ExportImageOverwriteMode.Invalid;
            }

            if (_parameter.BookType == ExportBookType.Zip)
            {
                if (FileIO.ExistsPath(_parameter.ExportBookPath))
                {
                    var allowOverwrite = ShowOverwriteConfirmDialog(owner, TextResources.GetFormatString("OverwriteFileDialog.Message", System.IO.Path.GetFileName(_parameter.ExportBookPath)), TextResources.GetString("OverwriteFileDialog.Title"));
                    if (!allowOverwrite)
                    {
                        return false;
                    }
                }
            }
            else if (_parameter.BookType == ExportBookType.Folder)
            {
                // フォルダが存在し、空でない場合は上書き確認
                var directory = new DirectoryInfo(_parameter.ExportBookPath);
                if (directory.Exists && directory.EnumerateFileSystemInfos().Any())
                {
                    var allowOverwrite = ShowOverwriteConfirmDialog(owner, TextResources.GetFormatString("OverwriteFolderDialog.Message", System.IO.Path.GetFileName(_parameter.ExportBookPath)), TextResources.GetString("OverwriteFolderDialog.Title"));
                    if (!allowOverwrite)
                    {
                        return false;
                    }
                }
            }

            return result;
        }

        private bool ShowOverwriteConfirmDialog(Window owner, string message, string title)
        {
            var confirm = new MessageDialog(message, title);
            confirm.Commands.Add(new UICommand("Word.Overwrite") { IsPossible = true });
            confirm.Commands.Add(UICommands.Cancel);
            var confirmResult = confirm.ShowDialog(owner);
            return confirmResult.IsPossible;
        }
    }
}
