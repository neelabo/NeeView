using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System.Windows;

namespace NeeView
{
    public class LayoutPanelAccessor
    {
        private readonly string _key;
        private readonly CustomLayoutPanelManager _manager;
        private readonly LayoutPanel _layoutPanel;

        public LayoutPanelAccessor(string key)
        {
            _key = key;
            _manager = CustomLayoutPanelManager.Current;
            _layoutPanel = _manager.Panels[key];
        }


        [WordNodeMember]
        public bool IsSelected
        {
            get { return AppDispatcher.Invoke(() => _manager.IsPanelSelected(_layoutPanel)); }
            set
            {
                if (value)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }

        [WordNodeMember]
        public bool IsVisible
        {
            get { return AppDispatcher.Invoke(() => _manager.IsPanelVisible(_layoutPanel)); }
        }

        [WordNodeMember]
        public bool IsFloating
        {
            get { return AppDispatcher.Invoke(() => _manager.IsPanelFloating(_layoutPanel)); }
        }

        [WordNodeMember]
        public WindowAccessor Window
        {
            get { return new WindowAccessor(new LayoutPanelWindowProxy(_key)); }
        }


        [WordNodeMember]
        public void Open()
        {
            AppDispatcher.Invoke(() => _manager.Open(_key, true));
        }

        [WordNodeMember]
        public void OpenDock()
        {
            AppDispatcher.Invoke(() => _manager.OpenDock(_key, true));
        }

        [WordNodeMember]
        public void OpenFloat()
        {
            AppDispatcher.Invoke(() => _manager.OpenWindow(_key, true));
        }

        [WordNodeMember]
        public void Close()
        {
            AppDispatcher.Invoke(() => _manager.Close(_key));
        }


        internal class LayoutPanelWindowProxy : WindowProxy
        {
            private readonly string _key;
            private readonly CustomLayoutPanelManager _manager;
            private readonly LayoutPanel _layoutPanel;

            public LayoutPanelWindowProxy(string key)
            {
                _key = key;
                _manager = CustomLayoutPanelManager.Current;
                _layoutPanel = _manager.Panels[key];
            }

            public override Window? Window => _manager.Windows.GetWindow(_layoutPanel);


            public override void SetWindowState(WindowStateEx state)
            {
                base.SetWindowState(state switch
                {
                    WindowStateEx.Minimized or WindowStateEx.FullScreen => WindowStateEx.None,
                    _ => state
                });
            }

            public override void Open()
            {
                if (Window is null)
                {
                    _manager.OpenWindow(_key, true);
                }
                else
                {
                    base.Open();
                }
            }

            public override void Close()
            {
                if (Window is not null)
                {
                    _manager.OpenDock(_key, true);
                }
            }
        }
    }
}
