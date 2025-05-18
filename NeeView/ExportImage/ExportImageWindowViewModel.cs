using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ExportImageWindowViewModel : BindableBase
    {
        private readonly ExportImage _model;
        private List<DestinationFolder>? _destinationFolderList;
        private static DestinationFolder? _lastSelectedDestinationFolder;
        private DestinationFolder? _selectedDestinationFolder = _lastSelectedDestinationFolder;

        public ExportImageWindowViewModel(ExportImage model)
        {
            _model = model;
            _model.PropertyChanged += Model_PropertyChanged;

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
            get { return _model.Preview; }
        }

        public double PreviewWidth
        {
            get { return _model.PreviewWidth; }
        }

        public double PreviewHeight
        {
            get { return _model.PreviewHeight; }
        }

        public string ImageFormatNote
        {
            get { return _model.ImageFormatNote; }
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

            var list = new List<DestinationFolder> { new DestinationFolder(Properties.TextResources.GetString("Word.None"), "") };
            list.AddRange(Config.Current.System.DestinationFolderCollection);
            DestinationFolderList = list;

            SelectedDestinationFolder = list.FirstOrDefault(e => e.Equals(oldSelect)) ?? list.First();
        }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_model.Mode):
                    RaisePropertyChanged(nameof(Mode));
                    break;

                case nameof(_model.HasBackground):
                    RaisePropertyChanged(nameof(HasBackground));
                    break;

                case nameof(_model.Preview):
                    RaisePropertyChanged(nameof(Preview));
                    break;

                case nameof(_model.PreviewWidth):
                    RaisePropertyChanged(nameof(PreviewWidth));
                    break;

                case nameof(_model.PreviewHeight):
                    RaisePropertyChanged(nameof(PreviewHeight));
                    break;

                case nameof(_model.ImageFormatNote):
                    RaisePropertyChanged(nameof(ImageFormatNote));
                    break;
            }
        }

        public void UpdatePreview()
        {
            _model.UpdatePreview();
        }

        public async ValueTask<bool?> ShowSelectSaveFileDialogAsync(Window owner, CancellationToken token)
        {
            var dialog = new ExportImageSeveFileDialog(_model.ExportFolder,
                _model.CreateFileName(ExportImageFileNameMode.Default, ExportImageFormat.Png),
                _model.Mode == ExportImageMode.View);

            if (SelectedDestinationFolder != null && SelectedDestinationFolder.IsValid())
            {
                dialog.InitialDirectory = SelectedDestinationFolder.Path;
            }

            var result = dialog.ShowDialog(owner);
            if (result == true)
            {
                await _model.ExportAsync(dialog.FileName, true, token);
            }

            return result;
        }
    }
}
