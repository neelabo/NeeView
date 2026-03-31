using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ExportBookDialogViewModel : BindableBase
    {
        private readonly ExportBookParameter _model;
        private readonly ExportImagePreview2 _preview;
        private List<DestinationFolder>? _destinationFolderList;
        private static DestinationFolder? _lastSelectedDestinationFolder;
        private DestinationFolder? _selectedDestinationFolder = _lastSelectedDestinationFolder;

        public ExportBookDialogViewModel(ExportBookParameter model)
        {
            _model = model;

            var source = ExportImageSourceFactory.Create();
            _preview = new ExportImagePreview2(_model, source);

            this.ExportImageViewDocument = new PropertyDocument();
            this.ExportImageViewDocument.AddProperty(_model, nameof(_model.HasBackground));
            this.ExportImageViewDocument.AddProperty(_model, nameof(_model.IsOriginalSize));
            this.ExportImageViewDocument.AddProperty(_model, nameof(_model.IsDotKeep));
            this.ExportImageViewDocument.AddProperty(_model, nameof(_model.FileFormat));
            this.ExportImageViewDocument.AddProperty(_model, nameof(_model.QualityLevel));
            this.ExportImageViewDocument.AddProperty(_model, nameof(_model.FileNameMode));
            this.ExportImageViewDocument.AddProperty(_model, nameof(_model.OverwriteMode));
            this.ExportImageViewDocument.SetVisualType<PropertyValue_Boolean>(PropertyVisualType.ToggleSwitch);
            this.ExportImageViewDocument.SetVisualType<PropertyValue_Color>(PropertyVisualType.ComboColorPicker);


            _model.SubscribePropertyChanged(nameof(_model.Mode),
                (s, e) => RaisePropertyChanged(nameof(Mode)));

            _model.SubscribePropertyChanged(nameof(_model.HasBackground),
                (s, e) => RaisePropertyChanged(nameof(HasBackground)));

            _model.SubscribePropertyChanged(nameof(_model.IsOriginalSize),
                (s, e) => RaisePropertyChanged(nameof(IsOriginalSize)));

            _model.SubscribePropertyChanged(nameof(_model.IsDotKeep),
                (s, e) => RaisePropertyChanged(nameof(IsDotKeep)));

            _model.SubscribePropertyChanged(nameof(_model.BookType),
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


        public PropertyDocument ExportImageViewDocument { get; set; }

        public Dictionary<ExportImageMode, string> ExportImageModeList => AliasNameExtensions.GetAliasNameDictionary<ExportImageMode>();
        public Dictionary<ExportBookType, string> ExportBookTypeList => AliasNameExtensions.GetAliasNameDictionary<ExportBookType>();

        public ExportImageMode Mode
        {
            get { return _model.Mode; }
            set { _model.Mode = value; }
        }

        public bool HasBackground
        {
            get { return _model.HasBackground; }
            set { _model.HasBackground = value; }
        }

        public bool IsOriginalSize
        {
            get { return _model.IsOriginalSize; }
            set { _model.IsOriginalSize = value; }
        }

        public bool IsDotKeep
        {
            get { return _model.IsDotKeep; }
            set { _model.IsDotKeep = value; }
        }

        public ExportBookType BookType
        {
            get { return _model.BookType; }
            set { _model.BookType = value; }
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


        // NOTE: 未使用？
        public List<DestinationFolder>? DestinationFolderList
        {
            get { return _destinationFolderList; }
            set { SetProperty(ref _destinationFolderList, value); }
        }

        public DestinationFolder? SelectedDestinationFolder
        {
            get { return _selectedDestinationFolder; }
            set
            {
                if (SetProperty(ref _selectedDestinationFolder, value))
                {
                    _lastSelectedDestinationFolder = _selectedDestinationFolder;
                }
            }
        }

        public void UpdateDestinationFolderList()
        {
            var oldSelect = _selectedDestinationFolder;

            var list = new List<DestinationFolder> { new DestinationFolder(TextResources.GetString("Word.None"), "") };
            list.AddRange(Config.Current.System.DestinationFolderCollection);
            DestinationFolderList = list;

            SelectedDestinationFolder = list.FirstOrDefault(e => e.Equals(oldSelect)) ?? list.First();
        }

        public async ValueTask<bool?> ShowSelectSaveFileDialogAsync(Window owner, CancellationToken token)
        {
            var initialFileName = _model.BookType == ExportBookType.Zip
                ? _model.BookName + ".zip"
                : "";

            var initialDirectory = SelectedDestinationFolder?.IsValid() == true
                ? SelectedDestinationFolder.Path
                : "";

            var dialog = new ExportBookSaveFileDialog(initialDirectory, initialFileName, _model.BookType);

            //if (_model.BookType == ExportBookType.Folder)
            //{
            //    Clipboard.SetText(_model.BookName);
            //}

            var result = dialog.ShowDialog(owner);
            if (result != true)
            {
                return result;
            }

            _model.ExportFolder = dialog.FileName;

            if (_model.BookType == ExportBookType.Folder)
            {
                // フォルダが存在し、空でない場合は上書き確認
                var directory = new DirectoryInfo(_model.ExportFolder);
                if (directory.Exists && directory.EnumerateFileSystemInfos().Any())
                {
                    var confirm = new MessageDialog(TextResources.GetFormatString("OverwriteFolderDialog.Message", System.IO.Path.GetFileName(_model.ExportFolder)), TextResources.GetString("OverwriteFolderDialog.Title"));
                    confirm.Commands.Add(new UICommand(TextResources.GetString("Word.Overwrite")) { IsPossible = true });
                    confirm.Commands.Add(UICommands.Cancel);
                    var confirmResult = confirm.ShowDialog(owner);
                    if (!confirmResult.IsPossible)
                    {
                        return false;
                    }
                }
            }

            return result;
        }
    }
}
