using NeeLaboratory.Generators;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// 動画ページのリピートフラグのみ動画設定を使用する動画コントロール情報
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PageMediaContext : IMediaContext
    {
        public static PageMediaContext Current { get; } = new PageMediaContext();

        private readonly MediaArchiveConfig _mediaConfig;
        private readonly ImageConfig _imageConfig;


        private PageMediaContext()
        {
            _mediaConfig = Config.Current.Archive.Media;
            _mediaConfig.PropertyChanged += MediaConfig_PropertyChanged;

            _imageConfig = Config.Current.Image;
            _imageConfig.PropertyChanged += ImageConfig_PropertyChanged;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsMuted
        {
            get => _mediaConfig.IsMuted;
            set => _mediaConfig.IsMuted = value;
        }

        public double Volume
        {
            get => _mediaConfig.Volume;
            set => _mediaConfig.Volume = value;
        }

        public bool IsRepeat
        {
            get => _imageConfig.IsMediaRepeat;
            set => _imageConfig.IsMediaRepeat = value;
        }

        public double MediaStartDelaySeconds
        {
            get => 0.02;
            set { }
        }


        private void MediaConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MediaArchiveConfig.IsMuted):
                    RaisePropertyChanged(nameof(IsMuted));
                    break;

                case nameof(MediaArchiveConfig.Volume):
                    RaisePropertyChanged(nameof(Volume));
                    break;
            }
        }

        private void ImageConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImageConfig.IsMediaRepeat):
                    RaisePropertyChanged(nameof(IsRepeat));
                    break;
            }
        }

    }

}
