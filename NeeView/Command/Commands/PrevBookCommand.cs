﻿namespace NeeView
{
    public class PrevBookCommand : CommandElement
    {
        public PrevBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandPrevFolder;
            this.Note = Properties.Resources.CommandPrevFolderNote;
            this.ShortCutKey = "Up";
            this.MouseGesture = "LU";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            var async = BookshelfFolderList.Current.PrevFolder();
        }
    }
}
