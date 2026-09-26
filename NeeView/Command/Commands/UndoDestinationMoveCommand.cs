using NeeView.Properties;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// Undo the most recent destination-folder move.
    /// </summary>
    public sealed class UndoDestinationMoveCommand : CommandElement
    {
        /// <summary>
        /// Initialize the editable default shortcut and command group.
        /// </summary>
        public UndoDestinationMoveCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+Z");
            this.IsShowMessage = true;
        }

        /// <summary>
        /// Determine whether undo history exists and the move service is idle.
        /// </summary>
        /// <param name="sender">Command sender.</param>
        /// <param name="e">Command context.</param>
        /// <returns>True when a move can be undone.</returns>
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return DestinationMoveService.Current.CanUndo;
        }

        /// <summary>
        /// Start undo; the service handles serialization, errors, and history consistency.
        /// </summary>
        /// <param name="sender">Command sender.</param>
        /// <param name="e">Command context.</param>
        public override void Execute(object? sender, CommandContext e)
        {
            _ = DestinationMoveService.Current.UndoAsync(CancellationToken.None);
        }
    }
}
