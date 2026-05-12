using CommunityToolkit.Mvvm.Input;
using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Runtime.LayoutPanel
{
    public partial class LayoutPanelWindow : Window, IDpiScaleProvider
    {
        private readonly DpiScaleProvider _dpiProvider;
        private readonly DisposableCollection _disposables = new();


        public LayoutPanelWindow()
        {
            _dpiProvider = new DpiScaleProvider();
            this.DpiChanged += (s, e) => _dpiProvider.SetDipScale(e.NewDpi);

            _disposables.Add(AppState.Current.SubscribePropertyChanged(nameof(AppState.IsHideWindow),
                (s, e) => AppStateTools.FlushWindowState(this)));
        }


        public LayoutPanelWindowManager LayoutPanelWindowManager
        {
            get { return (LayoutPanelWindowManager)GetValue(LayoutPanelWindowManagerProperty); }
            set { SetValue(LayoutPanelWindowManagerProperty, value); }
        }

        public static readonly DependencyProperty LayoutPanelWindowManagerProperty =
            DependencyProperty.Register("LayoutPanelWindowManager", typeof(LayoutPanelWindowManager), typeof(LayoutPanelWindow), new PropertyMetadata(null));


        public LayoutPanel? LayoutPanel
        {
            get { return (LayoutPanel?)GetValue(LayoutPanelProperty); }
            set { SetValue(LayoutPanelProperty, value); }
        }

        public static readonly DependencyProperty LayoutPanelProperty =
            DependencyProperty.Register("LayoutPanel", typeof(LayoutPanel), typeof(LayoutPanelWindow), new PropertyMetadata(null));


        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowPlacementTools.RestoreWindowPlacement(this, LayoutPanel?.WindowPlacement ?? WindowPlacement.None);
            base.OnSourceInitialized(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Snap();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _disposables.Dispose();

            if (LayoutPanel != null)
            {
                this.LayoutPanelWindowManager?.Closed(LayoutPanel);
            }

            base.OnClosed(e);
        }

        public DpiScale GetDpiScale()
        {
            return _dpiProvider.DpiScale;
        }

        public void Snap()
        {
            if (LayoutPanel is null) return;

            try
            {
                LayoutPanel.WindowPlacement = WindowPlacementTools.StoreWindowPlacement(this, withAeroSnap: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        protected ContextMenu CreateContextMenu()
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem() { Header = GetResource("Floating"), IsEnabled = false });
            contextMenu.Items.Add(new MenuItem() { Header = GetResource("Docking"), Command = OpenDockCommand });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = GetResource("Close"), Command = SystemCommands.CloseWindowCommand });

            return contextMenu;
        }

        private string GetResource(string key)
        {
            return LayoutPanelWindowManager?.Resources[key] ?? TextResources.GetLiteral(key);
        }

        [RelayCommand]
        private void OpenDock()
        {
            if (LayoutPanel != null)
            {
                this.LayoutPanelWindowManager?.LayoutPanelManager.OpenDock(LayoutPanel);
            }
        }
    }

}
