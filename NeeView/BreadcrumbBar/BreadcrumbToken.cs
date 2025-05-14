namespace NeeView
{
    public class BreadcrumbToken
    {
        private readonly string _parent;
        private readonly string _name;
        private readonly string? _label;


        public BreadcrumbToken(string parent, string name, string? label)
        {
            _parent = parent;
            _name = name;
            _label = label;
        }


        public string Path => LoosePath.Combine(_parent, _name);
        public string Name => _name;
        public string Label => _label ?? Name;
  

        public override string ToString()
        {
            return Label;
        }
    }



    public class FileBreadcrumbToken : BreadcrumbToken
    {
        public FileBreadcrumbToken(string parent, string name, string? label) : base(parent, name, label)
        {
        }
    }

    public class EmptyBreadcrumbToken : BreadcrumbToken
    {
        public EmptyBreadcrumbToken() : base("", "", ResourceService.GetString("@Word.ItemNone"))
        {
        }
    }

    public class LoadingBreadcrumbToken : BreadcrumbToken
    {
        public LoadingBreadcrumbToken() : base("", "", ResourceService.GetString("@Notice.LoadingTitle"))
        {
        }
    }
}
