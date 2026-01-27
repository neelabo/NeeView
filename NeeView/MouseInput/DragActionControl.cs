using System;
using System.Windows.Input;

namespace NeeView
{

    public class DragActionControl : IDisposable
    {
        private DragAction _source;
        private bool _disposedValue;

        public DragActionControl(DragTransformContext context, DragAction source)
        {
            Context = context;
            _source = source;
        }

        public DragTransformContext Context { get; }

        public DragKey DragKey => _source.DragKey;

        public DragActionParameter? Parameter => _source.Parameter;



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public virtual void ExecuteBegin()
        {
        }

        public virtual void Execute()
        {
        }

        public virtual void Flush()
        {
        }

        public virtual void ExecuteEnd(ISpeedometer? speedometer, bool continued)
        {
        }

        public virtual void MouseWheel(MouseWheelEventArgs e)
        {
        }

    }


    /// <summary>
    /// Normal DragActionControl.
    /// Context is NormalDragTransformContext 
    /// </summary>
    public class NormalDragActionControl : DragActionControl
    {
        public NormalDragActionControl(DragTransformContext context, DragAction source) : base(context, source)
        {
            Context = context as ContentDragTransformContext ?? throw new ArgumentException("need NormalDragTransformContext", nameof(context));
        }

        public new ContentDragTransformContext Context { get; }
    }

    /// <summary>
    /// DragActionControl for Loupe.
    /// Context is LoupeDragTransformContext 
    /// </summary>
    public class LoupeDragActionControl : DragActionControl
    {
        public LoupeDragActionControl(DragTransformContext context, DragAction source) : base(context, source)
        {
            Context = context as LoupeDragTransformContext ?? throw new ArgumentException("need LoupeDragTransformContext", nameof(context));
        }

        public new LoupeDragTransformContext Context { get; }
    }

}
