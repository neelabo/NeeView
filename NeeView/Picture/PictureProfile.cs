﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    //
    public class PictureProfile : BindableBase
    {
        static PictureProfile() => Current = new PictureProfile();
        public static PictureProfile Current { get; }

        public static readonly Uri HEIFImageExtensions = new Uri(@"ms-windows-store://pdp/?ProductId=9pmmsr1cgpwg");


        // Fields

        // 有効ファイル拡張子
        private PictureFileExtension _fileExtension = new PictureFileExtension();


        // Constructors

        private PictureProfile()
        {
            _CustomSize = new PictureCustomSize()
            {
                IsEnabled = false,
                IsUniformed = false,
                Size = new Size(256, 256)
            };
        }


        // Properties

        [PropertyMember("@ParamPictureProfileExtensions")]
        public FileTypeCollection SupportFileTypes => _fileExtension.DefaultExtensions;


        // 読み込みデータのサイズ制限適用フラグ
        [PropertyMember("@ParamPictureProfileIsLimitSourceSize", Tips = "@ParamPictureProfileIsLimitSourceSizeTips")]
        public bool IsLimitSourceSize { get; set; }

        // 画像処理の最大サイズ
        // リサイズフィルターで使用される。
        // IsLimitSourceSize フラグがONのときには、読み込みサイズにもこの制限が適用される
        private Size _MaximumSize = new Size(4096, 4096);
        [PropertyMember("@ParamPictureProfileMaximumSize", Tips = "@ParamPictureProfileMaximumSizeTips")]
        public Size MaximumSize
        {
            get { return _MaximumSize; }
            set
            {
                var size = new Size(Math.Max(value.Width, 1024), Math.Max(value.Height, 1024));
                if (_MaximumSize != size) { _MaximumSize = size; RaisePropertyChanged(); }
            }
        }

        /// <summary>
        /// IsResizeEnabled property.
        /// </summary>
        private bool _isResizeFilterEnabled = false;
        public bool IsResizeFilterEnabled
        {
            get { return _isResizeFilterEnabled; }
            set { if (_isResizeFilterEnabled != value) { _isResizeFilterEnabled = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// CustomSize property.
        /// </summary>
        private PictureCustomSize _CustomSize;
        public PictureCustomSize CustomSize
        {
            get { return _CustomSize; }
            set { if (_CustomSize != value) { _CustomSize = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsMagicScaleSimdEnabled property.
        /// </summary>
        private bool _IsMagicScaleSimdEnabled = true;
        [PropertyMember("@ParamPictureProfileIsMagicScaleSimdEnabled")]
        public bool IsMagicScaleSimdEnabled
        {
            get { return _IsMagicScaleSimdEnabled; }
            set
            {
                if (_IsMagicScaleSimdEnabled != value)
                {
                    _IsMagicScaleSimdEnabled = value;
                    MagicScalerBitmapFactory.EnabmeSimd = _IsMagicScaleSimdEnabled;
                    RaisePropertyChanged();
                }
            }
        }

        // 画像の解像度情報を表示に反映する
        private bool _IsAspectRatioEnabled;
        [PropertyMember("@ParamPictureProfileIsAspectRatioEnabled", Tips = "@ParamPictureProfileIsAspectRatioEnabledTips")]
        public bool IsAspectRatioEnabled
        {
            get { return _IsAspectRatioEnabled; }
            set { SetProperty(ref _IsAspectRatioEnabled, value); }
        }



        // Methods

        // 対応拡張子判定 (ALL)
        public bool IsSupported(string fileName)
        {
            return _fileExtension.IsSupported(fileName);
        }

        // 対応拡張子判定 (標準)
        public bool IsDefaultSupported(string fileName)
        {
            return _fileExtension.IsDefaultSupported(fileName);
        }

        // 対応拡張子判定 (Susie)
        public bool IsSusieSupported(string fileName)
        {
            return _fileExtension.IsSusieSupported(fileName);
        }

        // 最大サイズ内におさまるサイズを返す
        public Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(this.MaximumSize);
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(false)]
            public bool IsLimitSourceSize { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            public Size Maximum { get; set; }

            [DataMember]
            public bool IsResizeFilterEnabled { get; set; }

            [DataMember]
            public PictureCustomSize.Memento CustomSize { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsMagicScaleSimdEnabled { get; set; }

            [DataMember]
            public bool IsAspectRatioEnabled { get; set; }

            #region Constructors

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            private void Constructor()
            {
                IsMagicScaleSimdEnabled = true;
            }

            #endregion
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsLimitSourceSize = this.IsLimitSourceSize;
            memento.Maximum = this.MaximumSize;
            memento.IsResizeFilterEnabled = this.IsResizeFilterEnabled;
            memento.CustomSize = this.CustomSize.CreateMemento();
            memento.IsMagicScaleSimdEnabled = this.IsMagicScaleSimdEnabled;
            memento.IsAspectRatioEnabled = this.IsAspectRatioEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsLimitSourceSize = memento.IsLimitSourceSize;
            this.MaximumSize = memento.Maximum;
            this.IsResizeFilterEnabled = memento.IsResizeFilterEnabled;
            this.CustomSize.Restore(memento.CustomSize);
            this.IsMagicScaleSimdEnabled = memento.IsMagicScaleSimdEnabled;
            this.IsAspectRatioEnabled = memento.IsAspectRatioEnabled;
        }
        #endregion

    }
}
