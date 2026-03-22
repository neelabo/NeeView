using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Navigate : Panel
    /// </summary>
    public class NavigatePanel : BindableBase, IPanel
    {
        private readonly Lazy<NavigateView> _view;

        public NavigatePanel(NavigateModel model)
        {
            _view = new(() => new NavigateView(model));

            Icon = App.Current.MainWindow.Resources["pic_navigate"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(NavigatePanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => TextResources.GetString("Navigator.Title");

        public Lazy<FrameworkElement> View => new(() => _view.Value);

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        public void Refresh()
        {
            // nop.
        }

        public void Focus()
        {
            _view.Value.FocusAtOnce();
        }
    }
}
