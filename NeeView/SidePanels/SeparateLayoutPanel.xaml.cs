using NeeView.Runtime.LayoutPanel;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// SeparateLayoutPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class SeparateLayoutPanel : UserControl
    {
        public SeparateLayoutPanel()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        public LayoutPanel? LayoutPanel
        {
            get { return (LayoutPanel)GetValue(LayoutPanelProperty); }
            set { SetValue(LayoutPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayoutPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayoutPanelProperty =
            DependencyProperty.Register("LayoutPanel", typeof(LayoutPanel), typeof(SeparateLayoutPanel), new PropertyMetadata(null));
    }
}
