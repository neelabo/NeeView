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
    public class ExportBookDialogViewModel : BindableBase, IDisposable
    {
        private readonly ExportBookParameter _parameter;
        private readonly ExportImagePreview _preview;
        private string _bookName;
        private List<DestinationFolder>? _destinationFolderList;
        private DestinationFolder? _selectedDestinationFolder;
        private bool _disposedValue;
        private DisposableCollection _disposables = new DisposableCollection();

        public ExportBookDialogViewModel(ExportBookParameter parameter, string bookName)
        {
            _parameter = parameter;
            _bookName = bookName;

            var source = ExportImageSourceFactory.Create();
            _preview = new ExportImagePreview(_parameter, source);

            _disposables.Add(_preview);

            _parameter.OverwriteMode = _parameter.OverwriteMode == ExportImageOverwriteMode.Confirm ? ExportImageOverwriteMode.Disallow : _parameter.OverwriteMode;

            var overwriteMap = AliasNameExtensions.GetAliasNameDictionary<ExportImageOverwriteMode>()
                .Where(e => e.Key != ExportImageOverwriteMode.Confirm)
                .ToDictionary(e => (Enum)e.Key, e => e.Value);

            this.ExportBookDocument = new PropertyDocument();
            this.ExportBookDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.Mode)));


            var originalDocument = new PropertyDocument();

            this.ExportBookDocument.AddProperty(PropertyDocumentElement.Create(originalDocument, new PropertyMemberElementOptions()
            {
                VisibilityValue = new Setting.VisibilityPropertyValue(new Binding(nameof(_parameter.Mode))
                {
                    Source = _parameter,
                    Converter = new ValueToVisibilityConverter<ExportImageMode>() { Visible = ExportImageMode.Original }
                })
            }));

            var fileNameFormatModel0 = new ExportFileNameFormatModel(_parameter, ProxyProperty.Create((ExportImageParameter)_parameter, e => e.FileNameFormat0), ExportFileNameFormat.CreateDummyFileNameSource(1, 1), ExportBookParameter.DefaultBookFileNameFormat0);

            originalDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.FileNameFormat0), data: fileNameFormatModel0));
            originalDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.OverwriteMode), options: new() { EnumMap = overwriteMap }));

            var viewDocument = new PropertyDocument();

            this.ExportBookDocument.AddProperty(PropertyDocumentElement.Create(viewDocument, new PropertyMemberElementOptions()
            {
                VisibilityValue = new Setting.VisibilityPropertyValue(new Binding(nameof(_parameter.Mode))
                {
                    Source = _parameter,
                    Converter = new ValueToVisibilityConverter<ExportImageMode>() { Visible = ExportImageMode.View }
                })
            }));

            var direction = source.PageFrameContent.ViewContentsDirection;
            var fileNameFormatModel1 = new ExportFileNameFormatModel(_parameter, ProxyProperty.Create((ExportImageParameter)_parameter, e => e.FileNameFormat1), ExportFileNameFormat.CreateDummyFileNameSource(1, direction), ExportBookParameter.DefaultBookFileNameFormat1);
            var fileNameFormatModel2 = new ExportFileNameFormatModel(_parameter, ProxyProperty.Create((ExportImageParameter)_parameter, e => e.FileNameFormat2), ExportFileNameFormat.CreateDummyFileNameSource(2, direction), ExportBookParameter.DefaultBookFileNameFormat2, TextResources.GetString("Word.SameAsAbove"));

            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.FileNameFormat1), data: fileNameFormatModel1));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.FileNameFormat2), data: fileNameFormatModel2));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.OverwriteMode), options: new() { EnumMap = overwriteMap }));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.HasBackground)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.IsOriginalSize)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.IsDotKeep)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.FileFormat)));
            viewDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.QualityLevel)));

            viewDocument.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);

            this.ExportBookTypeDocument = new PropertyDocument();
            this.ExportBookTypeDocument.AddProperty(_parameter, nameof(_parameter.BookType));

            _disposables.Add(_parameter.SubscribePropertyChanged(nameof(_parameter.BookType),
                (s, e) => RaisePropertyChanged(nameof(BookType))));

            _disposables.Add(_preview.SubscribePropertyChanged(nameof(_preview.Preview),
                (s, e) => RaisePropertyChanged(nameof(Preview))));

            _disposables.Add(_preview.SubscribePropertyChanged(nameof(_preview.PreviewWidth),
                (s, e) => RaisePropertyChanged(nameof(PreviewWidth))));

            _disposables.Add(_preview.SubscribePropertyChanged(nameof(_preview.PreviewHeight),
                (s, e) => RaisePropertyChanged(nameof(PreviewHeight))));

            _disposables.Add(_preview.SubscribePropertyChanged(nameof(_preview.ImageFormatNote),
                (s, e) => RaisePropertyChanged(nameof(ImageFormatNote))));

            UpdateDestinationFolderList();
        }


        public string FileName { get; private set; } = "";

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
            var list = new List<DestinationFolder> { new DestinationFolder(TextResources.GetString("Word.None"), "") };
            list.AddRange(Config.Current.System.DestinationFolderCollection);
            DestinationFolderList = list;

            SelectedDestinationFolder = list.FirstOrDefault(e => e.Path == _parameter.ExportFolder) ?? list.First();
        }

        public async Task<bool?> ShowSelectSaveFileDialogAsync(Window owner, CancellationToken token)
        {
            var initialFileName = _parameter.BookType == ExportBookType.Zip
                ? _bookName + ".zip"
                : _bookName;

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

            _parameter.ExportFolder = LoosePath.GetDirectoryName(dialog.FileName);

            Debug.Assert(_parameter.OverwriteMode != ExportImageOverwriteMode.Confirm);
            if (_parameter.OverwriteMode == ExportImageOverwriteMode.Confirm)
            {
                _parameter.OverwriteMode = ExportImageOverwriteMode.Disallow;
            }

            if (_parameter.BookType == ExportBookType.Zip)
            {
                // ファイルが存在する場合は上書き確認
                if (FileIO.EntryExists(dialog.FileName))
                {
                    var allowOverwrite = ShowOverwriteConfirmDialog(owner, TextResources.GetString("OverwriteFileDialog.Title"), TextResources.GetFormatString("OverwriteFileDialog.Message", System.IO.Path.GetFileName(dialog.FileName)));
                    if (!allowOverwrite)
                    {
                        return false;
                    }
                }
            }
            else if (_parameter.BookType == ExportBookType.Folder)
            {
                // フォルダが存在し、空でない場合は上書き確認
                var directory = new DirectoryInfo(dialog.FileName);
                if (FileIO.Exists(directory) && directory.EnumerateFileSystemInfos().Any())
                {
                    var allowOverwrite = ShowOverwriteConfirmDialog(owner, TextResources.GetString("OverwriteFolderDialog.Title"), TextResources.GetFormatString("OverwriteFolderDialog.Message", System.IO.Path.GetFileName(dialog.FileName)));
                    if (!allowOverwrite)
                    {
                        return false;
                    }
                }
            }

            this.FileName = dialog.FileName;

            return result;
        }

        private bool ShowOverwriteConfirmDialog(Window owner, string caption, string message)
        {
            var confirm = new MessageDialog(caption, message, MessageDialogIcon.Warning);
            confirm.Title = TextResources.GetString("ExportBookAsCommand");
            confirm.Commands.Add(new UICommand("Word.Overwrite") { IsPossible = true, IsDanger = true });
            confirm.Commands.Add(UICommands.Cancel);
            confirm.DefaultCommandIndex = 1;
            var confirmResult = confirm.ShowDialog(owner);
            return confirmResult.IsPossible;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
