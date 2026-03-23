using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ImageStandardConfig : BindableBase
    {
        [DefaultEquality] private bool _useWicInformation = true;
        [DefaultEquality] private bool _isAspectRatioEnabled;
        [DefaultEquality] private bool _isAnimatedGifEnabled = true;
        [DefaultEquality] private bool _isAnimatedPngEnabled = true;
        [DefaultEquality] private bool _isAnimatedWebpEnabled = true;
        [DefaultEquality] private bool _isAllFileSupported;
        [IgnoreEquality] private FileTypeCollection? _supportFileTypes = null;
        [DefaultEquality] private FileTypeCollection _supportFileTypesAdd = new();
        [DefaultEquality] private FileTypeCollection _supportFileTypesExcept = new();

        // 既定の画像拡張子をWICから取得する
        [PropertyMember]
        public bool UseWicInformation
        {
            get { return _useWicInformation; }
            set { SetProperty(ref _useWicInformation, value); }
        }

        // サポートする画像ファイルの拡張子
        // JSONには保存されない (v46.0)
        [PropertyMember]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public FileTypeCollection? SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

        // 追加された画像ファイル拡張子
        public FileTypeCollection SupportFileTypesAdd
        {
            get { return _supportFileTypesAdd; }
            set { SetProperty(ref _supportFileTypesAdd, value); }
        }

        // 除外された画像ファイル拡張子
        public FileTypeCollection SupportFileTypesExcept
        {
            get { return _supportFileTypesExcept; }
            set { SetProperty(ref _supportFileTypesExcept, value); }
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
