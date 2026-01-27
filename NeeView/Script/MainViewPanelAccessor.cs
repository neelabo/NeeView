using System.Windows;

#pragma warning disable CA1822

namespace NeeView
{
    [WordNodeMember]
    public class MainViewPanelAccessor
    {
        [WordNodeMember]
        public WindowAccessor Window
        {
            get { return new WindowAccessor(new MainViewWindowProxy()); }
        }

        [WordNodeMember]
        public void Open()
        {
            Window.Open();
        }

        [WordNodeMember]
        public void Close()
        {
            Window.Close();
        }


        internal class MainViewWindowProxy : WindowProxy
        {
            public override Window? Window => MainViewManager.Current.Window;

            public override void Open()
            {
                if (Window is null)
                {
                    MainViewManager.Current.SetFloating(true);
                }
                else
                {
                    base.Open();
                }
            }

            public override void Close()
            {
                MainViewManager.Current.SetFloating(false);
            }
        }
    }
}
