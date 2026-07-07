using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// MessageDialog UICommand
    /// </summary>
    public class UICommand
    {
        public UICommand(string label)
        {
            this.Label = label;
        }

        public string Label { get; set; }

        public UICommandAlignment Alignment { get; set; }

        public bool IsPossible { get; set; }

        public bool IsDanger { get; set; }
    }


    /// <summary>
    /// Default UICommands
    /// </summary>
    public static class UICommands
    {
        public static UICommand OK { get; } = new UICommand("Word.OK") { IsPossible = true };
        public static UICommand Yes { get; } = new UICommand("Word.Yes") { IsPossible = true };
        public static UICommand No { get; } = new UICommand("Word.No");
        public static UICommand Cancel { get; } = new UICommand("Word.Cancel");
        public static UICommand Delete { get; } = new UICommand("Word.Delete") { IsPossible = true };
        public static UICommand Retry { get; } = new UICommand("Word.Retry") { IsPossible = true };

        // Usage: dialog.Commands.AddRange(...) 
        public static readonly List<UICommand> YesNo = new() { Yes, No };
        public static readonly List<UICommand> OKCancel = new() { OK, Cancel };
    }
}
