using NeeView.IO;
using NeeView.Properties;
using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// Coordinate destination-folder moves, undo, and redo for the current session.
    /// </summary>
    public sealed class DestinationMoveService
    {
        private static DestinationMoveService? _current;
        private readonly object _syncRoot = new();
        private readonly List<DestinationMoveRecord> _undoHistory = new();
        private readonly List<DestinationMoveRecord> _redoHistory = new();
        private int _isBusy;

        /// <summary>
        /// Initialize session move history and observe capacity changes.
        /// </summary>
        private DestinationMoveService()
        {
            Config.Current.Panels.SubscribePropertyChanged(nameof(PanelsConfig.DestinationMoveHistoryCapacity),
                (s, e) => ApplyHistoryCapacity());
        }

        /// <summary>
        /// Get the destination move service shared by the current process.
        /// </summary>
        public static DestinationMoveService Current => _current ??= new DestinationMoveService();

        /// <summary>
        /// Raised when the busy state or either history stack changes.
        /// </summary>
        public event EventHandler? StateChanged;

        /// <summary>
        /// Get whether a file operation is in progress.
        /// </summary>
        public bool IsBusy => Volatile.Read(ref _isBusy) != 0;

        /// <summary>
        /// Get whether the most recent successful move can be undone.
        /// </summary>
        public bool CanUndo
        {
            get
            {
                lock (_syncRoot)
                {
                    return !IsBusy && _undoHistory.Count > 0;
                }
            }
        }

        /// <summary>
        /// Get whether the most recently undone move can be redone.
        /// </summary>
        public bool CanRedo
        {
            get
            {
                lock (_syncRoot)
                {
                    return !IsBusy && _redoHistory.Count > 0;
                }
            }
        }

        /// <summary>
        /// Move files to a destination folder and record the actual Shell results.
        /// </summary>
        /// <param name="paths">Real file paths to move.</param>
        /// <param name="destinationDirectory">Destination directory.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True when at least one file was moved successfully.</returns>
        public async Task<bool> TryMoveAsync(IEnumerable<string> paths, string destinationDirectory, CancellationToken token)
        {
            // Remove invalid and duplicate Windows paths before submitting the operation to the Shell.
            var sourcePaths = paths.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (sourcePaths.Count == 0 || !TryBeginOperation()) return false;

            try
            {
                // Validate the directory before submission so a directory string cannot become a file name.
                if (!FileIO.DirectoryExists(destinationDirectory))
                {
                    throw new DirectoryNotFoundException(destinationDirectory);
                }

                var result = await FileIO.SHMoveToFolderAsync(sourcePaths, destinationDirectory, token);
                var records = result.Items
                    .Where(e => !string.Equals(e.Source, e.Destination, StringComparison.OrdinalIgnoreCase))
                    .Select(e => new DestinationMoveRecord(e.Source, e.Destination))
                    .ToList();

                if (records.Count == 0)
                {
                    // A Shell provider can report neither success nor an exception; make that failure visible.
                    throw new IOException(TextResources.GetString("DestinationMove.NoFilesMoved"));
                }

                lock (_syncRoot)
                {
                    // Store successful Shell results independently so one failed item cannot block another undo.
                    foreach (var record in records)
                    {
                        PushWithCapacity(_undoHistory, record);
                    }
                    _redoHistory.Clear();
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ShowError(ex, TextResources.GetString("Message.MoveFailed"));
                return false;
            }
            finally
            {
                EndOperation();
            }
        }

        /// <summary>
        /// Undo the most recent successful destination-folder move.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True when the file was restored to its original path.</returns>
        public async Task<bool> UndoAsync(CancellationToken token)
        {
            return await ReplayAsync(true, token);
        }

        /// <summary>
        /// Redo the most recently undone destination-folder move.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True when the file was moved back to its destination.</returns>
        public async Task<bool> RedoAsync(CancellationToken token)
        {
            return await ReplayAsync(false, token);
        }

        /// <summary>
        /// Replay undo or redo and update history only after a successful move.
        /// </summary>
        /// <param name="isUndo">True for undo; false for redo.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True when the requested replay succeeds.</returns>
        private async Task<bool> ReplayAsync(bool isUndo, CancellationToken token)
        {
            if (!TryBeginOperation()) return false;

            try
            {
                DestinationMoveRecord? record;
                lock (_syncRoot)
                {
                    var sourceHistory = isUndo ? _undoHistory : _redoHistory;
                    record = sourceHistory.Count > 0 ? sourceHistory[^1] : null;
                }
                if (record is null) return false;

                var source = isUndo ? record.Destination : record.Source;
                var destination = isUndo ? record.Source : record.Destination;
                if (!FileIO.FileExists(source))
                {
                    ToastService.Current.Show(new Toast(
                        TextResources.GetFormatString("DestinationMove.SourceMissing", source),
                        TextResources.GetString("DestinationMove.Error.Title"),
                        ToastIcon.Warning));
                    return false;
                }

                if (FileIO.DirectoryExists(destination))
                {
                    ToastService.Current.Show(new Toast(
                        TextResources.GetFormatString("DestinationMove.TargetIsDirectory", destination),
                        TextResources.GetString("DestinationMove.Error.Title"),
                        ToastIcon.Warning));
                    return false;
                }

                var overwrite = FileIO.FileExists(destination);
                if (overwrite && !ConfirmOverwrite(destination)) return false;

                var result = await FileIO.SHMoveAsync(source, destination, overwrite, token);
                var moved = result.Items.Any(e =>
                    string.Equals(e.Source, source, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.Destination, destination, StringComparison.OrdinalIgnoreCase));
                if (!moved) return false;

                lock (_syncRoot)
                {
                    var sourceHistory = isUndo ? _undoHistory : _redoHistory;
                    var destinationHistory = isUndo ? _redoHistory : _undoHistory;

                    // The busy lock keeps the top stable; verify it again before changing history.
                    if (sourceHistory.Count == 0 || sourceHistory[^1] != record) return false;
                    sourceHistory.RemoveAt(sourceHistory.Count - 1);
                    PushWithCapacity(destinationHistory, record);
                }

                if (isUndo)
                {
                    RestoreVisiblePage(destination);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ShowError(ex, TextResources.GetString("DestinationMove.Error.Title"));
                return false;
            }
            finally
            {
                EndOperation();
            }
        }

        /// <summary>
        /// Show an explicit overwrite-or-cancel confirmation before replacing a file.
        /// </summary>
        /// <param name="destination">Path that would be replaced.</param>
        /// <returns>True when the user confirms overwrite.</returns>
        private static bool ConfirmOverwrite(string destination)
        {
            var dialog = new MessageDialog(
                TextResources.GetString("DestinationMove.Overwrite.Title"),
                TextResources.GetFormatString("DestinationMove.Overwrite.Message", destination),
                MessageDialogIcon.Warning);
            var overwriteCommand = new UICommand("Word.Overwrite") { IsPossible = true, IsDanger = true };
            dialog.Commands.Add(overwriteCommand);
            dialog.Commands.Add(UICommands.Cancel);
            dialog.DefaultCommandIndex = 1;
            dialog.CancelCommandIndex = 1;
            return dialog.ShowDialog(MainViewComponent.Current.GetWindow()).Command == overwriteCommand;
        }

        /// <summary>
        /// Reload and select the restored image when its folder is still open; otherwise show a notification.
        /// </summary>
        /// <param name="restoredPath">Restored original path.</param>
        private static void RestoreVisiblePage(string restoredPath)
        {
            var restoredDirectory = Path.GetDirectoryName(restoredPath);
            var currentBook = BookOperation.Current.Address;
            var currentBookDirectory = string.IsNullOrWhiteSpace(currentBook) ? null : Path.GetDirectoryName(currentBook);
            if (IsSamePath(currentBook, restoredDirectory) || IsSamePath(currentBookDirectory, restoredDirectory))
            {
                BookHub.Current.RequestReLoad(Current, Path.GetFileName(restoredPath));
                return;
            }

            ToastService.Current.Show(new Toast(
                TextResources.GetFormatString("DestinationMove.Restored", restoredPath),
                TextResources.GetString("UndoDestinationMoveCommand"),
                ToastIcon.Information));
        }

        /// <summary>
        /// Compare Windows paths that may contain relative segments or trailing separators.
        /// </summary>
        /// <param name="left">Left path.</param>
        /// <param name="right">Right path.</param>
        /// <returns>True when normalized paths are equal.</returns>
        private static bool IsSamePath(string? left, string? right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right)) return false;

            try
            {
                var normalizedLeft = Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var normalizedRight = Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return false;
            }
        }

        /// <summary>
        /// Try to acquire the non-queueing operation lock.
        /// </summary>
        /// <returns>True when no move operation is already running.</returns>
        private bool TryBeginOperation()
        {
            if (Interlocked.CompareExchange(ref _isBusy, 1, 0) != 0) return false;

            RaiseStateChanged();
            return true;
        }

        /// <summary>
        /// Release the operation lock and notify commands and panels.
        /// </summary>
        private void EndOperation()
        {
            Interlocked.Exchange(ref _isBusy, 0);
            RaiseStateChanged();
        }

        /// <summary>
        /// Push a record while respecting the configured history capacity.
        /// </summary>
        /// <param name="history">History list to update.</param>
        /// <param name="record">Record to add.</param>
        private static void PushWithCapacity(List<DestinationMoveRecord> history, DestinationMoveRecord record)
        {
            var capacity = Config.Current.Panels.DestinationMoveHistoryCapacity;
            if (capacity <= 0) return;

            if (history.Count >= capacity)
            {
                history.RemoveRange(0, history.Count - capacity + 1);
            }
            history.Add(record);
        }

        /// <summary>
        /// Trim undo and redo immediately after a capacity change and refresh commands.
        /// </summary>
        private void ApplyHistoryCapacity()
        {
            var capacity = Config.Current.Panels.DestinationMoveHistoryCapacity;
            lock (_syncRoot)
            {
                TrimToCapacity(_undoHistory, capacity);
                TrimToCapacity(_redoHistory, capacity);
            }
            RaiseStateChanged();
        }

        /// <summary>
        /// Trim a history list to capacity, removing the oldest records first.
        /// </summary>
        /// <param name="history">History list to trim.</param>
        /// <param name="capacity">Maximum number of records to keep.</param>
        private static void TrimToCapacity(List<DestinationMoveRecord> history, int capacity)
        {
            var removeCount = Math.Max(0, history.Count - capacity);
            if (removeCount > 0)
            {
                history.RemoveRange(0, removeCount);
            }
        }

        /// <summary>
        /// Notify the UI and global command system to recalculate state.
        /// </summary>
        private void RaiseStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Display a file-operation error as a toast.
        /// </summary>
        /// <param name="exception">Caught exception.</param>
        /// <param name="caption">Error caption.</param>
        private static void ShowError(Exception exception, string caption)
        {
            ToastService.Current.Show(new Toast(exception.Message, caption, ToastIcon.Error));
        }

        /// <summary>
        /// Store the actual source and destination of a successful move.
        /// </summary>
        /// <param name="Source">Path before the move.</param>
        /// <param name="Destination">Actual path after the move.</param>
        private sealed record DestinationMoveRecord(string Source, string Destination);
    }
}
