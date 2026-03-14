using NeeView.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemFileAssociationGroupControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemFileAssociationGroupControl : UserControl, IHasWindowHandle
    {
        private SettingItemFileAssociationGroupViewModel? _vm;


        public SettingItemFileAssociationGroupControl()
        {
            InitializeComponent();
        }

        public SettingItemFileAssociationGroupControl(FileAssociationAccessorCollection collection, FileAssociationCategory category) : this()
        {
            _vm = new SettingItemFileAssociationGroupViewModel(collection, category, this);
            this.DataContext = _vm;
            this.Loaded += SettingItemFileAssociationGroupControl_Loaded;
            this.Unloaded += SettingItemFileAssociationGroupControl_Unloaded;
        }

        public IntPtr GetWindowHandle()
        {
            return WindowTools.GetWindowHandle(Window.GetWindow(this));
        }

        private void SettingItemFileAssociationGroupControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm?.Attach();
        }

        private void SettingItemFileAssociationGroupControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm?.Detach();
        }
    }
}
