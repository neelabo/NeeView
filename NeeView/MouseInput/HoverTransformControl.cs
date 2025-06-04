using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Windows;

namespace NeeView
{
    public class HoverTransformControl
    {
        private IDragTransformContextFactory _dragTransformContextFactory;
        private DragTransformContext? _transformContext;
        private bool _isEnabled;
        private DragActionProxy _action = new DragActionProxy();

        public HoverTransformControl(IDragTransformContextFactory dragTransformContextFactory)
        {
            _dragTransformContextFactory = dragTransformContextFactory;

            UpdateSelected();
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    UpdateSelected();
                }
            }
        }

        public void UpdateSelected()
        {
            if (_isEnabled)
            {
                _transformContext = _dragTransformContextFactory.CreateContentDragTransformContext(false);
                if (_transformContext is null)
                {
                    _action.ClearAction();
                    return;
                }

                var action = new HoverDragAction().CreateControl(_transformContext);
                _action.SetAction(action);
            }
            else
            {
                _transformContext = null;
                _action.ClearAction();
            }
        }

        public void HoverScroll(Point point, int timestamp, DragActionUpdateOptions options)
        {
            _action.Execute(point, timestamp, options);
        }
    }
}
