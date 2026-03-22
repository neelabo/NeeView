using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class SusieConfig : BindableBaseFull
    {
        private bool _isEnabled;
        private bool _isFirstOrderSusieImage;
        private bool _isFirstOrderSusieArchive;
        private string _susiePluginPath = "";


        /// <summary>
        /// Susie 有効/無効設定
        /// </summary>
        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        // Susie プラグインフォルダー
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string SusiePluginPath
        {
            get { return _susiePluginPath; }
            set { SetProperty(ref _susiePluginPath, value?.Trim() ?? ""); }
        }

        // Susie 画像プラグイン 優先フラグ
        [PropertyMember]
        public bool IsFirstOrderSusieImage
        {
            get { return _isFirstOrderSusieImage; }
            set { SetProperty(ref _isFirstOrderSusieImage, value); }
        }

        // Susie 書庫プラグイン 優先フラグ
        [PropertyMember]
        public bool IsFirstOrderSusieArchive
        {
            get { return _isFirstOrderSusieArchive; }
            set { SetProperty(ref _isFirstOrderSusieArchive, value); }
        }

    }
}
