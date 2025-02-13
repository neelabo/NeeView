using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class RemoveUnlinkedHistoryCommand : CommandElement
    {
        public RemoveUnlinkedHistoryCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var task = BookHistoryCollection.Current.RemoveUnlinkedAsync(CancellationToken.None);
            BookHistoryCollection.Current.ShowRemovedMessage(task.Result);
        }
    }
}
