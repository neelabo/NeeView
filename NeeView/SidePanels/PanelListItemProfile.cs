using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public enum PanelListItemImageShape
    {
        [AliasName]
        Original,

        [AliasName]
        Square,

        [AliasName]
        BookShape,

        [AliasName]
        Banner,
    }

    /// <summary>
    /// リスト項目の表示形式
    /// </summary>
    [NotifyPropertyChanged]
    public partial record class PanelListItemProfile : INotifyPropertyChanged
    {
        private static Rect _rectDefault = new(0, 0, 1, 1);
        private static Rect _rectBanner = new(0, 0, 1, 0.6);
        private static readonly SolidColorBrush _brushBanner = new(Color.FromArgb(0x20, 0x99, 0x99, 0x99));

        private PanelListItemImageShape _imageShape;
        private int _imageWidth;
        private bool _isDetailPopupEnabled = true;
        private bool _isImagePopupEnabled;
        private bool _isTextVisible;
        private bool _isTextWrapped;
        private bool _isTextHeightDirty = true;
        private double _textHeight;


        public PanelListItemProfile()
        {
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public PanelListItemProfile(PanelListItemImageShape imageShape, int imageWidth, bool isDetailPopupEnalbed, bool isImagePopupEnabled, bool isTextVisible, bool isTextWrapped)
        {
            _imageShape = imageShape;
            _imageWidth = imageWidth;
            _isDetailPopupEnabled = isDetailPopupEnalbed;
            _isImagePopupEnabled = isImagePopupEnabled;
            _isTextVisible = isTextVisible;
            _isTextWrapped = isTextWrapped;

            UpdateTextHeight();
        }


        #region 公開プロパティ

        [PropertyMember]
        public PanelListItemImageShape ImageShape
        {
            get { return _imageShape; }
            set
            {
                if (_imageShape != value)
                {
                    _imageShape = value;
                    RaisePropertyChanged(null);
                }
            }
        }

        [PropertyRange(64, 512, TickFrequency = 8, IsEditable = true, Format = "{0} × {0}")]
        public int ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (SetProperty(ref _imageWidth, Math.Max(0, value)))
                {
                    RaisePropertyChanged(nameof(ShapeWidth));
                    RaisePropertyChanged(nameof(ShapeHeight));
                }
            }
        }

        [PropertyMember]
        public bool IsDetailPopupEnabled
        {
            get { return _isDetailPopupEnabled; }
            set { SetProperty(ref _isDetailPopupEnabled, value); }
        }

        [PropertyMember]
        public bool IsImagePopupEnabled
        {
            get { return _isImagePopupEnabled; }
            set { SetProperty(ref _isImagePopupEnabled, value); }
        }

        [PropertyMember]
        public bool IsTextVisible
        {
            get { return _isTextVisible; }
            set { SetProperty(ref _isTextVisible, value); }
        }

        [PropertyMember]
        public bool IsTextWrapped
        {
            get { return _isTextWrapped; }
            set
            {
                if (SetProperty(ref _isTextWrapped, value))
                {
                    UpdateTextHeight();
                }
            }
        }

        #endregion

        #region Equals

        // NOTE: TextHeight の値は遅延生成されるため、標準の比較では一致しない
        // TODO: TextHeight は別の場所で管理すべきかも？

        public virtual bool Equals(PanelListItemProfile? other)
        {
            if (other is null) return false;

            return (this.ImageShape.Equals(other.ImageShape)
                && this.ImageWidth.Equals(other.ImageWidth)
                && this.IsDetailPopupEnabled.Equals(other.IsDetailPopupEnabled)
                && this.IsImagePopupEnabled.Equals(other.IsImagePopupEnabled)
                && this.IsTextVisible.Equals(other.IsTextVisible)
                && this.IsTextWrapped.Equals(other.IsTextWrapped));
        }

        public override int GetHashCode()
        {
            HashCode hashcode = new();
            hashcode.Add(this.ImageShape.GetHashCode());
            hashcode.Add(this.ImageWidth.GetHashCode());
            hashcode.Add(this.IsDetailPopupEnabled.GetHashCode());
            hashcode.Add(this.IsImagePopupEnabled.GetHashCode());
            hashcode.Add(this.IsTextVisible.GetHashCode());
            hashcode.Add(this.IsTextWrapped.GetHashCode());

            return hashcode.ToHashCode();
        }

        #endregion Equals


        #region Obsolete

        [Obsolete("no used"), Alternative("Panel.Note in the custom theme file", 39, IsFullName = true)] // ver.39
        [JsonIgnore]
        public double NoteOpacity
        {
            get { return 0.0; }
            set { }
        }

        #endregion

        #region 非公開プロパティ

        [PropertyMapIgnore]
        public int ShapeWidth
        {
            get
            {
                return _imageShape switch
                {
                    PanelListItemImageShape.BookShape => (int)(_imageWidth * 0.7071),
                    _ => _imageWidth,
                };
            }
        }

        [PropertyMapIgnore]
        public int ShapeHeight
        {
            get
            {
                return _imageShape switch
                {
                    PanelListItemImageShape.Banner => _imageWidth / 4,
                    _ => _imageWidth,
                };
            }
        }

        [PropertyMapIgnore]
        public Rect Viewbox
        {
            get
            {
                return _imageShape switch
                {
                    PanelListItemImageShape.Banner => _rectBanner,
                    _ => _rectDefault,
                };
            }
        }

        [PropertyMapIgnore]
        public AlignmentY AlignmentY
        {
            get
            {
                return _imageShape switch
                {
                    PanelListItemImageShape.Original => AlignmentY.Bottom,
                    PanelListItemImageShape.Banner => AlignmentY.Center,
                    _ => AlignmentY.Top,
                };
            }
        }

        [PropertyMapIgnore]
        public Brush? Background
        {
            get
            {
                return _imageShape switch
                {
                    PanelListItemImageShape.Banner => _brushBanner,
                    _ => null,
                };
            }
        }

        [PropertyMapIgnore]
        public Stretch ImageStretch
        {
            get
            {
                return _imageShape switch
                {
                    PanelListItemImageShape.Original => Stretch.Uniform,
                    _ => Stretch.UniformToFill,
                };
            }
        }

        [PropertyMapIgnore]
        public double TextHeight
        {
            get
            {
                if (_isTextHeightDirty)
                {
                    _isTextHeightDirty = false;
                    _textHeight = CalcTextHeight();
                    Debug.Assert(double.IsNormal(_textHeight));
                }
                return _textHeight;
            }
        }

        [PropertyMapIgnore]
        public double LayoutedTextHeight
        {
            get
            {
                return IsTextWrapped ? TextHeight : double.NaN;
            }
        }

        #endregion

        public PanelListItemProfile CloneInstance()
        {
            var profile = ObjectExtensions.DeepCopy(this);
            profile.UpdateTextHeight();
            return profile;
        }

        // TextHeightの更新要求
        public void UpdateTextHeight()
        {
            _isTextHeightDirty = true;
            RaisePropertyChanged(nameof(TextHeight));
            RaisePropertyChanged(nameof(LayoutedTextHeight));
        }

        // calc textbox height
        private double CalcTextHeight()
        {
            // 実際にTextBlockを作成して計算する
            var textBlock = new TextBlock()
            {
                Text = IsTextWrapped ? "Age\nBusy" : "Age Busy",
                FontSize = FontParameters.Current.PaneFontSize,
            };
            if (FontParameters.Current.DefaultFontName != null)
            {
                textBlock.FontFamily = new FontFamily(FontParameters.Current.DefaultFontName);
            }
            var panel = new Canvas();
            panel.Children.Add(textBlock);
            var area = new Size(0, 0);
            panel.Measure(area);
            panel.Arrange(new Rect(area));
            double height = (int)textBlock.ActualHeight + 1.0;

            return height;
        }
    }

    public record class NormalItemProfile : PanelListItemProfile
    {
        public NormalItemProfile() : base(PanelListItemImageShape.Square, 0, true, false, true, false)
        {
        }
    }

    public record class ContentItemProfile : PanelListItemProfile
    {
        public ContentItemProfile() : base(PanelListItemImageShape.Square, 64, true, true, true, false)
        {
        }
    }

    public record class BannerItemProfile : PanelListItemProfile
    {
        public BannerItemProfile() : base(PanelListItemImageShape.Banner, 200, true, false, true, false)
        {
        }
    }

    public record class ThumbnailItemProfile : PanelListItemProfile
    {
        public ThumbnailItemProfile() : base(PanelListItemImageShape.Original, 128, true, false, true, true)
        {
        }
    }
}
