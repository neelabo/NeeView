﻿using NeeLaboratory.Generators;
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
    /// <summary>
    /// PointInspector.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PointInspector : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        public Point Point
        {
            get { return (Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Point.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointProperty =
            DependencyProperty.Register("Point", typeof(Point), typeof(PointInspector), new PropertyMetadata(new Point()));

        /// <summary>
        /// Property: X
        /// </summary>
        public double X
        {
            get { return Point.X; }
            set { if (Point.X != value) { Point = new Point(value, Point.Y); RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Property: Y
        /// </summary>
        public double Y
        {
            get { return Point.Y; }
            set { if (Point.Y != value) { Point = new Point(Point.X, value); RaisePropertyChanged(); } }
        }


        public PointInspector()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }
    }
}
