using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace NeeView
{
    public class QuickAccessFolder : BindableBase, IQuickAccessEntry, ICloneable
    {
        private string? _name;

        public string? RawName => _name;

        [NotNull]
        public string? Name
        {
            get { return _name ?? ""; }
            set
            {
                var name = GetValidateName(value?.Trim());
                if (string.IsNullOrEmpty(name)) return;
                SetProperty(ref _name, name);
            }
        }

        public string? Path
        {
            get { return null; }
            set { }
        }

        public static string GetValidateName(string? name)
        {
            if (name is null) return "";
            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }

        public virtual bool CanRename()
        {
            return true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string GetRenameText()
        {
            return Name ?? "";
        }

        public async ValueTask<bool> RenameAsync(string name)
        {
            Name = name;
            await ValueTask.CompletedTask;
            return true;
        }
    }

}
