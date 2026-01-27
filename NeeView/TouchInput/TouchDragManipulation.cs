//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// タッチ操作によるビューの変更
    /// </summary>
    [LocalDebug]
    public partial class TouchDragManipulation
    {
        private readonly IDragTransformContextFactory _transformContextFactory;
        private ContentDragTransformContext? _transformContext;

        private DragTransform? _transform;
        private TouchDragContext? _origin;

        private TouchDragTransform? _first;
        private TouchDragTransform? _start;
        private TouchDragTransform? _goal;

        private bool _allowAngle;
        private bool _allowScale;

        private readonly TouchInputContext _context;


        public TouchDragManipulation(TouchInputContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (context.DragTransformContextFactory is null) throw new ArgumentException("context.DragTransformContextFactory must not be null.");

            _context = context;
            _transformContextFactory = _context.DragTransformContextFactory;
        }


        public void Initialize()
        {
            _transformContext = _transformContextFactory.CreateContentDragTransformContext(true);
            if (_transformContext is null)
            {
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine($"Warning: Cannot get DragTransformContext");
                    Debugger.Break();
                }
                return;
            }

            LocalDebug.WriteLine("Init");
            _transform = new DragTransform(_transformContext);
            _first = GetNowTransform();
        }

        /// <summary>
        /// タッチ操作開始
        /// タッチ数が変化したときに呼ばれる
        /// </summary>
        public void Start()
        {
            if (_transform is null) return;

            LocalDebug.WriteLine("Start");
            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            _start = GetNowTransform();
            _goal = _start.Clone();

            _allowAngle = false;
            _allowScale = false;

            _transform.ResetInertia();
        }



        /// <summary>
        /// タッチ操作終了
        /// タッチ数が０になったときに呼ばれる
        /// </summary>
        public void Stop(int timestamp)
        {
            if (_transform is null) return;
            if (_goal is null) return;

            LocalDebug.WriteLine("Stop");
            _transform.SetAngle(GetSnapAngle(_goal.Angle), TimeSpan.FromMilliseconds(200));
            _transform.DoInertia(_context.Speedometer.GetVelocity(), InertiaTools.GetAcceleration(Config.Current.Touch.InertiaSensitivity));
        }

        /// <summary>
        /// タッチ操作情報変化
        /// </summary>
        public void Update(object? sender, StylusEventArgs e)
        {
            if (_transform is null) return;
            ControlDragTransform(e.Timestamp);
        }

        private void ControlDragTransform(int timestamp)
        {
            if (_transform is null) return;
            if (_goal is null) throw new InvalidOperationException("TouchDragManipulation must be started");

            _goal = GetTransform();

            //var span = TimeSpan.FromMilliseconds(64);
            var span = TimeSpan.Zero;
            _transform.AddPoint((Point)_goal.Trans - _transform.Point, span);
            _transform.SetAngle(_goal.Angle, span);
            _transform.SetScale(_goal.Scale, span);
        }

        private TouchDragTransform GetNowTransform()
        {
            if (_transform is null) return new TouchDragTransform();

            Debug.Assert(_transformContext != null);
            return new TouchDragTransform((Vector)_transform.Point, _transform.Angle, _transform.Scale, (Vector)_transformContext.ContentCenter);
        }

        /// <summary>
        /// タッチ状態から変換情報を求める
        /// </summary>
        /// <returns></returns>
        private TouchDragTransform GetTransform()
        {
            if (_origin is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_start is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_transformContext is null) throw new InvalidOperationException();

            var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            // center
            var center = (Vector)current.Center;

            // move
            var move = current.GetMove(_origin);

            // rotate
            var angle = current.GetAngle(_origin);
            _allowAngle = Config.Current.Touch.IsAngleEnabled && (_allowAngle || (current.Radius > Config.Current.Touch.MinimumManipulationRadius && Math.Abs(current.Radius * 2.0 * Math.Sin(angle * 0.5 * Math.PI / 180)) > Config.Current.Touch.MinimumManipulationDistance));
            angle = _allowAngle ? angle : 0.0;

            //  scale
            var scale = current.GetScale(_origin);
            _allowScale = Config.Current.Touch.IsScaleEnabled && (_allowScale || (current.Radius > Config.Current.Touch.MinimumManipulationRadius && Math.Abs(current.Radius - _origin.Radius) > Config.Current.Touch.MinimumManipulationDistance));
            scale = _allowScale ? scale : 1.0;

            var delta = new TouchDragTransform(move, angle, scale, center - _start.Center);
            return AddTransform(_start, delta);
        }

        /// <summary>
        /// TouchDragTransform の合成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private TouchDragTransform AddTransform(TouchDragTransform source, TouchDragTransform delta)
        {
            if (_transformContext is null) throw new InvalidOperationException();
            var contentCenter = _transformContext.ContentCenter;

            // center
            var center = source.Center + delta.Center;

            // trans
            var trans = (Vector)contentCenter;
            trans = trans + delta.Trans;

            // rotate
            var m = new RotateTransform(delta.Angle);
            trans = center + (Vector)m.Transform((Point)(trans - center));

            // scale
            trans = trans + (trans - center) * (delta.Scale - 1.0);

            var diff = trans - (Vector)contentCenter;
            return new TouchDragTransform(source.Trans + diff, source.Angle + delta.Angle, source.Scale * delta.Scale, center);
        }


        /// <summary>
        /// スナップ角度を求める
        /// </summary>
        /// <returns></returns>
        private double GetSnapAngle(double angle)
        {
            if (_first is null) throw new InvalidOperationException("TouchDragManipulation must be initialized");

            if (Config.Current.View.AngleFrequency > 0.0)
            {
                var delta = angle - _first.Angle;

                if (Math.Abs(delta) > 1.0)
                {
                    var direction = delta > 0.0 ? 1.0 : -1.0;
                    return Math.Floor((angle + Config.Current.View.AngleFrequency * (0.5 + direction * 0.25)) / Config.Current.View.AngleFrequency) * Config.Current.View.AngleFrequency;
                }
            }

            return angle;
        }

    }
}
