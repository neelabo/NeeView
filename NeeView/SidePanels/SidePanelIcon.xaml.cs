using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// SidePanelIcon.xaml の相互作用ロジック<br/>
    /// DataContextに <see cref="LayoutPanel"/> を要求する
    /// </summary>
    public partial class SidePanelIcon : UserControl
    {
        public SidePanelIcon()
        {
            InitializeComponent();

            this.Unloaded += (s, e) => Detach(Descriptor);
        }


        public ISidePanelIconDescriptor Descriptor
        {
            get { return (ISidePanelIconDescriptor)GetValue(DescriptorProperty); }
            set { SetValue(DescriptorProperty, value); }
        }

        public static readonly DependencyProperty DescriptorProperty =
            DependencyProperty.Register("Descriptor", typeof(ISidePanelIconDescriptor), typeof(SidePanelIcon), new PropertyMetadata(null, DescriptorPropertyChanged));

        private static void DescriptorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelIcon control)
            {
                control.Detach((ISidePanelIconDescriptor)e.OldValue);
                control.Update();
                control.Attach((ISidePanelIconDescriptor)e.NewValue);
                control.UpdateSelected();
            }
        }

        public HorizontalAlignment MarkerAlignment
        {
            get { return (HorizontalAlignment)GetValue(MarkerAlignmentProperty); }
            set { SetValue(MarkerAlignmentProperty, value); }
        }

        public static readonly DependencyProperty MarkerAlignmentProperty =
            DependencyProperty.Register("MarkerAlignment", typeof(HorizontalAlignment), typeof(SidePanelIcon), new PropertyMetadata(HorizontalAlignment.Left, MarkerAlignmentChanged));

        private static void MarkerAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelIcon control)
            {
                control.SelectedMark.HorizontalAlignment = (HorizontalAlignment)e.NewValue;
            }
        }

        private void Update()
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                this.ButtonContent.Content = Descriptor?.CreateButtonContent(layoutPanel);
            }
            else
            {
                this.ButtonContent.Content = null;
            }
        }

        private void Attach(ISidePanelIconDescriptor descriptor)
        {
            if (descriptor is null) return;

            descriptor.SelectedPanelChanged += Descriptor_SelectedPanelChanged;
        }

        private void Detach(ISidePanelIconDescriptor descriptor)
        {
            if (descriptor is null) return;

            descriptor.SelectedPanelChanged -= Descriptor_SelectedPanelChanged;
        }

        private void Descriptor_SelectedPanelChanged(object? sender, EventArgs e)
        {
            UpdateSelected();
        }

        private void UpdateSelected()
        {
            if (Descriptor is null) return;

            if (this.DataContext is LayoutPanel layoutPanel)
            {
                if (Descriptor.IsSelected(layoutPanel))
                {
                    SelectedMark.Visibility = Visibility.Visible;
                }
                else
                {
                    SelectedMark.Visibility = Visibility.Hidden;
                }
            }
        }

        private void TogglePanelCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                Descriptor?.ToggleLayoutPanel(layoutPanel);
            }
        }

        private void OpenDockCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                e.CanExecute = !CustomLayoutPanelManager.Current.IsSeparated(layoutPanel);
            }
        }

        private void OpenDockCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                CustomLayoutPanelManager.Current.OpenDock(layoutPanel);
            }
        }

        private void OpenWindowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                e.CanExecute = !CustomLayoutPanelManager.Current.IsSeparated(layoutPanel);
            }
        }

        private void OpenWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                CustomLayoutPanelManager.Current.OpenWindow(layoutPanel, WindowPlacement.None);
            }
        }

        private void DragStartBehavior_DragBegin(object sender, DragStartEventArgs e)
        {
            Descriptor?.DragBegin();
        }

        private void DragStartBehavior_DragEnd(object sender, EventArgs e)
        {
            Descriptor?.DragEnd();
        }
    }

    public interface ISidePanelIconDescriptor
    {
        event EventHandler? SelectedPanelChanged;

        bool IsSelected(LayoutPanel panel);
        FrameworkElement CreateButtonContent(LayoutPanel panel);
        void ToggleLayoutPanel(LayoutPanel panel);
        void DragBegin();
        void DragEnd();
    }
}
