using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class BreadcrumbItem : Control
    {
        static BreadcrumbItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbItem), new FrameworkPropertyMetadata(typeof(BreadcrumbItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var root = this.GetTemplateChild("PART_Root") as StackPanel ?? throw new InvalidOperationException();

            var button = this.GetTemplateChild("PART_MainButton") as Button ?? throw new InvalidOperationException();
            button.Click += MainButton_Click;

            var comboBox = this.GetTemplateChild("PART_BreadcrumbComboBox") as ComboBox ?? throw new InvalidOperationException();
            comboBox.DropDownOpened += BreadcrumbComboBox_DropDownOpened;
            comboBox.DropDownClosed += BreadcrumbComboBox_DropDownClosed;
            comboBox.SelectionChanged += BreadcrumbComboBox_SelectionChanged;
        }


        public Breadcrumb? Breadcrumb
        {
            get { return (Breadcrumb)GetValue(BreadcrumbProperty); }
            set { SetValue(BreadcrumbProperty, value); }
        }

        public static readonly DependencyProperty BreadcrumbProperty =
            DependencyProperty.Register("Breadcrumb", typeof(Breadcrumb), typeof(BreadcrumbItem), new PropertyMetadata(null));


        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button control) return;
            if (Breadcrumb is null) return;

            SetPath(Breadcrumb.Path.SimplePath);
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

            SetPath(Breadcrumb.Path.Combine(token.Name).SimplePath);
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
