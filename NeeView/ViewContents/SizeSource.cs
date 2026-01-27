using NeeLaboratory.Generators;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class SizeSource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private double _width;
        private double _height;

        public SizeSource()
        {
        }

        public SizeSource(Size size) : this(size.Width, size.Height)
        {
        }

        public SizeSource(double width, double height)
        {
            Width = width;
            Height = height;
        }


        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }


        public void SetSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        public void BindTo(FrameworkElement element)
        {
            element.SetBinding(FrameworkElement.WidthProperty, new Binding(nameof(Width)) { Source = this });
            element.SetBinding(FrameworkElement.HeightProperty, new Binding(nameof(Height)) { Source = this });
        }
    }
}
