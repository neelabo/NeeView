using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FileInformationPanel : BindableBase, IPanel
    {
        private readonly Lazy<FileInformationView> _view;
        private readonly FileInformation _model;

        public FileInformationPanel(FileInformation model)
        {
            _view = new(() => new FileInformationView(model));
            _model = model;

            Icon = App.Current.MainWindow.Resources["pic_info_24px"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource `pic_info_24px`");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(FileInformationPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => TextResources.GetString("Information.Title");

        public Lazy<FrameworkElement> View => new(() => _view.Value);

        public FileInformation FileInformation => _model;

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
