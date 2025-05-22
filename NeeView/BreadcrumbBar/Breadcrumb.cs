
using NeeLaboratory.ComponentModel;
using System.Collections.Generic;

namespace NeeView
{
    public abstract class Breadcrumb : BindableBase
    {
        public abstract QueryPath Path { get; }
        public abstract bool HasName { get; }
        public abstract string Name { get; }
        public abstract bool HasChildren { get; }
        public abstract List<BreadcrumbToken> Children { get; set; }

        public abstract void LoadChildren();
        public abstract void CancelLoadChildren();
    }
}