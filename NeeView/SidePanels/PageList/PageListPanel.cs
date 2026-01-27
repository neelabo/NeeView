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
    public class PageListPanel : BindableBase, IPanel
    {
        private readonly LazyEx<PageListView> _view;
        private readonly PageListPresenter _presenter;

        public PageListPanel(PageList model)
        {
            _view = new(() => new PageListView(model));
            _presenter = new PageListPresenter(_view, model);

            Icon = App.Current.MainWindow.Resources["pic_photo_library_24px"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(PageListPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => TextResources.GetString("PageList.Title");

        public Lazy<FrameworkElement> View => new(() => _view.Value);

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace { get; set; } = PanelPlace.Right;

        public PageListPresenter Presenter => _presenter;


        public void Refresh()
        {
            // nop.
        }

        public void Focus()
        {
            _presenter.FocusAtOnce();
        }
    }

}
