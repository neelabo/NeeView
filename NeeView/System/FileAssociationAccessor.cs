using NeeLaboratory.Generators;
using NeeLaboratory.Windows.Input;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class FileAssociationAccessor : INotifyPropertyChanged, IFileAssociation
    {
        private readonly FileAssociation _source;
        private readonly FileAssociationIconBitmapCache _cache;
        private bool _isEnabled;
        private FileAssociationIcon _icon;


        public FileAssociationAccessor(FileAssociation source, FileAssociationIconBitmapCache cache)
        {
            _source = source;
            _cache = cache;
            _isEnabled = _source.IsEnabled;
            _icon = _source.Icon;

            ChangeIconCommand = new RelayCommand(ChangeIconCommand_Execute);
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public bool IsDirty
        {
            get { return _isEnabled != _source.IsEnabled || _icon != _source.Icon; }
        }

        public FileAssociationCategory Category => _source.Category;

        public string Extension => _source.Extension;

        public string? Description => _source.Description;

        public FileAssociationIcon Icon
        {
            get { return _icon; }
            set
            {
                if (SetProperty(ref _icon, value))
                {
                    RaisePropertyChanged(nameof(BitmapSource));
                }
            }
        }

        public BitmapSource? BitmapSource
        {
            get { return _cache.GetBitmapSource(_icon); }
        }


        public RelayCommand ChangeIconCommand { get; }


        private void ChangeIconCommand_Execute()
        {
            var icon = FileAssociationTools.ShowIconDialog(Icon);
            if (icon is not null)
            {
                Icon = icon;
            }
        }

        public bool Flush()
        {
            if (!IsDirty) return false;

            _source.IsEnabled = _isEnabled;
            _source.Icon = _icon;
            return true;
        }

        public void InitializeValue()
        {
            IsEnabled = false;
            Icon = new FileAssociationIcon(Category);
        }

    }
}