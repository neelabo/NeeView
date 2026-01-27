using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    public class LoupeDragTransformContext : DragTransformContext
    {
        private LoupeConfig _loupeConfig;
        private LoupeContext? _loupeContext;

        public LoupeDragTransformContext(FrameworkElement sender, ITransformControl transform, ViewConfig viewConfig, MouseConfig mouseConfig, LoupeConfig loupeConfig)
            : base(sender, transform, viewConfig, mouseConfig)
        {
            _loupeConfig = loupeConfig;
        }


        public Point LoupeBasePoint { get; set; }


        public override void Initialize(Point point, int timestamp)
        {
            base.Initialize(point, timestamp);
        }

        public void AttachLoupeContext(LoupeContext loupeContext)
        {
            DetachLoupeContext();

            _loupeContext = loupeContext;
            _loupeContext.PropertyChanged += LoupeContext_PropertyChanged;

            var center = new Point(0, 0); // ViewRect.Center();
            Vector v = First - center;
            LoupeBasePoint = (Point)(_loupeConfig.IsLoupeCenter ? -v : -v + v / _loupeContext.Scale);

            Update();
        }

        public void DetachLoupeContext()
        {
            if (_loupeContext is null) return;

            _loupeContext.PropertyChanged -= LoupeContext_PropertyChanged;
            _loupeContext = null;
        }

        private void LoupeContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        public void ZoomIn()
        {
            _loupeContext?.ZoomIn();
        }

        public void ZoomOut()
        {
            _loupeContext?.ZoomOut();
        }

        public void Update()
        {
            var span = TimeSpan.Zero;
            if (_loupeContext?.IsEnabled == true)
            {
                var point = LoupeBasePoint - (Last - First) * _loupeConfig.Speed;
                Transform.SetPoint(point, span);
                Transform.SetScale(_loupeContext.Scale, span);
            }
            else
            {
                Transform.SetPoint(new Point(0.0, 0.0), span);
                Transform.SetScale(1.0, span);
            }
        }
    }
}
