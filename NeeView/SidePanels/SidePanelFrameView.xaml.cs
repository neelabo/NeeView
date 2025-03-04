using NeeLaboratory.Generators;
using NeeView.Windows;
using NeeView.Windows.Media;
using System;
using System.ComponentModel;
using System.Diagnostics;
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

namespace NeeView
{
    /// <summary>
    /// SidePanelFrameView.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class SidePanelFrameView : UserControl, INotifyPropertyChanged
    {
        private const double _splitterWidth = 8.0;
        private const double _panelDefaultWidth = 300.0;
        private const double _panelMinWidth = 100.0;


        #region DependencyProperties

        public Thickness PanelMargin
        {
            get { return (Thickness)GetValue(PanelMarginProperty); }
            set { SetValue(PanelMarginProperty, value); }
        }

        public static readonly DependencyProperty PanelMarginProperty =
            DependencyProperty.Register("PanelMargin", typeof(Thickness), typeof(SidePanelFrameView), new PropertyMetadata(null));


        /// <summary>
        /// IsAutoHide property.
        /// </summary>
        public bool IsAutoHide
        {
            get { return (bool)GetValue(IsAutoHideProperty); }
            set { SetValue(IsAutoHideProperty, value); }
        }

        public static readonly DependencyProperty IsAutoHideProperty =
            DependencyProperty.Register("IsAutoHide", typeof(bool), typeof(SidePanelFrameView), new PropertyMetadata(false, IsAutoHide_Changed));

        private static void IsAutoHide_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrameView control)
            {
                control.UpdateAutoHide();
            }
        }

        /// <summary>
        /// SidePanelFrameModel を Sourceとして指定する。
        /// 指定することで初めてViewModelが生成される
        /// </summary>
        public SidePanelFrame Source
        {
            get { return (SidePanelFrame)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(SidePanelFrame), typeof(SidePanelFrameView), new PropertyMetadata(null, SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrameView control)
            {
                control.InitializeViewModel(control.Source);
            }
        }


        /// <summary>
        /// CanvasWidth property.
        /// </summary>
        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }

        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }

        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register("CanvasHeight", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        /// <summary>
        /// CanvasLeft property.
        /// </summary>
        public double CanvasLeft
        {
            get { return (double)GetValue(CanvasLeftProperty); }
            set { SetValue(CanvasLeftProperty, value); }
        }

        public static readonly DependencyProperty CanvasLeftProperty =
            DependencyProperty.Register("CanvasLeft", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        /// <summary>
        /// CanvasTop property.
        /// </summary>
        public double CanvasTop
        {
            get { return (double)GetValue(CanvasTopProperty); }
            set { SetValue(CanvasTopProperty, value); }
        }

        public static readonly DependencyProperty CanvasTopProperty =
            DependencyProperty.Register("CanvasTop", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        /// <summary>
        /// Screen LeftColumn Width
        /// </summary>
        public GridLength LeftColumnWidth
        {
            get { return (GridLength)GetValue(LeftColumnWidthProperty); }
            set { SetValue(LeftColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty LeftColumnWidthProperty =
            DependencyProperty.Register("LeftColumnWidth", typeof(GridLength), typeof(SidePanelFrameView), new PropertyMetadata(new GridLength(0.0)));


        /// <summary>
        /// Screen RightColumn Width
        /// </summary>
        public GridLength RightColumnWidth
        {
            get { return (GridLength)GetValue(RightColumnWidthProperty); }
            set { SetValue(RightColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty RightColumnWidthProperty =
            DependencyProperty.Register("RightColumnWidth", typeof(GridLength), typeof(SidePanelFrameView), new PropertyMetadata(new GridLength(0.0)));

        #endregion DependencyProperties


        // パネル幅自動調整用
        enum AdjustPanelWidthOrder
        {
            None,
            KeepLeft,
            KeepRight,
        };

        // パネル幅自動調整用
        private AdjustPanelWidthOrder _adjustPanelWidthOrder;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public SidePanelFrameView()
        {
            InitializeComponent();

            InitializeViewModel(this.Source);

            this.Root.DataContext = this;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        /// <summary>
        /// サイドバーの幅
        /// </summary>
        public double PanelIconGridWidth => 50.0;

        /// <summary>
        /// スプリッターの幅
        /// </summary>
        public double SplitterWidth => _splitterWidth;


        private SidePanelFrameViewModel? _vm;
        public SidePanelFrameViewModel? VM
        {
            get { return _vm; }
            private set { if (_vm != value) { _vm = value; RaisePropertyChanged(); } }
        }

        private Thickness _viewportMargin;
        public Thickness ViewportMargin
        {
            get { return _viewportMargin; }
            set { SetProperty(ref _viewportMargin, value); }
        }


        private void InitializeViewModel(SidePanelFrame model)
        {
            if (model == null) return;

            CustomLayoutPanelManager.Initialize();
            var leftPanelViewModel = new LeftPanelViewModel(this.LeftIconList, CustomLayoutPanelManager.Current.LeftDock, LeftPanelElementContains);
            leftPanelViewModel.AddPropertyChanged(nameof(leftPanelViewModel.SelectedItem), (s, e) => model.RaisePanelPropertyChanged());
            var rightPanelViewModel = new RightPanelViewModel(this.RightIconList, CustomLayoutPanelManager.Current.RightDock, RightPanelElementContains);
            rightPanelViewModel.AddPropertyChanged(nameof(rightPanelViewModel.SelectedItem), (s, e) => model.RaisePanelPropertyChanged());
            this.VM = new SidePanelFrameViewModel(model, leftPanelViewModel, rightPanelViewModel);
            this.VM.PanelVisibilityChanged += (s, e) => UpdateCanvas();

            InitializeColumnWidth(this.VM);

            UpdateAutoHide();
        }


        /// <summary>
        /// 左パネルに含まれる要素判定
        /// </summary>
        private bool LeftPanelElementContains(DependencyObject element)
        {
            return VisualTreeUtility.HasParentElement(element, this.LeftIconGrid) || VisualTreeUtility.HasParentElement(element, this.LeftPanel);
        }

        /// <summary>
        /// 右パネルに含まれる要素判定
        /// </summary>
        private bool RightPanelElementContains(DependencyObject element)
        {
            return VisualTreeUtility.HasParentElement(element, this.RightIconGrid) || VisualTreeUtility.HasParentElement(element, this.RightPanel);
        }

        /// <summary>
        /// AutoHide 更新
        /// </summary>
        private void UpdateAutoHide()
        {
            if (_vm == null) return;
            _vm.IsAutoHide = IsAutoHide;
        }

        /// <summary>
        /// コンテンツ表示領域サイズ更新
        /// </summary>
        private void UpdateCanvas()
        {
            if (_vm == null || _vm.IsAutoHide)
            {
                CanvasLeft = 0;
                CanvasTop = 0;
                CanvasWidth = this.Root.ActualWidth;
                CanvasHeight = this.Root.ActualHeight;
            }
            else
            {
                var panel0 = this.LeftPanel.IsVisible ? this.CenterPanel : this.ScreenRect;
                var panel1 = this.RightPanel.IsVisible ? this.CenterPanel : this.ScreenRect;
                var point0 = panel0.TranslatePoint(new Point(0, 0), this.Root);
                var point1 = panel1.TranslatePoint(new Point(panel1.ActualWidth, panel1.ActualHeight), this.Root);
                var rect = new Rect(point0, point1);

                CanvasLeft = rect.Left;
                CanvasTop = rect.Top;
                CanvasWidth = rect.Width;
                CanvasHeight = rect.Height;
            }
        }

        private void LeftIconGrid_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            ((UIElement?)sender)?.Focus();
        }

        private void LeftIconGrid_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            _vm.Left.Toggle();
        }

        private void RightIconGrid_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            ((UIElement?)sender)?.Focus();
        }

        private void RightIconGrid_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            _vm.Right.Toggle();
        }

        private void PanelIconItemsControl_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void DragStartBehavior_DragBegin(object? sender, DragStartEventArgs e)
        {
            if (_vm is null) return;

            _vm.DragBegin(sender, e);
        }

        private void DragStartBehavior_DragEnd(object? sender, EventArgs e)
        {
            if (_vm is null) return;

            _vm.DragEnd(sender, e);
        }


        public bool IsPanelMouseOver()
        {
            return IsLeftPaneMouseOver() || IsRightPanelMouseOver();
        }

        private bool IsLeftPaneMouseOver()
        {
            if (!this.LeftPanelContent.IsVisible) return false;

            var pos = Mouse.GetPosition(this.LeftPanelContent);
            return this.LeftPanelContent.IsMouseOver || pos.X <= 0.0;
        }

        private bool IsRightPanelMouseOver()
        {
            if (!this.RightPanelContent.IsVisible) return false;

            var pos = Mouse.GetPosition(this.RightPanelContent);
            return this.RightPanelContent.IsMouseOver || pos.X >= 0.0;
        }


        #region ColumnWidth

        public void InitializeColumnWidth(SidePanelFrameViewModel vm)
        {
            this.SetBinding(LeftColumnWidthProperty, new Binding(nameof(vm.LeftPanelWidth)) { Source = vm, Mode = BindingMode.TwoWay });
            this.SetBinding(RightColumnWidthProperty, new Binding(nameof(vm.RightPanelWidth)) { Source = vm, Mode = BindingMode.TwoWay });

            this.ScreenRect.SizeChanged += ScreenRect_SizeChanged;
            this.Screen.SizeChanged += Screen_SizeChanged;
            this.CenterPanel.SizeChanged += CenterPanel_SizeChanged;
            this.LeftPanel.IsVisibleChanged += LeftPanel_IsVisibleChanged;
            this.RightPanel.IsVisibleChanged += RightPanel_IsVisibleChanged;

            vm.AddPropertyChanged(nameof(vm.IsLeftPanelActive), ViewModel_IsLeftPanelActiveChanged);
            vm.AddPropertyChanged(nameof(vm.IsRightPanelActive), ViewModel_IsRightPanelActiveChanged);

            UpdateCanvas();
        }

        private void ViewModel_IsLeftPanelActiveChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm is null) return;

            if (_vm.IsLeftPanelActive && this.LeftColumnWidth.Value < _panelMinWidth)
            {
                var length = _panelMinWidth;
                _adjustPanelWidthOrder = AdjustPanelWidthOrder.KeepLeft;
                this.LeftColumnWidth = new GridLength(length);
            }

            UpdateCanvas();
        }

        private void ViewModel_IsRightPanelActiveChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm is null) return;

            // NOTE: 右パネル表示の場合のみ左パネルの幅を調整する。左パネル表示では不要。
            if (_vm.IsRightPanelActive && this.ScreenRect.ActualWidth < this.Screen.ActualWidth)
            {
                AdjustLeftPanelWidth();
            }

            if (_vm.IsRightPanelActive && this.RightColumnWidth.Value < _panelMinWidth)
            {
                var length = _panelMinWidth;
                _adjustPanelWidthOrder = AdjustPanelWidthOrder.KeepRight;
                this.RightColumnWidth = new GridLength(length);
            }

            UpdateCanvas();
        }

        private void LeftPanel_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateCanvas();
        }

        private void RightPanel_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateCanvas();
        }

        private void ScreenRect_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (_vm is null) return;

            if (_vm.IsLimitPanelWidth)
            {
                AdjustPanelWidthFromOrder(true);
            }

            UpdateCanvas();
        }

        private void Screen_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (this.RightSplitter.IsDragging)
            {
                AdjustRightPanelWidth();
                return;
            }

            if (this.LeftSplitter.IsDragging)
            {
                AdjustLeftPanelWidth();
                return;
            }

            AdjustPanelWidthFromOrder(false);
        }

        private void AdjustPanelWidthFromOrder(bool isForce)
        {
            if (isForce && _adjustPanelWidthOrder == AdjustPanelWidthOrder.None)
            {
                _adjustPanelWidthOrder = AdjustPanelWidthOrder.KeepLeft;
            }

            switch (_adjustPanelWidthOrder)
            {
                case AdjustPanelWidthOrder.KeepLeft:
                    AdjustLeftPanelWidth();
                    break;
                case AdjustPanelWidthOrder.KeepRight:
                    AdjustRightPanelWidth();
                    break;
            }

            _adjustPanelWidthOrder = AdjustPanelWidthOrder.None;
        }

        private void CenterPanel_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateCanvas();
        }

        private void AdjustLeftPanelWidth()
        {
            //if (this.LeftColumnWidth.Value <= 0.0) return;

            var over = this.Screen.ActualWidth - this.ScreenRect.ActualWidth;
            AdjustLeftColumn(AdjustRightColumn(over));
            UpdateCanvas();
        }

        private void AdjustRightPanelWidth()
        {
            //if (this.RightColumnWidth.Value <= 0.0) return;

            var over = this.Screen.ActualWidth - this.ScreenRect.ActualWidth;
            AdjustRightColumn(AdjustLeftColumn(over));
            UpdateCanvas();
        }

        private double AdjustLeftColumn(double over)
        {
            Debug.Assert(this.LeftColumnWidth.IsAbsolute);

            if (over <= 0.1) return over;

            var width = this.LeftColumnWidth.Value;
            var delta = Math.Min(width, over);
            this.LeftColumnWidth = new GridLength(width - delta);
            return over - delta;
        }

        private double AdjustRightColumn(double over)
        {
            Debug.Assert(this.RightColumnWidth.IsAbsolute);

            if (over <= 0.1) return over;

            var width = this.RightColumnWidth.Value;
            var delta = Math.Min(width, over);
            this.RightColumnWidth = new GridLength(width - delta);
            return over - delta;
        }

        #endregion ColumnWidth
    }

}
