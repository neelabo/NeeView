using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    public class MoreMenuButton : Control
    {
        static MoreMenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MoreMenuButton), new FrameworkPropertyMetadata(typeof(MoreMenuButton)));
        }


        private ToggleButton? _moreButton;


        public ImageSource? ImageSource
        {
            get { return (ImageSource?)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(MoreMenuButton), new PropertyMetadata(null));


        public MoreMenuDescription Description
        {
            get { return (MoreMenuDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(MoreMenuDescription), typeof(MoreMenuButton), new PropertyMetadata(null, DescriptionChanged));

        private static void DescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MoreMenuButton control)
            {
                control.Reset();
            }
        }


        public ContextMenu? MoreMenu
        {
            get { return (ContextMenu)GetValue(MoreMenuProperty); }
            set { SetValue(MoreMenuProperty, value); }
        }

        public static readonly DependencyProperty MoreMenuProperty =
            DependencyProperty.Register("MoreMenu", typeof(ContextMenu), typeof(MoreMenuButton), new PropertyMetadata(null));


        private void Reset()
        {
            MoreMenu = Description?.Create();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _moreButton = (ToggleButton)GetTemplateChild("PART_MoreButton");
            _moreButton.Checked += MoreButton_Checked;
            _moreButton.Unchecked += MoreButton_Unchecked;
            _moreButton.MouseRightButtonUp += MoreButton_MouseRightButtonUp;
        }

        private void MoreButton_Checked(object sender, RoutedEventArgs e)
        {
            MoreButton_IsCheckedChanged();
        }

        private void MoreButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MoreButton_IsCheckedChanged();
        }

        private void MoreButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                toggleButton.IsChecked = !toggleButton.IsChecked;
                e.Handled = true;
            }
        }

        private void MoreButton_IsCheckedChanged()
        {
            if (Description != null)
            {
                if (MoreMenu is not null)
                {
                    MoreMenu = Description.Update(MoreMenu);
                }
            }

            if (_moreButton?.IsChecked == true)
            {
                ContextMenuWatcher.RaiseContextMenuOpening(this, _moreButton);
            }
            else
            {
                ContextMenuWatcher.RaiseContextMenuClosing(this, _moreButton);
            }
        }

    }
}
