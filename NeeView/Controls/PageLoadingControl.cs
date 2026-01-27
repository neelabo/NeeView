using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NeeView
{
    /// <summary>
    /// PageLoadingControl.xaml の相互作用ロジック
    /// </summary>
    public class PageLoadingControl : Control
    {
        static PageLoadingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PageLoadingControl), new FrameworkPropertyMetadata(typeof(PageLoadingControl)));
        }


        private StackPanel? _root;
        private ProgressRing? _progressRing;

        public PageLoadingControl()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _root = this.GetTemplateChild("PART_Root") as StackPanel ?? throw new InvalidOperationException();
            _progressRing = this.GetTemplateChild("PART_ProgressRing") as ProgressRing ?? throw new InvalidOperationException();

            Update();
        }


        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(PageLoadingControl), new PropertyMetadata(false, IsActiveProperty_Changed));

        private static void IsActiveProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageLoadingControl control)
            {
                control.Update();
            }
        }


        public string? Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(PageLoadingControl), new PropertyMetadata(null));


        private void Update()
        {
            if (_root is null) return;
            if (_progressRing is null) return;

            if (IsActive)
            {
                _root.Opacity = 0;
                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)) { BeginTime = TimeSpan.FromSeconds(0.2) };
                _root.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);
                _root.Visibility = Visibility.Visible;
                _progressRing.IsActive = true;
            }
            else
            {
                _root.BeginAnimation(UIElement.OpacityProperty, null, HandoffBehavior.SnapshotAndReplace);
                _root.Visibility = Visibility.Collapsed;
                _progressRing.IsActive = false;
            }
        }
    }
}
