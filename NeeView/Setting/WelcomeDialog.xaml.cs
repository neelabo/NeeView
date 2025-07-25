using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
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

namespace NeeView.Setting
{
    /// <summary>
    /// WelcomeDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class WelcomeDialog : Window
    {
        public WelcomeDialog()
        {
            InitializeComponent();
        }

        public static void ShowDialog(Window owner)
        {
            var page = new SettingPageWelcome();

            var welcomeWindow = new WelcomeDialog();
            welcomeWindow.PageContent.Content = page.Content;
            welcomeWindow.Owner = owner;
            welcomeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            welcomeWindow.ShowDialog();

            CommandTable.Current.RestoreCommandCollection(page.InputScheme);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }

    public class SettingPageWelcome : SettingPage
    {
        private readonly CommandResetControl _commandResetControl;

        public SettingPageWelcome() : base("Welcome")
        {
            this.IsResetButtonEnabled = false;

            _commandResetControl = new CommandResetControl();

            this.Items = new List<SettingItem>();

            var group = new SettingItemGroup();
            group.Children.Add(new SettingItemContent(TextResources.GetString("CommandResetWindow.ResetType.Title"), _commandResetControl) { IsStretch = true });
            group.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsFileWriteAccessEnabled))));
            this.Items.Add(group);
        }

        public InputScheme InputScheme => _commandResetControl.InputScheme;
    }
}
