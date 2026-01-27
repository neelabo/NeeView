using NeeLaboratory.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ドライブサムネイル
    /// </summary>
    public class DriveThumbnail : BindableBase, IThumbnail
    {
        private readonly string _path;
        private ImageSource? _bitmapSource;
        private bool _initialized;


        public DriveThumbnail(string path)
        {
            _path = path;
        }


        public bool IsValid => true;
        public bool IsUniqueImage => true;
        public bool IsNormalImage => false;
        public Brush Background => Brushes.Transparent;
        public ImageSource? ImageSource => CreateImageSource();

        private ImageSource? CreateImageSource()
        {
            if (!_initialized)
            {
                _initialized = true;
                _ = DriveIconUtility.CreateDriveIconAsync(_path,
                    image =>
                    {
                        _bitmapSource = image.GetBitmapSource(256.0);
                        DriveIconUtility.SetDriveIconCache(_path, _bitmapSource);
                        RaisePropertyChanged("");
                    });
            }

            return DriveIconUtility.GetDriveIconCache(_path);
        }
    }
}
