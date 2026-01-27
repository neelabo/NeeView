using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public sealed class BreadcrumbBar : Panel
    {
        private MeasureContext? _context;


        public event EventHandler<BreadcrumbBarEventArgs>? PathChanged;

        public event RoutedEventHandler? PaddingFocused;


        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(BreadcrumbBar), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PathProperty_Changed));

        private static void PathProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                control.Update();
            }
        }

        public IBreadcrumbProfile Profile
        {
            get { return (IBreadcrumbProfile)GetValue(ProfileProperty); }
            set { SetValue(ProfileProperty, value); }
        }

        public static readonly DependencyProperty ProfileProperty =
            DependencyProperty.Register("Profile", typeof(IBreadcrumbProfile), typeof(BreadcrumbBar), new PropertyMetadata(new DefaultBreadcrumbProfile(), ProfileProperty_Changed));

        private static void ProfileProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                control.Update();
            }
        }

        public bool IsPaddingVisible
        {
            get { return (bool)GetValue(IsPaddingVisibleProperty); }
            set { SetValue(IsPaddingVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsPaddingVisibleProperty =
            DependencyProperty.Register("IsPaddingVisible", typeof(bool), typeof(BreadcrumbBar), new PropertyMetadata(false, IsPaddingVisibleProperty_Changed));

        private static void IsPaddingVisibleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                // TODO: Padding要素のみ追加/削除を行う
                control.Update();
            }
        }

        public double PaddingWidth
        {
            get { return (double)GetValue(PaddingWidthProperty); }
            set { SetValue(PaddingWidthProperty, value); }
        }

        public static readonly DependencyProperty PaddingWidthProperty =
            DependencyProperty.Register("PaddingWidth", typeof(double), typeof(BreadcrumbBar), new PropertyMetadata(1.0, PaddingWidthProperty_Changed));

        private static void PaddingWidthProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                control.InvalidateMeasure();
            }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(BreadcrumbBar), new PropertyMetadata(false, IsReadOnlyProperty_Changed));

        private static void IsReadOnlyProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BreadcrumbBar control)
            {
                // TODO: Mask要素のみ追加/削除を行う
                control.Update();
            }
        }


        private void Update()
        {
            var root = new RootBreadcrumb();
            var items = CreateBreadcrumbs(Path);

            this.Children.Clear();

            this.Children.Add(new RootBreadcrumbItem() { Name = "Root", Breadcrumb = root });

            foreach (var item in items)
            {
                this.Children.Add(new BreadcrumbItem() { Breadcrumb = item });
            }

            if (this.IsReadOnly)
            {
                this.Children.Add(new Rectangle() { Name = "Mask", Fill = Brushes.Transparent });
            }

            if (this.IsPaddingVisible)
            {
                this.Children.Add(new BreadcrumbPadding() { Name = "Padding" });
            }
        }

        public List<Breadcrumb> CreateBreadcrumbs(string path)
        {
            var query = Profile.GetQueryPath(path);
            if (query.IsNone)
            {
                return new();
            }

            var crumbs = Enumerable.Range(0, query.Tokens.Length)
                .Select(e => new NormalBreadcrumb(Profile, query, e))
                .ToList<Breadcrumb>();

            return Profile.ArrangeBreadCrumbs(crumbs);
        }

        public void SetPath(string path)
        {
            Path = path;
            PathChanged?.Invoke(this, new BreadcrumbBarEventArgs(path));
        }

        public void RaisePaddingFocused(RoutedEventArgs e)
        {
            PaddingFocused?.Invoke(this, e);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _context = new MeasureContext(this);

            var width = 0.0;
            var height = 0.0;
            var childMeasureSize = new Size(double.PositiveInfinity, availableSize.Height);

            _context.Root.Measure(childMeasureSize);
            width += _context.Root.DesiredSize.Width;

            if (IsPaddingVisible)
            {
                width += PaddingWidth;
            }

            int index = 0;
            var children = _context.Items;
            for (index = 0; index < children.Count; index++)
            {
                var child = children[children.Count - index - 1];
                child.Measure(childMeasureSize);

                if (index > 0 && width + child.DesiredSize.Width > availableSize.Width)
                {
                    if (_context.Root.DesiredSize.Width < child.DesiredSize.Width)
                    {
                        _context.IsRoot = true;
                        if (_context.Root.Breadcrumb is RootBreadcrumb root)
                        {
                            root.Items = children.Take(children.Count - index).Select(e => e.Breadcrumb).WhereNotNull().ToList();
                        }
                        break;
                    }
                }

                width += child.DesiredSize.Width;
                height = Math.Max(height, child.DesiredSize.Height);
            }
            for (var i = 0; i < children.Count - index; i++)
            {
                children[i].Measure(default);
            }
            _context.Ellipsis = index;

            if (!_context.IsRoot)
            {
                _context.Root.Measure(default);
            }

            if (_context.Mask is not null)
            {
                _context.Mask.Measure(new Size(width, availableSize.Height));
            }

            if (_context.Padding is not null)
            {
                var size = new Size(Math.Max(availableSize.Width - width, 0.0), availableSize.Height);
                _context.Padding.Measure(new Size(Math.Max(availableSize.Width - width, 0.0), availableSize.Height));
                width += _context.Padding.DesiredSize.Width;
            }

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_context is null)
            {
                return base.ArrangeOverride(finalSize);
            }

            var position = default(Point);

            if (_context.IsRoot)
            {
                _context.Root.Arrange(new Rect(position, new Size(_context.Root.DesiredSize.Width, finalSize.Height)));
                position.X += _context.Root.DesiredSize.Width;
            }
            else
            {
                _context.Root.Arrange(default);
            }

            var children = _context.Items;
            for (var i = 0; i < children.Count - _context.Ellipsis; i++)
            {
                children[i].Arrange(default);
            }
            for (var i = _context.Ellipsis; i >= 1; i--)
            {
                var child = children[children.Count - i];
                child.Arrange(new Rect(position, new Size(child.DesiredSize.Width, finalSize.Height)));
                position.X += child.DesiredSize.Width;
            }

            if (_context.Mask is not null)
            {
                var size = new Size(position.X, finalSize.Height);
                _context.Mask.Arrange(new Rect(default, size));
            }

            if (_context.Padding is not null)
            {
                var size = new Size(Math.Max(finalSize.Width - position.X, 0.0), finalSize.Height);
                _context.Padding.Arrange(new Rect(position, size));
            }

            return finalSize;
        }


        private class MeasureContext
        {
            public MeasureContext(BreadcrumbBar breadcrumbBar)
            {
                Initialize(breadcrumbBar);
            }

            public int Ellipsis { get; set; }
            public bool IsRoot { get; set; }

            public RootBreadcrumbItem Root { get; private set; }
            public List<BreadcrumbItem> Items { get; private set; }
            public UIElement? Mask { get; private set; }
            public BreadcrumbPadding? Padding { get; private set; }


            [MemberNotNull(nameof(Root), nameof(Items))]
            private void Initialize(BreadcrumbBar breadcrumbBar)
            {
                Items = new();

                for (int i = 0; i < breadcrumbBar.Children.Count; i++)
                {
                    var child = (FrameworkElement)breadcrumbBar.Children[i];
                    switch (child.Name)
                    {
                        case "Root":
                            if (Root is not null) throw new InvalidOperationException("Root already exists.");
                            Root = (RootBreadcrumbItem)child;
                            break;
                        case "Mask":
                            if (Mask is not null) throw new InvalidOperationException("Mask already exists.");
                            Mask = child;
                            break;
                        case "Padding":
                            if (Padding is not null) throw new InvalidOperationException("Padding already exists.");
                            Padding = (BreadcrumbPadding)child;
                            break;
                        default:
                            Items.Add((BreadcrumbItem)child);
                            break;
                    }
                }

                if (Root is null) throw new InvalidOperationException("Root not found.");
            }
        }
    }
}
