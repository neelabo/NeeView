using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NeeView
{
    public partial class BookPopupContent : UserControl
    {
        public BookPopupContent()
        {
            InitializeComponent();

            this.Loaded += (s, e) => this.Root.Focus();
        }


        public event EventHandler? SelfClosed;


        #region DependencyProperties

        public Popup ParentPopup
        {
            get { return (Popup)GetValue(ParentPopupProperty); }
            set { SetValue(ParentPopupProperty, value); }
        }

        public static readonly DependencyProperty ParentPopupProperty =
            DependencyProperty.Register("ParentPopup", typeof(Popup), typeof(BookPopupContent), new PropertyMetadata(null));

        #endregion


        private void MainContent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        Close();
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                case Key.Up:
                case Key.Right:
                case Key.Down:
                    e.Handled = true;
                    break;
            }
        }

        private void Close()
        {
            SelfClosed?.Invoke(this, EventArgs.Empty);

            if (ParentPopup != null)
            {
                ParentPopup.IsOpen = false;
            }
        }
    }
}
