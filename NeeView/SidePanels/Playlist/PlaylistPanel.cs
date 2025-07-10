using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class PlaylistPanel : BindableBase, IPanel
    {
        private readonly LazyEx<PlaylistView> _view;
        private readonly PlaylistPresenter _presenter;


        public PlaylistPanel(PlaylistHub model)
        {
            _view = new(() => new PlaylistView(model));
            _presenter = new PlaylistPresenter(_view, model);

            Icon = App.Current.MainWindow.Resources["pic_playlist_24px"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(PlaylistPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => TextResources.GetString("Playlist.Title");

        public Lazy<FrameworkElement> View => new(() => _view.Value);

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;

        public PlaylistPresenter Presenter => _presenter;


        public void Refresh()
        {
            _presenter.Refresh();
        }

        public void Focus()
        {
            _presenter.FocusAtOnce();
        }
    }

}
