using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class DragActionProxy
    {
        private DragActionControl? _action;


        public bool DragKeyEquals(DragKey key)
        {
            return _action?.DragKey == key;
        }

        public void SetAction(DragActionControl? action)
        {
            _action?.Dispose();
            _action = action;
        }

        public void ClearAction()
        {
            _action?.Dispose();
            _action = null;
        }

        public void ExecuteBegin(Point point, int timestamp)
        {
            if (_action is null) return;
            _action.Context.Initialize(point, timestamp);
            _action.ExecuteBegin();
        }

        public void Execute(Point point, int timestamp, DragActionUpdateOptions options)
        {
            if (_action is null) return;
            _action.Context.Update(point, timestamp, options);
            if (options.HasFlag(DragActionUpdateOptions.Immediate))
            {
                _action.Flush();
            }
            else
            {
                _action.Execute();
            }
        }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (_action is null) return;
            _action.MouseWheel(e);
        }

        public void ExecuteEnd(Point point, int timestamp, ISpeedometer? speedometer, DragActionUpdateOptions options, bool continued)
        {
            if (_action is null) return;
            _action.Context.Update(point, timestamp, options);
            _action.ExecuteEnd(speedometer, continued);
        }
    }


}
