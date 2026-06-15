using NeeView.Windows.Property;
using System.Windows;

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
            var page = new SettingPageInputScheme("Welcome");
            page.AddSettingItem(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsFileWriteAccessEnabled))));

            var welcomeWindow = new WelcomeDialog();
            welcomeWindow.PageContent.Content = page.Content;
            welcomeWindow.Owner = owner;
            welcomeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            welcomeWindow.ShowDialog();

            Config.Current.BookSettingDefault.BookReadOrder = page.PageReadOrder;
            CommandTable.Current.ResetCommandCollection(page.InputScheme, page.PageReadOrder);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
