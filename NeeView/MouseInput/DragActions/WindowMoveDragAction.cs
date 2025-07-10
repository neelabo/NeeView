using NeeView.Properties;
using NeeView.Windows;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class WindowMoveDragAction : DragAction
    {
        public WindowMoveDragAction()
        {
            Note = TextResources.GetString("DragActionType.WindowMove");
            DragKey = new DragKey("RightButton+LeftButton");
            DragActionCategory = DragActionCategory.None;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }

        private class ActionControl : DragActionControl
        {
            private Point _startPointFromWindow;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
            }

            public override void ExecuteBegin()
            {
                InitializeWindowDragPosition();
            }

            public override void Execute()
            {
                DragWindowMove(Context.First, Context.Last);
            }

            private void InitializeWindowDragPosition()
            {
                var window = Window.GetWindow(Context.Sender);
                if (window is null) return;

                var windowDiff = PointToLogicalScreen(window, new Point(0, 0)) - new Point(window.Left, window.Top);
                _startPointFromWindow = Context.Sender.TranslatePoint(Context.First, window) + windowDiff;
            }

            // ウィンドウ移動
            public void DragWindowMove(Point start, Point end)
            {
                var window = Window.GetWindow(Context.Sender);
                if (window is null) return;

                if (window.WindowState == WindowState.Normal)
                {
                    var pos = PointToLogicalScreen(Context.Sender, end) - _startPointFromWindow;
                    window.Left = pos.X;
                    window.Top = pos.Y;
                }
            }

            /// <summary>
            /// 座標を論理座標でスクリーン座標に変換
            /// </summary>
            private static Point PointToLogicalScreen(Visual visual, Point point)
            {
                var pos = visual.PointToScreen(point); // デバイス座標

                if (Window.GetWindow(visual) is IDpiScaleProvider dpiProvider)
                {
                    var dpi = dpiProvider.GetDpiScale();
                    pos.X = pos.X / dpi.DpiScaleX;
                    pos.Y = pos.Y / dpi.DpiScaleY;
                }
                return pos;
            }

        }
    }

}
