using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NeeView
{
    public abstract class QuickAccessEntry : BindableBase, ITreeListNode, IRenameable
    {
        public abstract string? RawName { get; }
        public abstract string? Name { get; set; }
        public virtual string? Path { get => null; set { } }

        public virtual bool CanRename()
        {
            return false;
        }

        public virtual string GetRenameText()
        {
            return Name ?? "";
        }

        public virtual ValueTask<bool> RenameAsync(string name)
        {
            return ValueTask.FromResult(false);
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }

}
