using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace NeeView
{
    public class QuickAccessFolder : QuickAccessEntry
    {
        private string? _name;

        public override string? RawName => _name;

        [NotNull]
        public override string? Name
        {
            get { return _name ?? ""; }
            set
            {
                var name = GetValidateName(value?.Trim());
                if (string.IsNullOrEmpty(name)) return;
                SetProperty(ref _name, name);
            }
        }

        public static string GetValidateName(string? name)
        {
            if (name is null) return "";
            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }

        public override bool CanRename()
        {
            return true;
        }

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        public override string GetRenameText()
        {
            return Name ?? "";
        }

        public override async ValueTask<bool> RenameAsync(string name)
        {
            Name = name;
            await ValueTask.CompletedTask;
            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }

}
