using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// ブラシの種類
    /// </summary>
    public enum BrushType
    {
        [AliasName]
        SolidColor,

        [AliasName]
        ImageTile,

        [AliasName]
        ImageFill,

        [AliasName]
        ImageUniform,

        [AliasName]
        ImageUniformToFill,
    }

    /// <summary>
    /// ブラシ構成要素.
    /// 単色ブラシ、画像タイルブラシ対応
    /// </summary>
    public class BrushSource : BindableBase, ICloneable
    {
        public static Dictionary<BrushType, string> BrushTypeList => AliasNameExtensions.GetAliasNameDictionary<BrushType>();

        private BrushType _type;
        private Color _color;
        private string? _imageFileName;


        public BrushSource()
        {
            Reset();
        }


        [PropertyMember]
        public BrushType Type
        {
            get { return _type; }
            set { if (_type != value) { _type = value; RaisePropertyChanged(); } }
        }

        [PropertyMember]
        public Color Color
        {
            get { return _color; }
            set { if (_color != value) { _color = value; RaisePropertyChanged(); } }
        }

        [PropertyMember]
        public string? ImageFileName
        {
            get { return _imageFileName; }
            set { if (_imageFileName != value) { _imageFileName = value; RaisePropertyChanged(); } }
        }


        public void Reset()
        {
            _type = BrushType.SolidColor;
            _color = Colors.LightGray;
            _imageFileName = null;
            RaisePropertyChanged(null);
        }

        public Brush CreateBackBrush()
        {
            return new SolidColorBrush(Color);
        }

        public Brush? CreateFrontBrush()
        {
            return Type switch
            {
                BrushType.ImageTile or BrushType.ImageFill or BrushType.ImageUniform or BrushType.ImageUniformToFill => CreateImageBrush(Type),
                _ => null,
            };
        }

        private Brush CreateImageBrush(BrushType type)
        {
            if (string.IsNullOrEmpty(this.ImageFileName))
            {
                return Brushes.Transparent;
            }

            try
            {
                var bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.UriSource = new Uri(ImageFileName);
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.EndInit();
                bmpImage.Freeze();

                var brush = new ImageBrush(bmpImage);
                switch (type)
                {
                    case BrushType.ImageTile:
                        brush.AlignmentX = AlignmentX.Left;
                        brush.AlignmentY = AlignmentY.Top;
                        brush.Viewport = new Rect(0, 0, bmpImage.PixelWidth, bmpImage.PixelHeight);
                        brush.ViewportUnits = BrushMappingMode.Absolute;
                        brush.Stretch = Stretch.Fill;
                        brush.TileMode = TileMode.Tile;
                        break;
                    case BrushType.ImageFill:
                        brush.Stretch = Stretch.Fill;
                        break;
                    case BrushType.ImageUniform:
                        brush.Stretch = Stretch.Uniform;
                        break;
                    case BrushType.ImageUniformToFill:
                        brush.Stretch = Stretch.UniformToFill;
                        break;
                }

                return brush;
            }
            catch
            {
                return Brushes.LightGray;
            }
        }

        public object Clone()
        {
            var clone = (BrushSource)MemberwiseClone();
            clone.ResetPropertyChanged();
            return clone;
        }
    }
}
