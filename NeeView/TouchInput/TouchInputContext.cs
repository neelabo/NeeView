using NeeView.PageFrames;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class TouchInputContext
    {
        public TouchInputContext(FrameworkElement sender, ICursorSetter? cursorSetter, MouseGestureCommandCollection? gestureCommandCollection, INotifyPageFrameBoxChanged? notifyPageFrameBoxChanged, IDragTransformContextFactory? dragTransformContextFactory, IDragTransformControl? dragTransformControl, LoupeContext? loupe, ViewScrollContext? viewScrollContext)
        {
            this.Sender = sender;
            this.CursorSetter = cursorSetter;
            this.GestureCommandCollection = gestureCommandCollection;
            this.NotifyPageFrameBoxChanged = notifyPageFrameBoxChanged;
            this.DragTransformContextFactory = dragTransformContextFactory;
            this.DragTransformControl = dragTransformControl;
            this.Loupe = loupe;
            this.ViewScrollContext = viewScrollContext;
        }


        public LoupeContext? Loupe { get; init; }

        public ViewScrollContext? ViewScrollContext { get; init; }

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        public ICursorSetter? CursorSetter { get; set; }

        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection? GestureCommandCollection { get; set; }

        public INotifyPageFrameBoxChanged? NotifyPageFrameBoxChanged { get; }

        public IDragTransformContextFactory? DragTransformContextFactory { get; init; }

        /// <summary>
        /// 操作する座標系
        /// </summary>
        public IDragTransformControl? DragTransformControl { get; set; }


        /// <summary>
        /// 有効なタッチデバイス情報
        /// </summary>
        public Dictionary<StylusDevice, TouchContext> TouchMap { get; set; } = new Dictionary<StylusDevice, TouchContext>();

        /// <summary>
        /// 速度計測器
        /// </summary>
        public MultiSpeedometer Speedometer { get; } = new MultiSpeedometer();
    }

}
