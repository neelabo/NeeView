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
    public partial class PointInspector : Control, INotifyPropertyChanged
    {
        static PointInspector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointInspector), new FrameworkPropertyMetadata(typeof(PointInspector)));
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var root = this.GetTemplateChild("PART_Root") as Grid ?? throw new InvalidOperationException();
            root.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public Point Point
        {
            get { return (Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }

        public static readonly DependencyProperty PointProperty =
            DependencyProperty.Register("Point", typeof(Point), typeof(PointInspector), new PropertyMetadata(new Point()));


        public double X
        {
            get { return Point.X; }
            set { if (Point.X != value) { Point = new Point(value, Point.Y); RaisePropertyChanged(); } }
        }

        public double Y
        {
            get { return Point.Y; }
            set { if (Point.Y != value) { Point = new Point(Point.X, value); RaisePropertyChanged(); } }
        }
    }
}
