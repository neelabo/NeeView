using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using NeeLaboratory.ComponentModel;
using System.Globalization;
using NeeView.Properties;

namespace NeeView
{
    /// <summary>
    /// VersionWindow の ViewModel
    /// </summary>
    public class VersionWindowViewModel : BindableBase
    {
        public VersionWindowViewModel()
        {
            var readmeFile = (TextResources.Culture.Name == "ja") ? "README.ja-jp.html" : "README.html";
            LicenseUri = "file://" + Environment.AssemblyFolder.Replace('\\', '/').TrimEnd('/') + $"/{readmeFile}";

            this.Icon = ResourceBitmapUtility.GetIconBitmapFrame("/Resources/App.ico", 256);

            // チェック開始
            Checker.CheckStart();
        }


        public string ApplicationName => Environment.ApplicationName;
        public string DisplayVersion => Environment.DisplayVersion;
        public string LicenseUri { get; private set; }
        public string ProjectUri => "https://neelabo.github.io/NeeView";
        public bool IsCheckerEnabled => Checker.IsEnabled;

        public BitmapFrame Icon { get; set; }

        // バージョンチェッカーは何度もチェックしないように static で確保する
        public static VersionChecker Checker { get; set; } = new VersionChecker();


        public void CopyVersionToClipboard()
        {
            Clipboard.SetText(Environment.VersionNote);
        }

    }
}
