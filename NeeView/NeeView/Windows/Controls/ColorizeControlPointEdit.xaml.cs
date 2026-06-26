using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Effects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// ColorizeControlPointEdit.xaml の相互作用ロジック
    /// </summary>
    [INotifyPropertyChanged]
    public partial class ColorizeControlPointEdit : UserControl
    {
        public ColorizeControlPointEdit()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }


        public ColorizeControlPoint ControlPoint
        {
            get { return (ColorizeControlPoint)GetValue(ControlPointProperty); }
            set { SetValue(ControlPointProperty, value); }
        }

        public static readonly DependencyProperty ControlPointProperty =
            DependencyProperty.Register(nameof(ControlPoint), typeof(ColorizeControlPoint), typeof(ColorizeControlPointEdit), new PropertyMetadata(new ColorizeControlPoint(), OnControlPointChanged));

        private static void OnControlPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorizeControlPointEdit control)
            {
                control.OnPropertyChanged("");
            }
        }

        public double Strength
        {
            get => ControlPoint.Strength;
            set
            {
                if (ControlPoint.Strength != value)
                {
                    ControlPoint = new ColorizeControlPoint(ControlPoint) { Strength = value };
                }
            }
        }

        public Color Color
        {
            get => ControlPoint.Color;
            set
            {
                if (ControlPoint.Color != value)
                {
                    ControlPoint = new ColorizeControlPoint(ControlPoint) { Color = value };
                }
            }
        }
    }
}
