using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ExportImageWindowViewModel : BindableBase
    {
        private readonly ExportImageParameter _model;
        private readonly ExportImageSource _source;
        private readonly ExportImagePreview _preview;
        private List<DestinationFolder>? _destinationFolderList;
        private static DestinationFolder? _lastSelectedDestinationFolder;
        private DestinationFolder? _selectedDestinationFolder = _lastSelectedDestinationFolder;

        public ExportImageWindowViewModel(ExportImageParameter model, ExportImageSource source)
        {
            _model = model;
            _source = source;
            _preview = new ExportImagePreview(_model, source);

            _model.SubscribePropertyChanged(nameof(_model.Mode),
                (s, e) => RaisePropertyChanged(nameof(Mode)));

            _model.SubscribePropertyChanged(nameof(_model.HasBackground),
                (s, e) => RaisePropertyChanged(nameof(HasBackground)));

            _model.SubscribePropertyChanged(nameof(_model.IsOriginalSize),
                (s, e) => RaisePropertyChanged(nameof(IsOriginalSize)));

            _model.SubscribePropertyChanged(nameof(_model.IsDotKeep),
                (s, e) => RaisePropertyChanged(nameof(IsDotKeep)));

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


        public Dictionary<ExportImageMode, string> ExportImageModeList => AliasNameExtensions.GetAliasNameDictionary<ExportImageMode>();

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
            var dialog = new ExportImageSeveFileDialog(_model.ExportFolder,
                    CreateFileName(ExportImageFileNameMode.Default, BitmapImageFormat.Png),
                    _model.Mode == ExportImageMode.View);

            if (SelectedDestinationFolder != null && SelectedDestinationFolder.IsValid())
            {
                dialog.InitialDirectory = SelectedDestinationFolder.Path;
            }

            var result = dialog.ShowDialog(owner);
            if (result == true)
            {
                var path = System.IO.Path.GetFullPath(dialog.FileName);

                using var exporter = ImageExporterFactory.CreateExporter(_source, _model.Mode);
                await exporter.ExportAsync(path, true, _model, token);
            }

            return result;
        }

        public string CreateFileName(ExportImageFileNameMode fileNameMode, BitmapImageFormat format)
        {
            var fileNamePolicy = new DefaultExportImageFileNamePolicy();
            return fileNamePolicy.CreateFileName(_source, Mode, fileNameMode, format);
        }

    }
}
