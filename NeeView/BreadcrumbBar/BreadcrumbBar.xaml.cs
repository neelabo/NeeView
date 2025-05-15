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
            DependencyProperty.Register("Profile", typeof(IBreadcrumbProfile), typeof(BreadcrumbBar), new PropertyMetadata(new DefaultBreadcrumbProfile(), ProfileProperty_Changed));

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

        public double PaddingWidth
        {
            get { return (double)GetValue(PaddingWidthProperty); }
            set { SetValue(PaddingWidthProperty, value); }
        }

        public static readonly DependencyProperty PaddingWidthProperty =
            DependencyProperty.Register("PaddingWidth", typeof(double), typeof(BreadcrumbBar), new PropertyMetadata(0.0, PaddingWidthProperty_Changed));

        private static void PaddingWidthProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            var query = Profile.GetQueryPath(path);
            if (query.IsNone)
            {
                return new();
            }

            var crumbs =  Enumerable.Range(0, query.Tokens.Length)
                .Select(e => new Breadcrumb(Profile, query, e))
                .ToList();

            return Profile.ArrangeBreadCrumbs(crumbs);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button control) return;
            if (control.DataContext is not Breadcrumb breadcrumb) return;

            SetPath(breadcrumb.Path.SimplePath);
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
            while (this.ItemsControl.ActualWidth + PaddingWidth + 1 > Root.ActualWidth)
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

            breadcrumb.LoadChildren();
        }


        private void BreadcrumbComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (comboBox.DataContext is not Breadcrumb breadcrumb) return;

            breadcrumb.CancelLoadChildren();
        }

        private void BreadcrumbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (comboBox.DataContext is not Breadcrumb breadcrumb) return;

            if (comboBox.SelectedItem is not BreadcrumbToken token) return;
            if (string.IsNullOrEmpty(token.Name)) return;

            SetPath(breadcrumb.Path.Combine(token.Name).SimplePath);
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
