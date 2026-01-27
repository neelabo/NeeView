using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// 描写フレーム頻度を上げるためのダミーアニメーション再生処理
    /// </summary>
    /// <remarks>
    /// NOTE: 描写更新処理が実行されればよいので表示はしなくて良い
    /// </remarks>
    public class ActiveMarker : Control
    {
        static ActiveMarker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActiveMarker), new FrameworkPropertyMetadata(typeof(ActiveMarker)));
        }


        private RotateTransform? _rotateTransform;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rotateTransform = this.GetTemplateChild("PART_MarkerRotate") as RotateTransform ?? throw new InvalidOperationException();

            this.Loaded += (s, e) => UpdateActivity();
            this.IsVisibleChanged += (s, e) => UpdateActivity();
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ActiveMarker), new PropertyMetadata(false, IsActiveProperty_Changed));


        private static void IsActiveProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ActiveMarker control)
            {
                //Debug.WriteLine($"ActiveMarker.IsActive: {control.IsActive}");
                control.UpdateActivity();
            }
        }

        private void UpdateActivity()
        {
            if (_rotateTransform is null) return;

            if (IsActive && IsVisible)
            {
                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(2.0);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                _rotateTransform.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
            else
            {
                _rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null, HandoffBehavior.SnapshotAndReplace);
            }
        }
    }

}
