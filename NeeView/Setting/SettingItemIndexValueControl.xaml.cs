using NeeView.Data;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemIndexValue.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemIndexValueControl : UserControl
    {
        public IIndexValue IndexValue
        {
            get { return (IIndexValue)GetValue(IndexValueProperty); }
            set { SetValue(IndexValueProperty, value); }
        }

        public static readonly DependencyProperty IndexValueProperty =
            DependencyProperty.Register("IndexValue", typeof(IIndexValue), typeof(SettingItemIndexValueControl), new PropertyMetadata(null));


        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(SettingItemIndexValueControl), new PropertyMetadata(false));


        public SettingItemIndexValueControl()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }
    }
}
