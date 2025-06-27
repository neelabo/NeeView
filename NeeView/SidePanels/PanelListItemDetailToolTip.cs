using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// パネルの詳細ポップアップ制御補助
    /// </summary>
    /// <remarks>
    /// パネルの種類と詳細表示設定、名前変更処理から有効状態を取得します。
    /// IsEnabled プロパティが最終的なポップアップの有効状態です。
    /// </remarks>
    public class PanelListItemDetailToolTip : BindableBase, IToolTipService, IDisposable
    {
        private readonly IHasPanelListItemStyle _panelListItemSource;
        private PanelListItemProfile? _profile;
        private bool _isToolTipEnabled = true;
        private bool _disposedValue;

        public PanelListItemDetailToolTip(IHasPanelListItemStyle panelListItemSource)
        {
            _panelListItemSource = panelListItemSource;
            _panelListItemSource.PropertyChanged += PanelListItemSource_PropertyChanged;
            AttachProfile(_panelListItemSource.PanelListItemStyle);
        }

        /// <summary>
        /// テキスト部での詳細ポップアップ有効
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (_profile == null) return false;
                return _isToolTipEnabled && _profile.IsDetailPopupEnabled;
            }
        }

        /// <summary>
        /// パネル全体での詳細ポップアップ有効
        /// </summary>
        /// <remarks>
        /// テキスト部が非表示でかつ画像ポップアップが無効な場合のみ
        /// </remarks>
        public bool IsPanelEnabled
        {
            get
            {
                if (_profile == null) return false;
                return _isToolTipEnabled && _profile.IsDetailPopupEnabled && !_profile.IsTextVisible && !_profile.IsImagePopupEnabled;
            }
        }

        // for Rename
        bool IToolTipService.IsToolTipEnabled
        {
            get { return _isToolTipEnabled; }
            set
            {
                if (SetProperty(ref _isToolTipEnabled, value))
                {
                    RaisePropertyChanged(nameof(IsEnabled));
                    RaisePropertyChanged(nameof(IsPanelEnabled));
                }
            }
        }

        private void PanelListItemSource_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IHasPanelListItemStyle.PanelListItemStyle))
            {
                AttachProfile(_panelListItemSource.PanelListItemStyle);
            }
        }

        private void AttachProfile(PanelListItemStyle panelListItemStyle)
        {
            DetachProfile();

            _profile = Config.Current.Panels.GetPanelListItemProfile(panelListItemStyle);
            if (_profile is not null)
            {
                _profile.PropertyChanged += Profile_PropertyChanged;
            }

            RaisePropertyChanged(nameof(IsEnabled));
        }

        private void DetachProfile()
        {
            if (_profile != null)
            {
                _profile.PropertyChanged -= Profile_PropertyChanged;
                _profile = null;
            }
        }

        private void Profile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var isEnabledChanged = string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(PanelListItemProfile.IsDetailPopupEnabled);

            if (isEnabledChanged)
            {
                RaisePropertyChanged(nameof(IsEnabled));
            }
            if (isEnabledChanged || e.PropertyName == nameof(PanelListItemProfile.IsTextVisible) || e.PropertyName == nameof(PanelListItemProfile.IsImagePopupEnabled))
            {
                RaisePropertyChanged(nameof(IsPanelEnabled));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DetachProfile();
                    _panelListItemSource.PropertyChanged -= PanelListItemSource_PropertyChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
