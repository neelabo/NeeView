using System;
using System.Windows;

namespace NeeView
{
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


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            if (node.Children is null) throw new InvalidOperationException();

            node.Children.Add(new WordNode(nameof(Window)));
            return node;
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
