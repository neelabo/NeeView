using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace NeeView
{
    public abstract class MenuElement : BindableBase, ITreeListNode, IEquatable<MenuElement>
    {
        public MenuElement(MenuElementType type)
        {
            if (type == MenuElementType.History) throw new ArgumentException("MenuElementType.History is not supported.");
            MenuElementType = type;
        }


        public virtual bool IsEnabled => true;

        public string? Name { get; set; }

        public MenuElementType MenuElementType { get; set; }

        public string? CommandName { get; set; }

        public string Label
        {
            get { return string.IsNullOrEmpty(Name) ? DefaultLabel : Name; }
            set
            {
                Name = (value == DefaultLabel) ? null : value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DisplayLabel));
            }
        }

        public string DisplayLabel => Label.Replace("_", "", StringComparison.Ordinal);

        public virtual string DefaultLongLabel => $"《{MenuElementType.ToAliasName()}》";

        public virtual string DefaultLabel => MenuElementType.ToAliasName();

        public virtual string Note => "";


        public void RaisePropertyChangedAll()
        {
            RaisePropertyChanged(null);
        }

        public bool Equals(MenuElement? other)
        {
            if (other is null) return false;
            if (this.MenuElementType != other.MenuElementType) return false;
            if (this.Label != other.Label) return false;
            if (this.CommandName != other.CommandName) return false;
            return true;
        }

        // TODO: immutable化すれば不要になる
        public MenuElement Clone()
        {
            return (MenuElement)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }



    public class NoneMenuElement : MenuElement
    {
        public NoneMenuElement() : base(MenuElementType.None)
        {
        }

        public override bool IsEnabled => false;

        public override string DefaultLongLabel => $"({MenuElementType.ToAliasName()})";

        public override string DefaultLabel => $"({MenuElementType.ToAliasName()})";

        public override string Note => "";
    }


    public class GroupMenuElement : MenuElement, IRenameable
    {
        public GroupMenuElement() : base(MenuElementType.Group)
        {
        }

        public override string DefaultLongLabel => $"《{MenuElementType.ToAliasName()}》";

        public override string DefaultLabel => MenuElementType.ToAliasName();

        public override string Note => "";

        public string GetRenameText()
        {
            return Label;
        }

        public bool CanRename()
        {
            return true;
        }

        public async ValueTask<bool> RenameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            Label = name;
            await ValueTask.CompletedTask;
            return true;
        }
    }


    public class CommandMenuElement : MenuElement, IRenameable
    {
        public CommandMenuElement() : base(MenuElementType.Command)
        {
        }

        public override bool IsEnabled => CommandTools.IsCommandEnabled(CommandName);

        public override string DefaultLongLabel => CommandTools.GetCommandText(CommandName, (CommandElement e) => e.LongText);

        public override string DefaultLabel => CommandTools.GetCommandText(CommandName, (CommandElement e) => e.Menu);

        public override string Note => CommandTable.Current.GetElement(CommandName).Remarks;

        public string GetRenameText()
        {
            return Label;
        }

        public bool CanRename()
        {
            return true;
        }

        public async ValueTask<bool> RenameAsync(string name)
        {
            Label = name;
            await ValueTask.CompletedTask;
            return true;
        }
    }


    public class SeparatorMenuElement : MenuElement
    {
        public SeparatorMenuElement() : base(MenuElementType.Separator)
        {
        }
    }

}
