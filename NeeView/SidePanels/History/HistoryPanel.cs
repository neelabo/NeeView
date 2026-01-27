using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 履歴パネル
    /// Type: ControlModel? ViewModelParts?
    /// </summary>
    public class HistoryPanel : BindableBase, IPanel
    {
        private readonly LazyEx<HistoryListView> _view;
        private readonly HistoryListPresenter _presenter;

        public HistoryPanel(HistoryList model)
        {
            _view = new(() => new HistoryListView(model));
            _presenter = new HistoryListPresenter(_view, model);

            Icon = App.Current.MainWindow.Resources["pic_history_24px"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(HistoryPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => TextResources.GetString("History.Title");

        public Lazy<FrameworkElement> View => new(() => _view.Value);

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;

        public HistoryListPresenter Presenter => _presenter;


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
