using NeeLaboratory;
using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 表示のTransform操作
    /// </summary>
    // TODO: PageFrames.ViewTransformControl と名前が競合
    public class ViewTransformControl : IViewTransformControl
    {
        private readonly PageFrameBoxPresenter _presenter;


        public ViewTransformControl(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;
        }



        // 水平スクロールの正方向
        // TODO: どうやって取得？どこから取得？
        public double ViewHorizontalDirection => Config.Current.BookSetting.BookReadOrder == PageReadOrder.LeftToRight ? 1.0 : -1.0;

        
        public void ScaleDown(ViewScaleCommandParameter parameter)
        {
            ScaleDown(ScaleType.TransformScale, parameter);
        }

        public void ScaleDown(ScaleType scaleType, ViewScaleCommandParameter parameter)
        {
            var control = GetDragTransform(Config.Current.View.ScaleCenter == DragControlCenter.Cursor);
            if (control is null) return;

            var scaleDelta = parameter.Scale;
            var isSnap = parameter.IsSnapDefaultScale;
            Debug.Assert(scaleDelta >= 0.0);
            var startScale = control.Context.GetStartScale(scaleType);
            var scale = startScale / (1.0 + scaleDelta);

            // TODO: 100%となるスケール。表示の100%にするかソースの100%にするかで変わってくる
            var originalScale = 1.0;

            if (isSnap)
            {
                if (Config.Current.Notice.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (startScale * originalScale > 1.01 && scale * originalScale < 1.01)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (startScale > 1.01 && scale < 1.01)
                    {
                        scale = 1.0;
                    }
                }
            }

            control.DoScale(scaleType, scale, TimeSpan.Zero);
        }


        public void ScaleUp(ViewScaleCommandParameter parameter)
        {
            ScaleUp(ScaleType.TransformScale, parameter);
        }

        public void ScaleUp(ScaleType scaleType, ViewScaleCommandParameter parameter)
        {
            var control = GetDragTransform(Config.Current.View.ScaleCenter == DragControlCenter.Cursor);
            if (control is null) return;

            var scaleDelta = parameter.Scale;
            var isSnap = parameter.IsSnapDefaultScale;
            Debug.Assert(scaleDelta >= 0.0);
            var startScale = control.Context.GetStartScale(scaleType);
            var scale = startScale * (1.0 + scaleDelta);

            // TODO: 100%となるスケール。表示の100%にするかソースの100%にするかで変わってくる
            var originalScale = 1.0;

            if (isSnap)
            {
                if (Config.Current.Notice.IsOriginalScaleShowMessage && originalScale > 0.0)
                {
                    // original scale 100% snap
                    if (startScale * originalScale < 0.99 && scale * originalScale > 0.99)
                    {
                        scale = 1.0 / originalScale;
                    }
                }
                else
                {
                    // visual scale 100% snap
                    if (startScale < 0.99 && scale > 0.99)
                    {
                        scale = 1.0;
                    }
                }
            }

            control.DoScale(scaleType, scale, TimeSpan.Zero);
        }


        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            Rotate(-parameter.Angle, parameter.IsStretch);
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            Rotate(parameter.Angle, parameter.IsStretch);
        }

        private void Rotate(double angle, bool isStretch)
        {
            var control = GetDragTransform(Config.Current.View.RotateCenter == DragControlCenter.Cursor);
            if (control is null) return;

            // スナップ値を下限にする
            if (Math.Abs(angle) < Config.Current.View.AngleFrequency)
            {
                angle = Config.Current.View.AngleFrequency * Math.Sign(angle);
            }

            control.DoRotate(MathUtility.NormalizeLoopRange(control.Context.StartAngle + angle, -180, 180), TimeSpan.Zero);

            if (isStretch)
            {
                Stretch(false);
            }
        }


        public void Stretch(bool ignoreViewOrigin)
        {
            _presenter.Stretch(ignoreViewOrigin);
        }


        private TimeSpan ScrollDuration() => TimeSpan.FromSeconds(Config.Current.View.ScrollDuration);

        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            if (Config.Current.Mouse.IsHoverScroll) return;

            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = ScrollDuration();

            var old = control.Point;
            control.DoMove(new Vector(control.Context.ViewRect.Width * rate, 0), span);

            if (parameter.AllowCrossScroll && control.Point.X == old.X)
            {
                control.DoMove(new Vector(0, control.Context.ViewRect.Height * rate * ViewHorizontalDirection), span);
            }
        }

        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            if (Config.Current.Mouse.IsHoverScroll) return;

            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = ScrollDuration();

            var old = control.Point;
            control.DoMove(new Vector(control.Context.ViewRect.Width * -rate, 0), span);

            if (parameter.AllowCrossScroll && control.Point.X == old.X)
            {
                control.DoMove(new Vector(0, control.Context.ViewRect.Height * -rate * ViewHorizontalDirection), span);
            }
        }

        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            if (Config.Current.Mouse.IsHoverScroll) return;

            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = ScrollDuration();

            var old = control.Point;
            control.DoMove(new Vector(0, control.Context.ViewRect.Height * -rate), span);

            if (parameter.AllowCrossScroll && control.Point.Y == old.Y)
            {
                control.DoMove(new Vector(control.Context.ViewRect.Width * -rate * ViewHorizontalDirection, 0), span);
            }
        }

        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            if (Config.Current.Mouse.IsHoverScroll) return;

            var control = GetDragTransform(false);
            if (control is null) return;

            var rate = parameter.Scroll;
            var span = ScrollDuration();

            var old = control.Point;
            control.DoMove(new Vector(0, control.Context.ViewRect.Height * rate), span);

            if (parameter.AllowCrossScroll && control.Point.Y == old.Y)
            {
                control.DoMove(new Vector(control.Context.ViewRect.Width * rate * ViewHorizontalDirection, 0), span);
            }
        }

        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            if (Config.Current.Mouse.IsHoverScroll) return;

            _presenter.ScrollToNext(LinkedListDirection.Next, parameter);
        }

        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
            if (Config.Current.Mouse.IsHoverScroll) return;

            _presenter.ScrollToNext(LinkedListDirection.Previous, parameter);
        }

        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            bool allowScroll = !Config.Current.Mouse.IsHoverScroll;
            _presenter.ScrollToNextFrame(LinkedListDirection.Next, parameter, parameter.LineBreakStopMode, parameter.EndMargin, allowScroll);
        }

        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            bool allowScroll = !Config.Current.Mouse.IsHoverScroll;
            _presenter.ScrollToNextFrame(LinkedListDirection.Previous, parameter, parameter.LineBreakStopMode, parameter.EndMargin, allowScroll);
        }


        public void FlipHorizontal(bool isFlip)
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipHorizontal(isFlip, TimeSpan.Zero);
        }

        public void FlipVertical(bool isFlip)
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipVertical(isFlip, TimeSpan.Zero);
        }

        public void ToggleFlipHorizontal()
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipHorizontal(!transform.IsFlipHorizontal, TimeSpan.Zero);
        }

        public void ToggleFlipVertical()
        {
            var transform = GetDragTransform(Config.Current.View.FlipCenter == DragControlCenter.Cursor);
            transform?.DoFlipVertical(!transform.IsFlipVertical, TimeSpan.Zero);
        }



        private DragTransform? GetDragTransform(bool isPointed)
        {
            var context = _presenter.CreateContentDragTransformContext(isPointed);
            if (context is null) return null;
            return new DragTransform(context);
        }


        public void ResetContentSizeAndTransform()
        {
            _presenter.ResetTransform();
        }
    }
}
