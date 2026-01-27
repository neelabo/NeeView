using NeeView.Properties;
using System.Diagnostics;
using System.Windows.Media;

namespace NeeView
{
    public class BreadcrumbToken
    {
        private readonly QueryPath _parent;
        private readonly string _name;
        private readonly string? _label;

        public BreadcrumbToken(QueryPath parent, string name, string? label)
        {
            Debug.Assert(parent is not null);

            _parent = parent;
            _name = name;
            _label = label;
        }

        public QueryPath Path => _parent.Combine(_name);
        public string Name => _name;
        public string Label => _label ?? Name;


        public override string ToString()
        {
            return Label;
        }
    }



    public class FileBreadcrumbToken : BreadcrumbToken
    {
        public FileBreadcrumbToken(QueryPath parent, string name, string? label) : base(parent, name, label)
        {
        }
    }

    public class SchemeBreadcrumbToken : BreadcrumbToken
    {
        public SchemeBreadcrumbToken(QueryScheme scheme) : base(QueryPath.None, scheme.ToSchemeString(), scheme.ToAliasName())
        {
            Scheme = scheme;
        }

        public QueryScheme Scheme { get; }
        public ImageSource ImageSource => Scheme.ToImage();
    }


    public class EmptyBreadcrumbToken : BreadcrumbToken
    {
        public EmptyBreadcrumbToken() : base(QueryPath.None, "", TextResources.GetString("Word.ItemNone"))
        {
        }
    }


    public class LoadingBreadcrumbToken : BreadcrumbToken
    {
        public LoadingBreadcrumbToken() : base(QueryPath.None, "", TextResources.GetString("Notice.LoadingTitle"))
        {
        }
    }
}
