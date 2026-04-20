using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class PanelGrid : Control
    {
        static PanelGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PanelGrid), new FrameworkPropertyMetadata(typeof(PanelGrid)));
        }

        private const double _defaultTreeAreaWidth = 128.0;
        private const double _defaultTreeAreaHeight = 72.0;
        private double _areaWidth = double.PositiveInfinity;
        private double _areaHeight = double.PositiveInfinity;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.SizeChanged += Grid_SizeChanged;
        }


        public FrameworkElement MainContent
        {
            get { return (FrameworkElement)GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(nameof(MainContent), typeof(FrameworkElement), typeof(PanelGrid), new PropertyMetadata(null));


        public FrameworkElement SubContent
        {
            get { return (FrameworkElement)GetValue(SubContentProperty); }
            set { SetValue(SubContentProperty, value); }
        }

        public static readonly DependencyProperty SubContentProperty =
            DependencyProperty.Register(nameof(SubContent), typeof(FrameworkElement), typeof(PanelGrid), new PropertyMetadata(null));



        public bool IsFolderTreeVisible
        {
            get { return (bool)GetValue(IsFolderTreeVisibleProperty); }
            set { SetValue(IsFolderTreeVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsFolderTreeVisibleProperty =
            DependencyProperty.Register(nameof(IsFolderTreeVisible), typeof(bool), typeof(PanelGrid), new PropertyMetadata(false, ParameterChanged));


        public FolderTreeLayout FolderTreeLayout
        {
            get { return (FolderTreeLayout)GetValue(FolderTreeLayoutProperty); }
            set { SetValue(FolderTreeLayoutProperty, value); }
        }

        public static readonly DependencyProperty FolderTreeLayoutProperty =
            DependencyProperty.Register(nameof(FolderTreeLayout), typeof(FolderTreeLayout), typeof(PanelGrid), new PropertyMetadata(FolderTreeLayout.Left, ParameterChanged));


        public double FolderTreeAreaWidth
        {
            get { return (double)GetValue(FolderTreeAreaWidthProperty); }
            set { SetValue(FolderTreeAreaWidthProperty, value); }
        }

        public static readonly DependencyProperty FolderTreeAreaWidthProperty =
            DependencyProperty.Register(nameof(FolderTreeAreaWidth), typeof(double), typeof(PanelGrid), new FrameworkPropertyMetadata(_defaultTreeAreaWidth, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ParameterChanged));


        public double FolderTreeAreaHeight
        {
            get { return (double)GetValue(FolderTreeAreaHeightProperty); }
            set { SetValue(FolderTreeAreaHeightProperty, value); }
        }

        public static readonly DependencyProperty FolderTreeAreaHeightProperty =
            DependencyProperty.Register(nameof(FolderTreeAreaHeight), typeof(double), typeof(PanelGrid), new FrameworkPropertyMetadata(_defaultTreeAreaHeight, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ParameterChanged));


        public double SubContentWidth
        {
            get { return (double)GetValue(SubContentWidthProperty); }
            set { SetValue(SubContentWidthProperty, value); }
        }

        public static readonly DependencyProperty SubContentWidthProperty =
            DependencyProperty.Register(nameof(SubContentWidth), typeof(double), typeof(PanelGrid), new FrameworkPropertyMetadata(_defaultTreeAreaWidth, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SubContentWidth_Changed));

        private static void SubContentWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PanelGrid control)
            {
                control.FlushWidth();
            }
        }


        public double SubContentHeight
        {
            get { return (double)GetValue(SubContentHeightProperty); }
            set { SetValue(SubContentHeightProperty, value); }
        }

        public static readonly DependencyProperty SubContentHeightProperty =
            DependencyProperty.Register(nameof(SubContentHeight), typeof(double), typeof(PanelGrid), new FrameworkPropertyMetadata(_defaultTreeAreaHeight, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SubContentHeight_Changed));

        private static void SubContentHeight_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PanelGrid control)
            {
                control.FlushHeight();
            }
        }


        private static void ParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PanelGrid control)
            {
                control.UpdateGridLayout();
            }
        }


        private bool IsLeftPaneVisible() => this.IsFolderTreeVisible && this.FolderTreeLayout == FolderTreeLayout.Left;

        private bool IsTopPaneVisible() => this.IsFolderTreeVisible && this.FolderTreeLayout == FolderTreeLayout.Top;

        private void FlushWidth()
        {
            if (IsLeftPaneVisible())
            {
                FolderTreeAreaWidth = SubContentWidth;
            }
        }

        private void FlushHeight()
        {
            if (this.IsTopPaneVisible())
            {
                FolderTreeAreaHeight = SubContentHeight;
            }
        }

        private void UpdateGridLayout()
        {
            if (IsLeftPaneVisible())
            {
                SubContentWidth = Math.Max(Math.Min(FolderTreeAreaWidth, _areaWidth - 32.0), 32.0 - 6.0);
            }
            else
            {
                SubContentWidth = 0.0;
            }

            if (IsTopPaneVisible())
            {
                SubContentHeight = Math.Max(Math.Min(FolderTreeAreaHeight, _areaHeight - 32.0), 32.0 - 6.0);
            }
            else
            {
                SubContentHeight = 0.0;
            }
        }

        private void Grid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                _areaWidth = e.NewSize.Width;
                UpdateGridLayout();
            }
            if (e.HeightChanged)
            {
                _areaHeight = e.NewSize.Height;
                UpdateGridLayout();
            }
        }

    }
}
