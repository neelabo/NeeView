using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Dockable panel for quick destination-folder classification.
    /// </summary>
    public sealed class DestinationFolderPanel : ObservableObject, IPanel
    {
        private readonly Lazy<DestinationFolderPanelView> _view;

        /// <summary>
        /// Initialize the lazy panel view and sidebar icon.
        /// </summary>
        public DestinationFolderPanel()
        {
            _view = new Lazy<DestinationFolderPanelView>(() => new DestinationFolderPanelView());
            Icon = App.Current.MainWindow.Resources["pic_bookshelf"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource `pic_bookshelf`");
        }

#pragma warning disable CS0067
        /// <summary>
        /// Visibility-lock change event; this panel never locks visibility and does not raise it.
        /// </summary>
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067

        /// <summary>
        /// Get the stable panel type code used by the layout system.
        /// </summary>
        public string TypeCode => nameof(DestinationFolderPanel);

        /// <summary>
        /// Get the sidebar icon.
        /// </summary>
        public ImageSource Icon { get; }

        /// <summary>
        /// Get the icon tooltip text.
        /// </summary>
        public string IconTips => TextResources.GetString("DestinationFolderPanel.Title");

        /// <summary>
        /// Get the lazily created panel view.
        /// </summary>
        public Lazy<FrameworkElement> View => new Lazy<FrameworkElement>(() => _view.Value);

        /// <summary>
        /// Get whether the panel must remain visible.
        /// </summary>
        public bool IsVisibleLock => false;

        /// <summary>
        /// Get the default dock location.
        /// </summary>
        public PanelPlace DefaultPlace => PanelPlace.Right;

        /// <summary>
        /// Refresh destination folders when the view has already been created.
        /// </summary>
        public void Refresh()
        {
            if (_view.IsValueCreated)
            {
                _view.Value.Refresh();
            }
        }

        /// <summary>
        /// Create the view and move keyboard focus into the panel.
        /// </summary>
        public void Focus()
        {
            _view.Value.FocusAtOnce();
        }
    }
}
