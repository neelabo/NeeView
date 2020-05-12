﻿namespace NeeView
{
    public class NextBookCommand : CommandElement
    {
        public NextBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandNextFolder;
            this.Note = Properties.Resources.CommandNextFolderNote;
            this.ShortCutKey = "Down";
            this.MouseGesture = "LD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            var async = BookshelfFolderList.Current.NextFolder();
        }
    }
}
