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
    /// ImageEffect : Panel
    /// </summary>
    public class ImageEffectPanel : BindableBase, IPanel
    {
        private readonly Lazy<FrameworkElement> _view;

        public ImageEffectPanel(ImageEffect model)
        {
            _view = new (() =>new ImageEffectView(model));

            Icon = App.Current.MainWindow.Resources["pic_toy_24px"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(ImageEffectPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => TextResources.GetString("Effect.Title");

        public Lazy<FrameworkElement> View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        public void Refresh()
        {
            // nop.
        }

        public void Focus()
        {
            ((ImageEffectView)_view.Value).FocusAtOnce();
        }
    }
}
