using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// PageMakers : View
    /// </summary>
    public class PageMarkersView : Canvas
    {
        public PageMarkers Source
        {
            get { return (PageMarkers)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(PageMarkers), typeof(PageMarkersView), new PropertyMetadata(null, SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PageMarkersView)?.Initialize();
        }


        private PageMarkersViewModel? _vm;


        private void Initialize()
        {
            _vm = new PageMarkersViewModel(this.Source, this);
            this.DataContext = _vm;
        }
    }
}
