using NeeView.ComponentModel;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
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
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// CustomLayoutPanelWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomLayoutPanelWindow : LayoutPanelWindow, IDisposable, IHasRenameManager
    {
        private RoutedCommandBinding? _routedCommandBinding;
        private bool _disposedValue;
        
        // インスタンス保持用
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:読み取られていないプライベート メンバーを削除", Justification = "<保留中>")]
        private readonly WindowStateManager _windowStateManager;



        public CustomLayoutPanelWindow()
        {
            InitializeComponent();
            WindowChromeTools.SetWindowChromeSource(this);

            this.DataContext = this;

            _windowStateManager = new WindowStateManager(this);
        }

        public CustomLayoutPanelWindow(LayoutPanelWindowManager manager, LayoutPanel layoutPanel) : this()
        {
            this.LayoutPanelWindowManager = manager;
            this.LayoutPanel = layoutPanel;
            this.Title = layoutPanel.Title;

            this.CaptionBar.ContextMenu = CreateContextMenu();

            _routedCommandBinding = new RoutedCommandBinding(this, RoutedCommandTable.Current);

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.CloseAll();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.CloseAll();
        }


        protected override void OnActivated(EventArgs e)
        {
            RoutedCommandTable.Current.UpdateInputGestures();

            base.OnActivated(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();

            base.OnClosed(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                PendingItemManager.Current.Cancel();
            }

            base.OnPreviewKeyDown(e);
        }

        #region IHasRenameManager

        public RenameManager GetRenameManager()
        {
            return this.RenameManager;
        }

        #endregion IHasRenameManager

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _routedCommandBinding?.Dispose();
                    _routedCommandBinding = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
