using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ImageGridConfig : BindableBase
    {
        [DefaultEquality] private bool _isEnabled;
        [DefaultEquality] private Color _color = Color.FromArgb(0x80, 0x80, 0x80, 0x80);
        [DefaultEquality] private int _divX = 8;
        [DefaultEquality] private int _divY = 8;
        [DefaultEquality] private bool _isSquare;
        [DefaultEquality] private ImageGridTarget _target;


        [PropertyMember(IsVisible = false)]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        [PropertyMember, DefaultValue(ImageGridTarget.Image)]
        public ImageGridTarget Target
        {
            get { return _target; }
            set { SetProperty(ref _target, value); }
        }

        [PropertyMember, DefaultValue(typeof(Color), "#80808080")]
        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        [PropertyRange(1, 50, TickFrequency = 1), DefaultValue(8)]
        public int DivX
        {
            get { return _divX; }
            set { SetProperty(ref _divX, value); }
        }

        [PropertyRange(1, 50, TickFrequency = 1), DefaultValue(8)]
        public int DivY
        {
            get { return _divY; }
            set { SetProperty(ref _divY, value); }
        }

        [PropertyMember, DefaultValue(false)]
        public bool IsSquare
        {
            get { return _isSquare; }
            set { SetProperty(ref _isSquare, value); }
        }
    }


    public enum ImageGridTarget
    {
        Image,
        Screen,
    }
}
