using Jint.Native;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Windows.Controls
{
    [NotifyPropertyChanged]
    public partial class SizeInspector : Control, INotifyPropertyChanged
    {
        static SizeInspector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SizeInspector), new FrameworkPropertyMetadata(typeof(SizeInspector)));
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var root = this.GetTemplateChild("PART_Root") as Grid ?? throw new InvalidOperationException();
            root.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public Size Size
        {
            get { return (Size)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Size), typeof(SizeInspector), new FrameworkPropertyMetadata(new Size(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SizePropertyChanged));

        private static void SizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SizeInspector)?.RaisePropertyChanged(null);
        }


        public double X
        {
            get { return Size.Width; }
            set { if (Size.Width != value) { Size = new Size(value, Size.Height); RaisePropertyChanged(); } }
        }

        public double Y
        {
            get { return Size.Height; }
            set { if (Size.Height != value) { Size = new Size(Size.Width, value); RaisePropertyChanged(); } }
        }
    }
}
