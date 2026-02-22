using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutDockPanel : Grid
    {
        const double _minContainerWidth = 40.0;
        const double _splitterWidth = 8.0;

        public LayoutPanelManager Manager
        {
            get { return (LayoutPanelManager)GetValue(ManagerProperty); }
            set { SetValue(ManagerProperty, value); }
        }

        public static readonly DependencyProperty ManagerProperty =
            DependencyProperty.Register("Manager", typeof(LayoutPanelManager), typeof(LayoutDockPanel), new PropertyMetadata(null));


        public LayoutPanelCollection ItemsSource
        {
            get { return (LayoutPanelCollection)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(LayoutPanelCollection), typeof(LayoutDockPanel), new PropertyMetadata(null, ItemsSourceProperty_Changed));

        private static void ItemsSourceProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LayoutDockPanel control)
            {
                control.ItemsSourceChanged((LayoutPanelCollection)e.NewValue, (LayoutPanelCollection)e.OldValue);
            }
        }


        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(LayoutDockPanel), new PropertyMetadata(Brushes.Transparent));



        private void ItemsSourceChanged(LayoutPanelCollection newValue, LayoutPanelCollection oldValue)
        {
            Debug.Assert(newValue == this.ItemsSource);

            if (oldValue != null)
            {
                oldValue.CollectionChanged -= ItemsSource_CollectionChanged;
            }

            if (newValue != null)
            {
                newValue.CollectionChanged += ItemsSource_CollectionChanged;
            }

            Flush();
        }

        private void ItemsSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Flush();
        }

        private void Flush()
        {
            if (Manager is null) throw new InvalidOperationException("need Provider");

            var containers = ItemsSource != null ? ItemsSource.Select(e => new LayoutPanelContainer(Manager, e)).ToList() : new List<LayoutPanelContainer>();
            var orientation = ItemsSource?.Orientation ?? Orientation.Vertical;

            this.Children.Clear();
            this.RowDefinitions.Clear();
            this.ColumnDefinitions.Clear();

            foreach (var container in containers)
            {
                AddPanelContainer(container, orientation);
                container.Snap();
                if (container != containers.Last())
                {
                    AddPanelSplitter(orientation);
                }
            }
        }

        private void AddPanelContainer(LayoutPanelContainer container, Orientation orientation)
        {
            if (orientation == Orientation.Vertical)
            {
                AddPanelContainerVertical(container);
            }
            else
            {
                AddPanelContainerHorizontal(container);
            }
        }

        private void AddPanelContainerVertical(LayoutPanelContainer container)
        {
            var rowDefinition = new RowDefinition();
            rowDefinition.MinHeight = _minContainerWidth;
            rowDefinition.SetBinding(RowDefinition.HeightProperty, new Binding(nameof(LayoutPanel.GridLength)) { Source = container.LayoutPanel, Mode = BindingMode.TwoWay });
            this.RowDefinitions.Add(rowDefinition);
            Grid.SetRow(container, this.RowDefinitions.Count - 1);
            this.Children.Add(container);
        }

        private void AddPanelContainerHorizontal(LayoutPanelContainer container)
        {
            var columnDefinition = new ColumnDefinition();
            columnDefinition.MinWidth = _minContainerWidth;
            columnDefinition.SetBinding(ColumnDefinition.WidthProperty, new Binding(nameof(LayoutPanel.GridLength)) { Source = container.LayoutPanel, Mode = BindingMode.TwoWay });
            this.ColumnDefinitions.Add(columnDefinition);
            Grid.SetColumn(container, this.ColumnDefinitions.Count - 1);
            this.Children.Add(container);
        }
        private void AddPanelSplitter(Orientation orientation)
        {
            if (orientation == Orientation.Vertical)
            {
                AddPanelSplitterVertical();
            }
            else
            {
                AddPanelSplitterHorizontal();
            }
        }

        private void AddPanelSplitterVertical()
        {
            var rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(_splitterWidth);
            this.RowDefinitions.Add(rowDefinition);
            var splitter = new GridSplitter()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Height = _splitterWidth - 2.0,
                FocusVisualStyle= App.Current.Resources["NVFocusVisual"] as Style,
            };
            splitter.SetBinding(GridSplitter.BackgroundProperty, new Binding(nameof(BorderBrush)) { Source = this });
            Grid.SetRow(splitter, this.RowDefinitions.Count - 1);
            this.Children.Add(splitter);
        }

        private void AddPanelSplitterHorizontal()
        {
            var columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(_splitterWidth);
            this.ColumnDefinitions.Add(columnDefinition);
            var splitter = new GridSplitter()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = _splitterWidth - 2.0,
                FocusVisualStyle = App.Current.Resources["NVFocusVisual"] as Style,
            };
            splitter.SetBinding(GridSplitter.BackgroundProperty, new Binding(nameof(BorderBrush)) { Source = this });
            Grid.SetColumn(splitter, this.ColumnDefinitions.Count - 1);
            this.Children.Add(splitter);
        }
    }
}
