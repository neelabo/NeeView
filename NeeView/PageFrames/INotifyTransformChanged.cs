using System;
using System.Diagnostics;

namespace NeeView.PageFrames
{
    public interface INotifyTransformChanged
    {
        event TransformChangedEventHandler? TransformChanged;
    }


    public enum TransformChangeSource
    {
        Logic,
        ViewControl,
    }

    public enum TransformCategory
    {
        Content,
        View,
        Loupe,
        BaseScale,
    }

    public enum TransformAction
    {
        Point,
        Angle,
        Scale,
        FlipHorizontal,
        FlipVertical,
    }

    public enum TransformTrigger
    {
        None,
        Clear,
        Snap,
        SnapTracking,
        WindowSnap,
    }

    public static class TransformActionTriggerExtensions
    {
        public static bool IsManualTrigger(this TransformTrigger trigger)
        {
            return trigger switch
            {
                TransformTrigger.None => true,
                TransformTrigger.Snap => true,
                _ => false
            };
        }
    }


    public class TransformChangedEventArgs : EventArgs
    {
        public TransformChangedEventArgs(ITransformControlObject source, TransformCategory category, TransformAction action)
            : this(source, category, action, TransformTrigger.None)
        {
        }

        public TransformChangedEventArgs(ITransformControlObject source, TransformCategory category, TransformAction action, TransformTrigger trigger)
        {
            switch (action)
            {
                case TransformAction.Scale:
                    Debug.Assert(source is IScaleControl);
                    break;
                case TransformAction.Angle:
                    Debug.Assert(source is IAngleControl);
                    break;
                case TransformAction.Point:
                    Debug.Assert(source is IPointControl);
                    break;
                case TransformAction.FlipHorizontal:
                case TransformAction.FlipVertical:
                    Debug.Assert(source is IFlipControl);
                    break;
                default:
                    throw new ArgumentException($"Source has not action: {action}");
            }

            Source = source;
            Category = category;
            Action = action;
            Trigger = trigger;
        }

        public TransformChangedEventArgs(TransformChangedEventArgs source)
        {
            Source = source.Source;
            Category = source.Category;
            Action = source.Action;
            Trigger = source.Trigger;
        }


        public ITransformControlObject Source { get; set; }
        public TransformCategory Category { get; }
        public TransformAction Action { get; }
        public TransformTrigger Trigger { get; init; }
    }

    /// <summary>
    /// 基準スケールを保持した <see cref="TransformChangedEventArgs"/>
    /// </summary>
    public class OriginalScaleTransformChangedEventArgs : TransformChangedEventArgs
    {
        public OriginalScaleTransformChangedEventArgs(TransformChangedEventArgs source, double originalScale) :
            base(source)
        {
            OriginalScale = originalScale;
        }

        public double OriginalScale { get; } = 1.0;
    }


    public delegate void TransformChangedEventHandler(object? sender, TransformChangedEventArgs e);
}
