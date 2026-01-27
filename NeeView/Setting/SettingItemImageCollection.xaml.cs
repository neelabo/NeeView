using NeeView.Text;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemImageCollection.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemImageCollection : UserControl
    {
        public SettingItemImageCollection()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public SettingItemImageCollection(double height, bool isStoreHelpEnabled) : this()
        {
            this.ExtensionsView.Height = height;
        }

        #region Dependency Properties

        public StringCollection Collection
        {
            get { return (StringCollection)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(StringCollection), typeof(SettingItemImageCollection), new PropertyMetadata(null, CollectionPropertyChanged));

        private static void CollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingItemImageCollection control)
            {
                control.Refresh();
            }
        }

        #endregion

        public Config Config => Config.Current;

        private void Refresh()
        {
            if (Collection == null) return;
        }


        // from http://gushwell.ldblog.jp/archives/52279481.html
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ExternalProcess.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
