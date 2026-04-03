using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    public class ExportImageDialogViewModel : BindableBase
    {
        private readonly ExportImageParameter _parameter;
        private readonly ExportImageSource _source;
        private readonly ExportImagePreview _preview;
        private List<DestinationFolder>? _destinationFolderList;
        private static DestinationFolder? _lastSelectedDestinationFolder;
        private DestinationFolder? _selectedDestinationFolder = _lastSelectedDestinationFolder;

        public ExportImageDialogViewModel(ExportImageParameter parameter, ExportImageSource source)
        {
            _parameter = parameter;
            _source = source;
            _preview = new ExportImagePreview(_parameter, source);

            this.ExportBookDocument = new PropertyDocument();
            this.ExportBookDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.FileNameMode)));
            this.ExportBookDocument.AddProperty(PropertyMemberElement.Create(_parameter, nameof(_parameter.OverwriteMode)));
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

        public string FileName { get; private set; } = "";

        public Dictionary<ExportImageMode, string> ExportImageModeList => AliasNameExtensions.GetAliasNameDictionary<ExportImageMode>();

        public PropertyDocument ExportBookDocument { get; set; }

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
            var dialog = new ExportImageSeveFileDialog(_parameter.ExportFolder,
                    CreateFileName(ExportImageFileNameMode.Default, _parameter.FileFormat),
                    _parameter.Mode == ExportImageMode.View);

            if (SelectedDestinationFolder != null && SelectedDestinationFolder.IsValid())
            {
                dialog.InitialDirectory = SelectedDestinationFolder.Path;
            }

            var result = dialog.ShowDialog(owner);
            if (result == true)
            {
                this.FileName = dialog.FileName;
            }

            return result;
        }

        public string CreateFileName(ExportImageFileNameMode fileNameMode, BitmapImageFormat format)
        {
            var fileNamePolicy = new DefaultExportImageFileNamePolicy();
            return fileNamePolicy.CreateFileName(_source, _parameter.Mode, fileNameMode, format);
        }

    }
}
