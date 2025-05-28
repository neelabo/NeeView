using NeeView.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class RootBreadcrumbItem : Control
    {
        static RootBreadcrumbItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RootBreadcrumbItem), new FrameworkPropertyMetadata(typeof(RootBreadcrumbItem)));
        }


        private ComboBox? _mainComboBox;
        private WeakBindableBase<RootBreadcrumb>? _rootBreadcrumb;
        private bool _isMainComboBoxEnabled;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _mainComboBox = this.GetTemplateChild("PART_MainComboBox") as ComboBox ?? throw new InvalidOperationException();
            _mainComboBox.DropDownOpened += MainComboBox_DropDownOpened;
            _mainComboBox.DropDownClosed += MainComboBox_DropDownClosed;
            _mainComboBox.SelectionChanged += MainComboBox_SelectionChange;

            var comboBox = this.GetTemplateChild("PART_BreadcrumbComboBox") as ComboBox ?? throw new InvalidOperationException();
            comboBox.DropDownOpened += BreadcrumbComboBox_DropDownOpened;
            comboBox.DropDownClosed += BreadcrumbComboBox_DropDownClosed;
            comboBox.SelectionChanged += BreadcrumbComboBox_SelectionChanged;
        }


        public RootBreadcrumb? Breadcrumb
        {
            get { return (RootBreadcrumb)GetValue(BreadcrumbProperty); }
            set { SetValue(BreadcrumbProperty, value); }
        }

        public static readonly DependencyProperty BreadcrumbProperty =
            DependencyProperty.Register("Breadcrumb", typeof(RootBreadcrumb), typeof(RootBreadcrumbItem), new PropertyMetadata(null, BreadcrumbProperty_Changed));

        private static void BreadcrumbProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RootBreadcrumbItem control) return;
            control.AttachBreadcrumb();
        }


        private void AttachBreadcrumb()
        {
            if (_rootBreadcrumb is not null)
            {
                _rootBreadcrumb.PropertyChanged -= Breadcrumb_PropertyChanged;
                _rootBreadcrumb = null;
            }

            if (Breadcrumb is null)
            {
                return;
            }

            _rootBreadcrumb = new WeakBindableBase<RootBreadcrumb>(Breadcrumb);
            _rootBreadcrumb.PropertyChanged += Breadcrumb_PropertyChanged;
        }

        private void Breadcrumb_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            bool update = false;

            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(RootBreadcrumb.Parents))
            {
                update = true;
            }

            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(RootBreadcrumb.FirstChildren))
            {
                update = true;
            }

            if (update && _isMainComboBoxEnabled)
            {
                AppDispatcher.BeginInvoke(UpdateMainComboBoxItems);
            }
        }

        // NOTE: セパレーターを使うため、メインのComboBoxのアイテムを手動で更新する
        private void UpdateMainComboBoxItems()
        {
            if (_mainComboBox is null) return;

            _mainComboBox.Items.Clear();
            if (Breadcrumb is null) return;

            foreach (var item in Breadcrumb.Parents)
            {
                _mainComboBox.Items.Add(item);
            }

            if (Breadcrumb.FirstChildren.Count > 0)
            {
                _mainComboBox.Items.Add(new Separator());

                foreach (var item in Breadcrumb.FirstChildren)
                {
                    _mainComboBox.Items.Add(item);
                }
            }
        }

        private void MainComboBox_DropDownOpened(object? sender, EventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (Breadcrumb is null) return;

            Breadcrumb.LoadFirstChildren();
            UpdateMainComboBoxItems();
            _isMainComboBoxEnabled = true;
        }

        private void MainComboBox_DropDownClosed(object? sender, EventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (Breadcrumb is null) return;

            Breadcrumb?.CancelLoadFirstChildren();
            _isMainComboBoxEnabled = false;
        }

        private void MainComboBox_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (Breadcrumb is null) return;

            if (comboBox.SelectedItem is not BreadcrumbToken token) return;

            SetPath(token.Path.SimplePath);
        }

        private void BreadcrumbComboBox_DropDownOpened(object? sender, EventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (Breadcrumb is null) return;

            Breadcrumb.LoadChildren();
        }

        private void BreadcrumbComboBox_DropDownClosed(object? sender, EventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (Breadcrumb is null) return;

            Breadcrumb?.CancelLoadChildren();
        }

        private void BreadcrumbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (Breadcrumb is null) return;

            if (comboBox.SelectedItem is not BreadcrumbToken token) return;
            if (string.IsNullOrEmpty(token.Name)) return;

            SetPath(token.Path.SimplePath);
        }

        private void SetPath(string path)
        {
            if (VisualTreeHelper.GetParent(this) is BreadcrumbBar breadcrumbBar)
            {
                breadcrumbBar.SetPath(path);
            }
        }
    }
}
