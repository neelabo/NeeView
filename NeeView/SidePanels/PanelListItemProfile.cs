using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
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
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class PanelListItemProfile : ObservableObject
    {
        private static Rect _rectDefault = new(0, 0, 1, 1);
        private static Rect _rectBanner = new(0, 0, 1, 0.6);
        private static readonly SolidColorBrush _brushBanner = new(Color.FromArgb(0x20, 0x99, 0x99, 0x99));

        [DefaultEquality] private PanelListItemImageShape _imageShape;
        [DefaultEquality] private int _imageWidth;
        [DefaultEquality] private bool _isDetailPopupEnabled = true;
        [DefaultEquality] private bool _isImagePopupEnabled;
        [DefaultEquality] private bool _isTextVisible;
        [DefaultEquality] private bool _isTextWrapped;
        [DefaultEquality] private bool _isTagVisible;
        private bool _isTextHeightDirty = true;
        private double _textHeight;


        public PanelListItemProfile()
        {
        }

        public PanelListItemProfile(PanelListItemImageShape imageShape, int imageWidth, bool isDetailPopupEnabled, bool isImagePopupEnabled, bool isTextVisible, bool isTextWrapped, bool isTagVisible)
        {
            _imageShape = imageShape;
            _imageWidth = imageWidth;
            _isDetailPopupEnabled = isDetailPopupEnabled;
            _isImagePopupEnabled = isImagePopupEnabled;
            _isTextVisible = isTextVisible;
            _isTextWrapped = isTextWrapped;
            _isTagVisible = isTagVisible;

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
                    OnPropertyChanged("");
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
                    OnPropertyChanged(nameof(ShapeWidth));
                    OnPropertyChanged(nameof(ShapeHeight));
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

        [PropertyMember]
        public bool IsTagVisible
        {
            get { return _isTagVisible; }
            set { SetProperty(ref _isTagVisible, value); }
        }

        #endregion

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
            OnPropertyChanged(nameof(TextHeight));
            OnPropertyChanged(nameof(LayoutedTextHeight));
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

    public class NormalItemProfile : PanelListItemProfile
    {
        public NormalItemProfile() : base(PanelListItemImageShape.Square, 0, true, false, true, false, true)
        {
        }
    }

    public class ContentItemProfile : PanelListItemProfile
    {
        public ContentItemProfile() : base(PanelListItemImageShape.Square, 64, true, true, true, false, true)
        {
        }
    }

    public class BannerItemProfile : PanelListItemProfile
    {
        public BannerItemProfile() : base(PanelListItemImageShape.Banner, 200, true, false, true, false, true)
        {
        }
    }

    public class ThumbnailItemProfile : PanelListItemProfile
    {
        public ThumbnailItemProfile() : base(PanelListItemImageShape.Original, 128, true, false, true, true, true)
        {
        }
    }
}
