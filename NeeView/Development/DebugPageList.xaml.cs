using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// DebugPageList.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugPageList : UserControl
    {
        private DebugPageListViewModel _vm;

        public DebugPageList()
        {
            InitializeComponent();

            _vm = new DebugPageListViewModel();
            this.Root.DataContext = _vm;

            this.Loaded += DebugPageList_Loaded;
            this.Unloaded += DebugPageList_Unloaded;
        }

        private void DebugPageList_Loaded(object sender, RoutedEventArgs e)
        {
            BookOperation.Current.Control.SelectedRangeChanged += BookOperation_SelectedRangeChanged;
        }

        private void DebugPageList_Unloaded(object sender, RoutedEventArgs e)
        {
            BookOperation.Current.Control.SelectedRangeChanged -= BookOperation_SelectedRangeChanged;
            _vm.Dispose();
        }

        private void BookOperation_SelectedRangeChanged(object? sender, PageRangeChangedEventArgs e)
        {
            ////var page = BookOperation.Current.Book?.GetViewPage();
            ////this.Root.ScrollIntoView(page);
            ///
        }


        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.PageListView.Items.Refresh();

            long totalMemory = GC.GetTotalMemory(true);
            long workingSet = System.Environment.WorkingSet;
            Debug.WriteLine($"WorkingSet: {totalMemory:#,0}");
            Debug.WriteLine($"WorkingSet: {workingSet:#,0}");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Clear();
        }
    }



    public class PageContentToPictureSourceMemoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BitmapPageContent content)
            {
                //return string.Format("{0:#,0}", content.PictureSource?.GetMemorySize());
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PageContentToPictureMemoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BitmapPageContent content)
            {
                //return string.Format("{0:#,0}", content.Picture?.GetMemorySize());
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
