using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class ArcSegmentShape : Canvas
    {
        private Path _path = new();
        private PathFigure _pathFigure = new();
        private ArcSegment _arcSegment = new();
        private double _startAngle = 0.0;
        private double _angle = 180.0;
        private double _centerX;
        private double _centerY;
        private double _radius = 16.0;
        private Point _startPoint;
        private Point _endPoint;


        public ArcSegmentShape()
        {
            _pathFigure.Segments.Add(_arcSegment);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(_pathFigure);

            _path.Stroke = Brushes.SteelBlue;
            _path.StrokeThickness = 4;
            _path.Data = pathGeometry;

            Update();

            this.Children.Add(_path);
        }


        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(ArcSegmentShape), new PropertyMetadata(Brushes.Black, Stroke_PropertyChanged));

        private static void Stroke_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArcSegmentShape control)
            {
                control._path.Stroke = (Brush)e.NewValue;
            }
        }


        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(ArcSegmentShape), new PropertyMetadata(1.0, StrokeThickness_PropertyChanged));

        private static void StrokeThickness_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArcSegmentShape control)
            {
                control._path.StrokeThickness = (double)e.NewValue;
                control.Update();
            }
        }


        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(ArcSegmentShape), new PropertyMetadata(90.0, Angle_PropertyChanged));

        private static void Angle_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArcSegmentShape control)
            {
                control.UpdateAngle();
            }
        }


        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(ArcSegmentShape), new PropertyMetadata(16.0, Radius_PropertyChanged));

        private static void Radius_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArcSegmentShape control)
            {
                control.Update();
            }
        }


        private void Update()
        {
            _radius = Radius;
            _startPoint = GetPointOnCircle(_radius, _startAngle);
            _pathFigure.StartPoint = _startPoint;
            _arcSegment.Size = new Size(_radius, _radius);
            _arcSegment.SweepDirection = SweepDirection.Clockwise;
            UpdateAngle();
        }

        private void UpdateAngle()
        {
            _angle = Angle;
            _endPoint = GetPointOnCircle(_radius, _startAngle + _angle);
            _arcSegment.Point = _endPoint;
            _arcSegment.IsLargeArc = _angle > 180.0; ;
        }

        private Point GetPointOnCircle(double radius, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double x = _centerX + radius * Math.Cos(angleInRadians);
            double y = _centerY + radius * Math.Sin(angleInRadians);
            return new Point(x, y);
        }
    }

}
