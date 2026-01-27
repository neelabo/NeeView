using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenSourceControls
{
    /// <summary>
    /// ComboColorPicker user control
    /// This code is open source published with the Code Project Open License (CPOL).
    ///
    /// Originally written by Øystein Bjørke, March 2009.
    /// 
    /// The code and accompanying article can be found at http://www.codeproject.com
    /// </summary>

    public partial class ComboColorPicker : UserControl
    {
        #region Dependency properties
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ComboColorPicker),
            new FrameworkPropertyMetadata(OnSelectedColorChanged));

        private static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ComboColorPicker cp = obj as ComboColorPicker ?? throw new InvalidOperationException();

            Color newColor = (Color)args.NewValue;
            Color oldColor = (Color)args.OldValue;

            if (newColor == oldColor)
                return;

            // When the SelectedColor changes, set the selected value of the combo box
            if (cp.ColorList1.SelectedValue is not ColorViewModel selectedColorViewModel || selectedColorViewModel.Color != newColor)
            {
                // Add the color if not found
                if (!cp.ListContains(newColor))
                {
                    cp.AddColor(newColor, newColor.ToString(CultureInfo.InvariantCulture));
                }
            }

            // Also update the brush
            cp.SelectedBrush = new SolidColorBrush(newColor);
            cp.OnColorChanged(oldColor, newColor);
        }

        private bool ListContains(Color newColor)
        {
            foreach (object o in ColorList1.Items)
            {
                if (o is not ColorViewModel vcm) continue;
                if (vcm.Color == newColor) return true;
            }
            return false;
        }

        public Brush SelectedBrush
        {
            get { return (Brush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(ComboColorPicker),
            new FrameworkPropertyMetadata(OnSelectedBrushChanged));

        private static void OnSelectedBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Debug.WriteLine("OnSelectedBrushChanged");
            ComboColorPicker cp = (ComboColorPicker)obj;
            SolidColorBrush newBrush = (SolidColorBrush)args.NewValue;
            // SolidColorBrush oldBrush = (SolidColorBrush)args.OldValue;

            if (cp.SelectedColor != newBrush.Color)
                cp.SelectedColor = newBrush.Color;
        }
        #endregion

        #region Events
        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ComboColorPicker));

        public event RoutedPropertyChangedEventHandler<Color> ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        protected virtual void OnColorChanged(Color oldValue, Color newValue)
        {
            var args = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue);
            args.RoutedEvent = ComboColorPicker.ColorChangedEvent;
            RaiseEvent(args);
        }
        #endregion

        static readonly Brush _CheckerBrush = CreateCheckerBrush();
        public static Brush CheckerBrush { get { return _CheckerBrush; } }
        // Todo: should this be disposed somewhere?

        public ComboColorPicker()
        {
            InitializeComponent();

            InitializeColors();
        }

        public void InitializeColors()
        {
            ColorList1.Items.Clear();

            // Add some common colors
            AddColor(Colors.Black, "Black");
            AddColor(Colors.Gray, "Gray");
            AddColor(Colors.LightGray, "LightGray");
            AddColor(Colors.White, "White");
            AddColor(Colors.Transparent, "Transparent");
            AddColor(Colors.Red, "Red");
            AddColor(Colors.Green, "Green");
            AddColor(Colors.Blue, "Blue");
            AddColor(Colors.Cyan, "Cyan");
            AddColor(Colors.Magenta, "Magenta");
            AddColor(Colors.Yellow, "Yellow");
            AddColor(Colors.Purple, "Purple");
            AddColor(Colors.Orange, "Orange");
            AddColor(Colors.Brown, "Brown");

            // And some colors with transparency
            AddColor(Color.FromArgb(128, 0, 0, 0), "Black 50%");
            AddColor(Color.FromArgb(128, 128, 128, 128), "Gray 50%");
            AddColor(Color.FromArgb(128, 255, 255, 255), "White 50%");
            AddColor(Color.FromArgb(128, 255, 0, 0), "Red 50%");
            AddColor(Color.FromArgb(128, 0, 255, 0), "Green 50%");
            AddColor(Color.FromArgb(128, 0, 0, 255), "Blue 50%");
            ColorList1.Items.Add(new SeparatorWithColorProperty());

            // Enumerate constant colors from the Colors class
            Type colorsType = typeof(Colors);
            PropertyInfo[] pis = colorsType.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                var color = pi.GetValue(null, null);
                if (color is null) throw new InvalidOperationException();
                AddColor((Color)color, pi.Name);
            }

            // todo: does this work?
            ColorList1.SelectedValuePath = "Color";
        }


        private void AddColor(Color color, string name)
        {
            if (!name.StartsWith("#", StringComparison.Ordinal))
                name = NiceName(name);
            var cvm = new ColorViewModel() { Color = color, Name = name };
            ColorList1.Items.Add(cvm);
        }

        private static string NiceName(string name)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                    sb.Append(' ');
                sb.Append(name[i]);
            }
            return sb.ToString();
        }

        public static Brush CreateCheckerBrush()
        {
            // from http://msdn.microsoft.com/en-us/library/aa970904.aspx

            var checkerBrush = new DrawingBrush();

            var backgroundSquare =
                new GeometryDrawing(
                    Brushes.White,
                    null,
                    new RectangleGeometry(new Rect(0, 0, 8, 8)));

            var aGeometryGroup = new GeometryGroup();
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 4, 4)));
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(4, 4, 4, 4)));

            var checkers = new GeometryDrawing(Brushes.Black, null, aGeometryGroup);

            var checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            checkerBrush.Drawing = checkersDrawingGroup;
            checkerBrush.Viewport = new Rect(0, 0, 0.5, 0.5);
            checkerBrush.TileMode = TileMode.Tile;

            return checkerBrush;
        }

    }

    public class ColorViewModel
    {
        public Color Color { get; set; }
        public Brush Brush { get { return new SolidColorBrush(Color); } }
        public string? Name { get; set; }
    }


    /// <summary>
    /// Bind 警告回避するためだけの専用 Separator
    /// </summary>
    /// <remarks>
    /// Color バインド警告が回避できないのでダミーのプロパティを実装することで対処
    /// </remarks>
    public class SeparatorWithColorProperty : Separator
    {
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(SeparatorWithColorProperty), new PropertyMetadata(Colors.Transparent));
    }
}
