using NeeView.Properties;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// Redo the most recently undone destination-folder move.
    /// </summary>
    public sealed class RedoDestinationMoveCommand : CommandElement
    {
        /// <summary>
        /// Initialize the editable default shortcut and command group.
        /// </summary>
        public RedoDestinationMoveCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+Y");
            this.IsShowMessage = true;
        }

        /// <summary>
        /// Determine whether redo history exists and the move service is idle.
        /// </summary>
        /// <param name="sender">Command sender.</param>
        /// <param name="e">Command context.</param>
        /// <returns>True when a move can be redone.</returns>
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return DestinationMoveService.Current.CanRedo;
        }

        /// <summary>
        /// Start redo; the service handles serialization, errors, and history consistency.
        /// </summary>
        /// <param name="sender">Command sender.</param>
        /// <param name="e">Command context.</param>
        public override void Execute(object? sender, CommandContext e)
        {
            _ = DestinationMoveService.Current.RedoAsync(CancellationToken.None);
        }
    }
}
