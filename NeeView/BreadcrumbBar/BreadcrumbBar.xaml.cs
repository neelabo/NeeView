using NeeLaboratory.Generators;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
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
    /// BreadcrumbBar.xaml の相互作用ロジック
    /// </summary>
    public partial class BreadcrumbBar : UserControl
    {
        private List<Breadcrumb> _items = new();


        public BreadcrumbBar()
        {
            InitializeComponent();
        }


        [Subscribable]
        public event EventHandler<BreadcrumbBarEventArgs>? PathChanged;

        [Subscribable]
        public event RoutedEventHandler? PaddingFocused;


        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(BreadcrumbBar), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PathProperty_Changed));

        private static void PathProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                control.Update();
            }
        }

        public IBreadcrumbProfile Profile
        {
            get { return (IBreadcrumbProfile)GetValue(ProfileProperty); }
            set { SetValue(ProfileProperty, value); }
        }

        public static readonly DependencyProperty ProfileProperty =
            DependencyProperty.Register("Profile", typeof(IBreadcrumbProfile), typeof(BreadcrumbBar), new PropertyMetadata(new FileSystemBreadcrumbProfile(), ProfileProperty_Changed));

        private static void ProfileProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                control.Update();
            }
        }

        public bool IsPaddingVisible
        {
            get { return (bool)GetValue(IsPaddingVisibleProperty); }
            set { SetValue(IsPaddingVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsPaddingVisibleProperty =
            DependencyProperty.Register("IsPaddingVisible", typeof(bool), typeof(BreadcrumbBar), new PropertyMetadata(false, IsPaddingVisibleProperty_Changed));

        private static void IsPaddingVisibleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                var isPaddingVisible = (bool)(e.NewValue);
                control.BreadcrumbPadding.Visibility = isPaddingVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public double MinPadding
        {
            get { return (double)GetValue(MinPaddingProperty); }
            set { SetValue(MinPaddingProperty, value); }
        }

        public static readonly DependencyProperty MinPaddingProperty =
            DependencyProperty.Register("MinPadding", typeof(double), typeof(BreadcrumbBar), new PropertyMetadata(0.0, MinPaddingProperty_Changed));

        private static void MinPaddingProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                control.Update();
            }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(BreadcrumbBar), new PropertyMetadata(false, IsReadOnlyProperty_Changed));

        private static void IsReadOnlyProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                var isReadOnly = (bool)(e.NewValue);
                control.ItemsControlMask.Visibility = isReadOnly ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        private void Update()
        {
            _items = CreateBreadcrumbs(Path);
            this.ItemsControl.ItemsSource = _items;

            AppDispatcher.BeginInvoke(() => AdjustLayout());
        }

        public List<Breadcrumb> CreateBreadcrumbs(string path)
        {
            var queries = new QueryPath(path);

            return Enumerable.Range(0, queries.Tokens.Length)
                .Select(e => new Breadcrumb(Profile, queries, e))
                .ToList();
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button control) return;
            if (control.DataContext is not Breadcrumb breadcrumb) return;

            SetPath(breadcrumb.Path);
        }

        public void SetPath(string path)
        {
            Path = path;
            PathChanged?.Invoke(this, new BreadcrumbBarEventArgs(path));
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            AppDispatcher.BeginInvoke(() => AdjustLayout());
        }

        // TODO: UpdateLayout() ではなく、MeasureOverride(), ArrangeOverride() を使用した更新方法を検討する
        private void AdjustLayout()
        {
            _items.ForEach(e => e.TrimMode = BreadcrumbTrimMode.None);
            this.ItemsControl.UpdateLayout();

            int index = -2;
            while (this.ItemsControl.ActualWidth + MinPadding > Root.ActualWidth)
            {
                if (index > _items.Count - 4)
                {
                    break;
                }
                if (index + 1 >= 0)
                {
                    _items[index + 1].TrimMode = BreadcrumbTrimMode.Hide;
                }
                if (index + 2 >= 0)
                {
                    _items[index + 2].TrimMode = BreadcrumbTrimMode.Trim;
                }
                this.ItemsControl.UpdateLayout();

                index++;
            }
        }

        private void Padding_GotFocus(object sender, RoutedEventArgs e)
        {
            PaddingFocused?.Invoke(this, e);
        }

        private void Padding_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.BreadcrumbPadding.Focus();
        }

        private void BreadcrumbComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (comboBox.DataContext is not Breadcrumb breadcrumb) return;

            comboBox.ItemsSource = breadcrumb.Children;
        }

        private void BreadcrumbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (comboBox.DataContext is not Breadcrumb breadcrumb) return;

            if (comboBox.SelectedItem is not string subDirectory) return;

            var path = LoosePath.Combine(breadcrumb.Path, subDirectory);
            SetPath(path);
        }

    }


    public class BreadcrumbBarEventArgs : EventArgs
    {
        public BreadcrumbBarEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }

}
