using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class MenuBarConfig : BindableBase
    {
        private bool _isVisible;
        private bool _isHideMenu;
        private bool _isAddressBarEnabled = true;
        private bool _isHamburgerMenu;
        private bool _isHideMenuInAutoHideMode = true;
        private bool _isBookmarkDialogEnabled = true;
        private bool _isSettingsButtonEnabled = true;


        [JsonIgnore]
        [PropertyMapReadOnly]
        [PropertyMember]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        // メニューを自動的に隠す
        [PropertyMember]
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set { SetProperty(ref _isHideMenu, value); }
        }

        // メニューを自動的に隠す(自動非表示モード)
        [PropertyMember]
        public bool IsHideMenuInAutoHideMode
        {
            get { return _isHideMenuInAutoHideMode; }
            set { SetProperty(ref _isHideMenuInAutoHideMode, value); }
        }

        /// <summary>
        /// ハンバーガーメニューにする
        /// </summary>
        [PropertyMember]
        public bool IsHamburgerMenu
        {
            get { return _isHamburgerMenu; }
            set { SetProperty(ref _isHamburgerMenu, value); }
        }


        /// <summary>
        /// アドレスバーON/OFF
        /// </summary>
        [PropertyMember]
        public bool IsAddressBarEnabled
        {
            get { return _isAddressBarEnabled; }
            set { SetProperty(ref _isAddressBarEnabled, value); }
        }

        /// <summary>
        /// アドレスバーのブックマークボタンでのダイアログ表示ON/OFF
        /// </summary>
        [PropertyMember]
        public bool IsBookmarkDialogEnabled
        {
            get { return _isBookmarkDialogEnabled; }
            set { SetProperty(ref _isBookmarkDialogEnabled, value); }
        }

        /// <summary>
        /// アドレスバーの設定ボタンON/OFF
        /// </summary>
        [PropertyMember]
        public bool IsSettingsButtonEnabled
        {
            get { return _isSettingsButtonEnabled; }
            set { SetProperty(ref _isSettingsButtonEnabled, value); }
        }
    }
}


