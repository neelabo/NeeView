using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace NeeView
{
    public class RootBreadcrumb : AlternativeBreadcrumb
    {
        private List<Breadcrumb>? _items;
        private Breadcrumb? _first;
        private List<BreadcrumbToken> _parents = new();
        private List<BreadcrumbToken> _firstChildren = new();


        public override QueryPath Path => _first?.Path ?? new QueryPath("");

        public List<Breadcrumb>? Items
        {
            get { return _items; }
            set
            {
                if (SetProperty(ref _items, value))
                {
                    AttachFirstBreadcrumb(_items?.FirstOrDefault());
                    Breadcrumb = _items?.LastOrDefault();
                }
            }
        }

        public List<BreadcrumbToken> Parents
        {
            get { return _parents; }
            set { SetProperty(ref _parents, value); }
        }

        public List<BreadcrumbToken> FirstChildren
        {
            get { return _firstChildren; }
            set { SetProperty(ref _firstChildren, value); }
        }


        private void AttachFirstBreadcrumb(Breadcrumb? breadcrumb)
        {
            DetachFirstBreadcrumb();

            if (breadcrumb is null) return;

            _first = breadcrumb;
            _first.PropertyChanged += FirstBreadcrumb_PropertyChanged;
        }

        private void DetachFirstBreadcrumb()
        {
            if (_first is null) return;

            _first.PropertyChanged -= FirstBreadcrumb_PropertyChanged;
            _first = null;
        }

        private void FirstBreadcrumb_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Breadcrumb.Children))
            {
                UpdateFirstChildren();
            }
        }

        private void UpdateParents()
        {
            var parents = _items?.Where(e => e.HasName).Select(e => CreateBreadcrumbToken(e.Path, e.Name)).Reverse();
            Parents = parents?.ToList() ?? new();
        }

        private void UpdateFirstChildren()
        {
            FirstChildren = HasFirstChildren() ? _first.Children : new();
        }

        [MemberNotNullWhen(true, nameof(_first))]
        private bool HasFirstChildren()
        {
            // 名前を持たない場合はパスと無関係の最初の選択肢があると判断する
            return _first != null && !_first.HasName;
        }

        private BreadcrumbToken CreateBreadcrumbToken(QueryPath path, string name)
        {
            if (path.Scheme == QueryScheme.File)
            {
                return new FileBreadcrumbToken(path, "", name);
            }

            return new BreadcrumbToken(path, "", name);
        }

        public void LoadFirstChildren()
        {
            UpdateParents();
            UpdateFirstChildren();

            if (HasFirstChildren())
            {
                _first.LoadChildren();
            }
        }

        public void CancelLoadFirstChildren()
        {
            _first?.CancelLoadChildren();
        }
    }

}
