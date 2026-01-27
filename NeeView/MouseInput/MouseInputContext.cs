using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System.Windows;

namespace NeeView
{
    public class MouseInputContext : BindableBase
    {
        public MouseInputContext(FrameworkElement sender, ICursorSetter? cursorSetter, MouseGestureCommandCollection? gestureCommandCollection, INotifyPageFrameBoxChanged? notifyPageFrameBoxChanged, IDragTransformContextFactory? dragTransformContextFactory, IDragTransformControl? dragTransformControl, LoupeContext? loupe, ViewScrollContext? viewScrollContext)
        {
            this.Sender = sender;
            this.CursorSetter = cursorSetter;
            this.GestureCommandCollection = gestureCommandCollection;
            this.NotifyPageFrameBoxChanged = notifyPageFrameBoxChanged;
            this.DragTransformContextFactory = dragTransformContextFactory;
            this.DragTransformControl = dragTransformControl;
            //this.DragTransform = dragTransform;
            //this.LoupeTransform = loupeTransform;
            this.Loupe = loupe;
            this.ViewScrollContext = viewScrollContext;
        }


        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; init; }

        public ICursorSetter? CursorSetter { get; set; }

        public LoupeContext? Loupe { get; init; }

        public ViewScrollContext? ViewScrollContext { get; init; }

        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection? GestureCommandCollection { get; init; }

        public INotifyPageFrameBoxChanged? NotifyPageFrameBoxChanged { get; init; }

        public IDragTransformContextFactory? DragTransformContextFactory { get; init; }

        public IDragTransformControl? DragTransformControl { get; init; }

        //public DragTransform? DragTransform { get; init; }

        //public LoupeTransform? LoupeTransform { get; init; }

        public bool IsGestureEnabled { get; init; } = true;

        public bool IsLeftButtonDownEnabled { get; init; } = true;

        public bool IsRightButtonDownEnabled { get; init; } = true;

        public bool IsVerticalWheelEnabled { get; init; } = true;

        public bool IsHorizontalWheelEnabled { get; init; } = true;

        public bool IsMouseEventTerminated { get; init; } = true;

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }

        public int StartTimestamp { get; set; }

        public Speedometer Speedometer { get; } = new Speedometer();
    }

}
