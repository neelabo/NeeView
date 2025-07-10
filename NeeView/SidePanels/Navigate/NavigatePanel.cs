using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Properties;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Config.Current.Control.AddPropertyChanged(nameof(ControlConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, EventArgs.Empty));
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler? IsSelectedChanged;


        public string TypeCode => nameof(NavigatePanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => TextResources.GetString("Navigator.Title");

        public Lazy<FrameworkElement> View => new(() => _view.Value);

        public bool IsSelected
        {
            get { return Config.Current.Control.IsSelected; }
            set { if (Config.Current.Control.IsSelected != value) Config.Current.Control.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.Control.IsVisible;
            set => Config.Current.Control.IsVisible = value;
        }

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
