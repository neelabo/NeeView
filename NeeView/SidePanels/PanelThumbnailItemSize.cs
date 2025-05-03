//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ListBox のサムネイル表示での ListBoxItem サイズ
    /// </summary>
    /// 
    /// サムネイル項目は以下のような構成を想定しています
    /// +--------+
    /// | Image  |
    /// +--------+
    /// | Select |
    /// +--------+
    /// | Text   |
    /// +--------+
    [LocalDebug]
    public partial class PanelThumbnailItemSize : BindableBase, IDisposable
    {
        private readonly PanelListItemProfile _profile;
        private double _margin;
        private double _selectHeight;
        private Size _iconSize;
        private Size _itemSize;
        private bool _disposedValue;


        public PanelThumbnailItemSize(PanelListItemProfile profile, double margin, double selectHeight, Size iconSize)
        {
            _margin = margin;
            _selectHeight = selectHeight;
            _iconSize = iconSize;

            _profile = profile;
            _profile.PropertyChanged += Profile_PropertyChanged;

            Update();
        }


        /// <summary>
        /// ルートコントロールの Margin
        /// </summary>
        public double Margin
        {
            get { return _margin; }
            set
            {
                if (SetProperty(ref _margin, value))
                {
                    Update();
                }
            }
        }

        /// <summary>
        /// サムネイル画像直下の選択マークの高さ
        /// </summary>
        public double SelectHeight
        {
            get { return _selectHeight; }
            set
            {
                if (SetProperty(ref _selectHeight, value))
                {
                    Update();
                }
            }
        }

        /// <summary>
        /// フォルダー等のアイコンサイズ。これが最小サイズとなる
        /// </summary>
        public Size IconSize
        {
            get { return _iconSize; }
            set
            {
                if (SetProperty(ref _iconSize, value))
                {
                    Update();
                }
            }
        }

        /// <summary>
        /// 計算された ItemSize
        /// </summary>
        public Size ItemSize
        {
            get { return _itemSize; }
            private set { SetProperty(ref _itemSize, value); }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _profile.PropertyChanged -= Profile_PropertyChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Profile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "":
                case nameof(_profile.ShapeWidth):
                case nameof(_profile.ShapeHeight):
                case nameof(_profile.IsTextVisible):
                case nameof(_profile.TextHeight):
                    Update();
                    break;
            }
        }

        private void Update()
        {
            ItemSize = GetItemSize();
        }

        private Size GetItemSize()
        {
            var width = Math.Max(_profile.ShapeWidth, IconSize.Width) + Margin * 2.0;
            var height = Math.Max(_profile.ShapeHeight, IconSize.Height) + SelectHeight + (_profile.IsTextVisible ? Math.Max(_profile.TextHeight, IconSize.Height) : 0.0) + Margin * 2.0;
            var size = new Size(width, height);
            LocalDebug.WriteLine($"Thumbnail: {size:F2}");
            Debug.Assert(double.IsNormal(size.Width));
            Debug.Assert(double.IsNormal(size.Height));
            return size;
        }

    }
}
