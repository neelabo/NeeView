using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class ViewAutoScrollControl : BindableBase, IViewAutoScrollControl
    {
        private readonly MainViewComponent _viewComponent;

        public ViewAutoScrollControl(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;

            _viewComponent.MouseInput.SubscribePropertyChanged(nameof(MouseInput.IsAutoScrollMode),
                (s, e) => RaisePropertyChanged(nameof(IsAutoScrollMode)));
        }

        public bool IsAutoScrollMode
        {
            get { return _viewComponent.MouseInput.IsAutoScrollMode; }
            set { _viewComponent.MouseInput.IsAutoScrollMode = value; }
        }

        public void SetAutoScrollMode(bool isAutoScroll)
        {
            IsAutoScrollMode = isAutoScroll;
        }

        public bool GetAutoScrollMode()
        {
            return IsAutoScrollMode;
        }

        public void ToggleAutoScrollMode()
        {
            IsAutoScrollMode = !IsAutoScrollMode;
        }

    }
}
