using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// MenuBar : Model
    /// </summary>
    public class MenuBar : BindableBase
    {
        private readonly WindowStateManager _windowStateManager;
        private bool _isMaximizeButtonMouseOver;


        public MenuBar(WindowStateManager windowStateManager)
        {
            _windowStateManager = windowStateManager;

            NeeView.MainMenu.Current.AddPropertyChanged(nameof(NeeView.MainMenu.Menu),
                (s, e) => RaisePropertyChanged(nameof(MainMenu)));
        }


        public Menu? MainMenu => NeeView.MainMenu.Current.Menu;

        public WindowStateManager WindowStateManager => _windowStateManager;

        public bool IsMaximizeButtonMouseOver => _isMaximizeButtonMouseOver;


        public void SetMaximizeButtonMouseOver(bool isOver)
        {
            _isMaximizeButtonMouseOver = isOver;
        }
    }
}
