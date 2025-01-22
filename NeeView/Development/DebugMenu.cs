using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
#if DEBUG
    public class DebugMenu : BindableBase
    {
        private DebugWindow? _debugWindow;

        private bool _isDebugWindowVisible;
        public bool IsDebugWindowVisible
        {
            get { return _isDebugWindowVisible; }
            set
            {
                if (SetProperty(ref _isDebugWindowVisible, value))
                {
                    if (_isDebugWindowVisible)
                    {
                        if (_debugWindow == null)
                        {
                            _debugWindow = new DebugWindow(MainWindow.Current.ViewModel);
                            _debugWindow.Owner = MainWindow.Current;
                            _debugWindow.Closed += (s, e) =>
                            {
                                _debugWindow = null;
                                _isDebugWindowVisible = false;
                            };
                            _debugWindow.Show();
                        }
                    }
                    else
                    {
                        _debugWindow?.Close();
                        _debugWindow = null;
                    }
                }
            }
        }

        public MenuItem CreateDevMenuItem()
        {
            var top = new MenuItem() { Header = Properties.TextResources.GetString("MenuTree.Debug") };
            var collection = top.Items;

            var item = new MenuItem() { Header = "Debug Window", IsCheckable = true };
            item.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(IsDebugWindowVisible)) { Source = this });
            collection.Add(item);

            collection.Add(new Separator());

            item = new MenuItem() { Header = "Open application folder" };
            item.Click += MenuItemDevApplicationFolder_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Open data folder" };
            item.Click += MenuItemDevApplicationDataFolder_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Open current folder" };
            item.Click += MenuItemDevCurrentFolder_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Open temp folder" };
            item.Click += MenuItemDevTempFolder_Click;
            collection.Add(item);

            collection.Add(new Separator());

            item = new MenuItem() { Header = "Export Colors.xaml" };
            item.Click += MenuItemDevExportColorsXaml_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Export embedded documents" };
            item.Click += MenuItemDevExportEmbeddedDocuments_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Stop RemoteServer" };
            item.Click += MenuItemDevStopRemoteServer_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "GC" };
            item.Click += MenuItemDevGC_Click;
            collection.Add(item);

            item = new MenuItem() { Header = "Go TEST" };
            item.Click += MenuItemDevButton_Click;
            collection.Add(item);

            return top;
        }

        // [開発用] 組み込みドキュメントを出力
        private void MenuItemDevExportEmbeddedDocuments_Click(object sender, RoutedEventArgs e)
        {
            DebugCommand.ExportEmbeddedDocuments();
            MessageBox.Show("Export done.");
        }

        // [開発用] RemoteServer停止
        private void MenuItemDevStopRemoteServer_Click(object sender, RoutedEventArgs e)
        {
            RemoteCommandService.Current.StopServer();
            MessageBox.Show("Stop RemoteServer");
        }

        // [開発用] Colors.xaml 出力
        private void MenuItemDevExportColorsXaml_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ThemeProfile is null) throw new InvalidOperationException();
            ThemeProfileTools.SaveColorsXaml(ThemeManager.Current.ThemeProfile, "Colors.xaml");
        }

        // [開発用] GCボタン
        private void MenuItemDevGC_Click(object sender, RoutedEventArgs e)
        {
            DebugGC();
        }

        // [開発用] テストボタン
        private void MenuItemDevButton_Click(object sender, RoutedEventArgs e)
        {
            DebugTestAction();
        }

        // 開発用コマンド：テンポラリフォルダーを開く
        private void MenuItemDevTempFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(Temporary.Current.TempDirectory);
        }

        // 開発用コマンド：アプリケーションフォルダーを開く
        private void MenuItemDevApplicationFolder_Click(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (path is null) throw new InvalidOperationException();
            DebugOpenFolder(path);
        }

        // 開発用コマンド：アプリケーションデータフォルダーを開く
        private void MenuItemDevApplicationDataFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(Environment.LocalApplicationDataPath);
        }

        // 開発用コマンド：カレントフォルダーを開く
        private void MenuItemDevCurrentFolder_Click(object sender, RoutedEventArgs e)
        {
            DebugOpenFolder(System.Environment.CurrentDirectory);
        }


        /// <summary>
        /// 開発用：GC
        /// </summary>
        [Conditional("DEBUG")]
        private static void DebugGC()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 開発用：テストボタンのアクション
        /// </summary>
        [Conditional("DEBUG")]
        private static void DebugTestAction()
        {
            _ = DebugTest.ExecuteTestAsync();
        }

        /// <summary>
        /// 開発用：フォルダーを開く
        /// </summary>
        [Conditional("DEBUG")]
        private static void DebugOpenFolder(string path)
        {
            Debug.WriteLine($"OpenFolder: {path}");
            ExternalProcess.OpenWithFileManager(path, true);
        }
    }

#endif // DEBUG
}
