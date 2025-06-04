using System;
using System.Windows;
using System.Windows.Input;
using NeeView.PageFrames;

namespace NeeView
{
    /// <summary>
    /// 座標系操作のリソース基底
    /// </summary>
    public class DragTransformContext
    {
        public DragTransformContext(FrameworkElement sender, ITransformControl transform, ViewConfig viewConfig, MouseConfig mouseConfig)
        {
            Sender = sender;
            ViewRect = CreateViewRect();
            ViewConfig = viewConfig;
            MouseConfig = mouseConfig;
            Transform = transform;
        }

        public ViewConfig ViewConfig { get; }

        public MouseConfig MouseConfig { get; }

        public ITransformControl Transform { get; }

        public FrameworkElement Sender { get; }

        // NOTE: 画面中央を(0,0)とした座標系
        public Point OriginPoint { get; set; }
        public Point First { get; set; }
        public Point Old { get; set; }
        public Point Last { get; set; }

        public int FirstTimeStamp { get; set; }
        public int OldTimeStamp { get; set; }
        public int LastTimeStamp { get; set; }

        public Rect ViewRect { get; private set; }

        public Point StartPoint { get; set; }
        public double StartAngle { get; set; }
        public double StartScale { get; set; } = 1.0;
        public double StartBaseScale { get; set; } = 1.0;
        public bool StartFlipHorizontal { get; set; }
        public bool StartFlipVertical { get; set; }


        public virtual void Initialize(Point point, int timestamp)
        {
            OriginPoint = point;

            First = point;
            Old = point;
            Last = point;

            FirstTimeStamp = timestamp;
            OldTimeStamp = timestamp;
            LastTimeStamp = timestamp;

            StartPoint = Transform.Point;
            StartAngle = Transform.Angle;
            StartScale = Transform.Scale;
            StartFlipHorizontal = Transform.IsFlipHorizontal;
            StartFlipVertical = Transform.IsFlipVertical;

            StartBaseScale = Config.Current.BookSetting.BaseScale;
        }

        public double GetStartScale(ScaleType scaleType)
        {
            return scaleType switch
            {
                ScaleType.TransformScale => StartScale,
                ScaleType.BaseScale => StartBaseScale,
                _ => throw new ArgumentException("Not support ScaleType", nameof(scaleType))
            };
        }

        public void Update(Point point, int timestamp, DragActionUpdateOptions options)
        {
            if (!options.HasFlag(DragActionUpdateOptions.IgnoreUpdateState))
            {
                Old = Last;
                OldTimeStamp = LastTimeStamp;
                Last = point;
                LastTimeStamp = timestamp;
            }
        }

        public void UpdateViewRect()
        {
            ViewRect = CreateViewRect();
        }

        private Rect CreateViewRect()
        {
            var viewRect = new Size(Sender.ActualWidth, Sender.ActualHeight).ToRect();
            return viewRect;
        }
    }


    [Flags]
    public enum DragActionUpdateOptions
    {
        None = 0,
        IgnoreUpdateState = (1 << 0),
        Immediate = (1 << 1),
    }


    public enum ScaleType
    {
        TransformScale,
        BaseScale,
    }

}
