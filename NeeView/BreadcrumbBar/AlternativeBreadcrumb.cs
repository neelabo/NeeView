using System.Collections.Generic;
using System.ComponentModel;


namespace NeeView
{
    /// <summary>
    /// 代替 Breadcrumb
    /// </summary>
    /// <remarks>
    /// 長さ制限で省略される Breadcrumb の代わりに使用する。
    /// </remarks>
    public class AlternativeBreadcrumb : Breadcrumb
    {
        private Breadcrumb? _breadcrumb;

        public AlternativeBreadcrumb() : this(null)
        {
        }

        public AlternativeBreadcrumb(Breadcrumb? breadcrumb)
        {
            Breadcrumb = breadcrumb;
        }

        public Breadcrumb? Breadcrumb
        {
            get { return _breadcrumb; }
            set
            {
                if (_breadcrumb != value)
                {
                    AttachBreadcrumb(value);
                    RaisePropertyChanged(null);
                }
            }
        }

        private void AttachBreadcrumb(Breadcrumb? breadcrumb)
        {
            DetachBreadcrumb();

            if (breadcrumb is null) return;

            _breadcrumb = breadcrumb;
            _breadcrumb.PropertyChanged += Breadcrumb_PropertyChanged;
        }

        private void DetachBreadcrumb()
        {
            if (_breadcrumb is null) return;

            _breadcrumb.PropertyChanged -= Breadcrumb_PropertyChanged;
            _breadcrumb = null;
        }


        private void Breadcrumb_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Breadcrumb.Children))
            {
                RaisePropertyChanged(nameof(Children));
                RaisePropertyChanged(nameof(HasChildren));
            }
        }

        public override QueryPath Path => Breadcrumb?.Path ?? new QueryPath("");

        public override string Name => "...";

        public override bool HasName => true;

        public override List<BreadcrumbToken> Children
        {
            get { return Breadcrumb?.Children ?? new(); }
            set { if (Breadcrumb is not null) Breadcrumb.Children = value; }
        }

        public override bool HasChildren => true;

        public override void LoadChildren() => Breadcrumb?.LoadChildren();

        public override void CancelLoadChildren() => Breadcrumb?.CancelLoadChildren();
    }

}
