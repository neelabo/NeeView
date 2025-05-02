using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NeeView.Windows
{
    public class RectangleAdorner : Adorner
    {
        private readonly AdornerLayer _layer;
        private bool _isAttached;
        private Point _start;
        private Point _end;
        private Brush _brush;

        public RectangleAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.IsHitTestVisible = false;
            this.UseLayoutRounding = true;
            this.SnapsToDevicePixels = true;

            _brush = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));

            _layer = AdornerLayer.GetAdornerLayer(adornedElement);
        }


        public Point Start
        {
            get { return _start; }
            set { if (_start != value) { _start = value; Update(); } }
        }

        public Point End
        {
            get { return _end; }
            set { if (_end != value) { _end = value; Update(); } }
        }

        public Brush Brush
        {
            get { return _brush; }
            set { if (_brush != value) { _brush = value; Update(); } }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var rect = new Rect(Start, End);
            drawingContext.DrawRectangle(_brush, null, rect);
        }

        public void Attach()
        {
            if (_layer != null && !_isAttached)
            {
                _layer.Add(this);
                _isAttached = true;
            }
        }

        public void Detach()
        {
            if (_layer != null && _isAttached)
            {
                _layer.Remove(this);
                _isAttached = false;
            }
        }

        private void Update()
        {
            _layer?.Update();
        }
    }
}