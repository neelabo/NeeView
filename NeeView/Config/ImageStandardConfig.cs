using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ImageStandardConfig: BindableBase
    {
        private bool _useWicInformation = true;
        private bool _isAspectRatioEnabled;
        private bool _isAnimatedGifEnabled = true;
        private bool _isAnimatedPngEnabled = true;
        private bool _isAnimatedWebpEnabled = true;
        private bool _isAllFileSupported;
        private FileTypeCollection? _supportFileTypes = null;

        // 既定の画像拡張子をWICから取得する
        [PropertyMember]
        public bool UseWicInformation
        {
            get { return _useWicInformation; }
            set { SetProperty(ref _useWicInformation, value); }
        }

        // サポートする画像ファイルの拡張子。nullの場合は既定の画像拡張子が適用される
        [PropertyMember]
        public FileTypeCollection? SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

        // 画像の解像度情報を表示に反映する
        [PropertyMember]
        public bool IsAspectRatioEnabled
        {
            get { return _isAspectRatioEnabled; }
            set { SetProperty(ref _isAspectRatioEnabled, value); }
        }

        // GIFアニメ有効
        [PropertyMember]
        public bool IsAnimatedGifEnabled
        {
            get { return _isAnimatedGifEnabled; }
            set { SetProperty(ref _isAnimatedGifEnabled, value); }
        }

        // PNGアニメ有効
        [PropertyMember]
        public bool IsAnimatedPngEnabled
        {
            get { return _isAnimatedPngEnabled; }
            set { SetProperty(ref _isAnimatedPngEnabled, value); }
        }

        // WEBPアニメ有効
        [PropertyMember]
        public bool IsAnimatedWebpEnabled
        {
            get { return _isAnimatedWebpEnabled; }
            set { SetProperty(ref _isAnimatedWebpEnabled, value); }
        }

        // サポート外ファイル有効のときに、すべてのファイルを画像とみなす
        [PropertyMember]
        public bool IsAllFileSupported
        {
            get { return _isAllFileSupported; }
            set { SetProperty(ref _isAllFileSupported, value); }
        }

    }
}
