using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// PageOrderIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSortModeIcon : UserControl
    {
        public PageSortModeIcon()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }

        public PageSortMode PageSortMode
        {
            get { return (PageSortMode)GetValue(PageSortModeProperty); }
            set { SetValue(PageSortModeProperty, value); }
        }

        public static readonly DependencyProperty PageSortModeProperty =
            DependencyProperty.Register("PageSortMode", typeof(PageSortMode), typeof(PageSortModeIcon), new PropertyMetadata(PageSortMode.FileName, PageSortModePropertyChanged));

        private static void PageSortModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PageSortModeIcon)?.Refresh();
        }


        private void Refresh()
        {
            var key = "Icon" + Enum.GetName(typeof(PageSortMode), PageSortMode);
            this.Root.Content = this.Resources[key];
        }
    }
}
