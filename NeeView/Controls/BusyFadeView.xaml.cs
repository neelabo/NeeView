﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// BusyFadeView.xaml の相互作用ロジック
    /// </summary>
    public partial class BusyFadeView : UserControl
    {
        public BusyFadeView()
        {
            InitializeComponent();
            Update();
        }


        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(BusyFadeView), new PropertyMetadata(false, IsBusyProperty_Changed));

        private static void IsBusyProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BusyFadeView control)
            {
                control.Update();
            }
        }


        public UIElement? Target
        {
            get { return (UIElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(UIElement), typeof(BusyFadeView), new PropertyMetadata(null));


        private void Update()
        {
            if (IsBusy)
            {
                this.Opacity = 0.0;
                this.BeginAnimation(UserControl.OpacityProperty, new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(1.0) });

                if (Target is not null)
                {
                    Target.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(1.0) });
                }

                this.IsHitTestVisible = true;
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.BeginAnimation(UserControl.OpacityProperty, null);
                this.Opacity = 0.0;

                if (Target is not null)
                {
                    Target.BeginAnimation(UIElement.OpacityProperty, null);
                    Target.Opacity = 1.0;
                }

                this.IsHitTestVisible = false;
                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}
