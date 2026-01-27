using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame環境パラメータ
    /// TODO: PageFramesPanel 共通のパラメータのようなものにしたい
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PageFrameProfile : INotifyPropertyChanged, IStaticFrame, IDisposable
    {
        /// <summary>
        /// キャンバスサイズをリファレンスサイズに設定する許可
        /// </summary>
        public static Locker ReferenceSizeLocker { get; } = new Locker();


        public const double MinWidth = 32.0;
        public const double MinHeight = 32.0;

        private readonly Config _config;
        private readonly BookConfig _bookConfig;
        private readonly BookSettingConfig _settingConfig;
        private readonly MainViewConfig _mainViewConfig;
        private Size _canvasSize;
        private DpiScale _dpiScale;
        private bool disposedValue;


        public PageFrameProfile(Config config)
        {
            _config = config;
            _bookConfig = _config.Book;
            _settingConfig = _config.BookSetting;
            _mainViewConfig = _config.MainView;
            _canvasSize = new Size(MinWidth, MinHeight);

            _bookConfig.PropertyChanged += BookConfig_PropertyChanged;
            _settingConfig.PropertyChanged += BookSettingConfig_PropertyChanged;
            _mainViewConfig.PropertyChanged += MainViewConfig_PropertyChanged;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsStaticFrame => !_config.Book.IsPanorama;

        public double FrameMargin => _config.Book.FrameSpace;

        public Size CanvasSize
        {
            get { return _canvasSize; }
            set
            {
                if (SetProperty(ref _canvasSize, value))
                {
                    if (!ReferenceSizeLocker.IsLocked)
                    {
                        ResetReferenceSize();
                    }
                    else if (_mainViewConfig.ReferenceSize.IsEmptyOrZero())
                    {
                        ReferenceSizeLocker.ForceUnlock();
                        ResetReferenceSize();
                    }
                }
            }
        }

        /// <summary>
        /// フレームサイズ計算の基準となるサイズ。
        /// 基本的にキャンバスサイズと一致
        /// </summary>
        public Size ReferenceSize
        {
            get
            {
                // ウィンドウモードで自動ストレッチ時のみ専用値を使用する
                if (_mainViewConfig.IsFloating && _mainViewConfig.IsAutoStretch)
                {
                    return _mainViewConfig.ReferenceSize.IsEmptyOrZero() ? _canvasSize : _mainViewConfig.ReferenceSize;
                }
                else
                {
                    return _canvasSize;
                }
            }
        }


        public DpiScale DpiScale
        {
            get => _dpiScale;
            set => SetProperty(ref _dpiScale, value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _bookConfig.PropertyChanged -= BookConfig_PropertyChanged;
                    _settingConfig.PropertyChanged -= BookSettingConfig_PropertyChanged;
                    _mainViewConfig.PropertyChanged -= MainViewConfig_PropertyChanged;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void BookConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookConfig.FrameSpace):
                    RaisePropertyChanged(nameof(FrameMargin));
                    break;

                case nameof(BookConfig.IsPanorama):
                    RaisePropertyChanged(nameof(IsStaticFrame));
                    break;
            }
        }

        private void BookSettingConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        private void MainViewConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewConfig.IsFloating):
                    RaisePropertyChanged(nameof(ReferenceSize));
                    break;

                case nameof(MainViewConfig.IsAutoStretch):
                    RaisePropertyChanged(nameof(ReferenceSize));
                    break;

                case nameof(MainViewConfig.ReferenceSize):
                    RaisePropertyChanged(nameof(ReferenceSize));
                    break;
            }
        }

        public void ResetReferenceSize()
        {
            if (!_mainViewConfig.IsFloating) return;
            _mainViewConfig.ReferenceSize = _canvasSize;
            //Debug.WriteLine($"ReferenceSize: {_mainViewConfig.ReferenceSize:f0}");
        }
    }
}
