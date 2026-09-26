using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Display state and interaction commands for the destination-folder panel.
    /// </summary>
    public partial class DestinationFolderPanelViewModel : ObservableObject
    {
        private readonly DestinationMoveService _moveService;
        private bool _isFolderOperationBusy;
        // The directory and generation counter discard stale results from slow network scans.
        private string? _currentImageDirectory;
        private long _childFolderRefreshVersion;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateFolderCommand))]
        private string _newFolderName = "";

        /// <summary>
        /// Initialize destination folders and observe configuration, page, and move-history changes.
        /// </summary>
        public DestinationFolderPanelViewModel()
        {
            _moveService = DestinationMoveService.Current;
            ManagedItems = new ObservableCollection<DestinationFolderPanelItem>();
            CurrentFolderItems = new ObservableCollection<DestinationFolderPanelItem>();

            Config.Current.System.SubscribePropertyChanged(nameof(SystemConfig.DestinationFolderCollection),
                (s, e) => Refresh());
            Config.Current.Panels.SubscribePropertyChanged(nameof(PanelsConfig.IsDestinationFolderCopyMode),
                (s, e) => UpdateCommandStates());
            Config.Current.Panels.SubscribePropertyChanged(nameof(PanelsConfig.IsDestinationFolderAutoRefreshEnabled),
                (s, e) => OnAutoRefreshChanged());
            PageFrameBoxPresenter.Current.ViewPageChanged += (s, e) => OnCurrentPageChanged();
            BookOperation.Current.BookChanged += (s, e) => OnCurrentPageChanged();
            _moveService.StateChanged += (s, e) => UpdateCommandStates();

            Refresh();
            UpdateCurrentDirectory();
        }

        /// <summary>
        /// Get all manually managed, persisted destination folders.
        /// </summary>
        public ObservableCollection<DestinationFolderPanelItem> ManagedItems { get; }

        /// <summary>
        /// Get immediate subfolders of the current image directory without persisting them.
        /// </summary>
        public ObservableCollection<DestinationFolderPanelItem> CurrentFolderItems { get; }

        /// <summary>
        /// Get whether the managed section contains any destinations.
        /// </summary>
        public bool HasManagedItems => ManagedItems.Count > 0;

        /// <summary>
        /// Get whether the current-directory section contains any child folders.
        /// </summary>
        public bool HasCurrentFolderItems => CurrentFolderItems.Count > 0;

        /// <summary>
        /// Get whether a destination move is in progress.
        /// </summary>
        public bool IsBusy => _moveService.IsBusy;

        /// <summary>
        /// Rebuild the managed destination list from persistent configuration.
        /// </summary>
        public void Refresh()
        {
            ManagedItems.Clear();

            // Managed destinations have no item limit; only the first nine map to numeric shortcuts.
            foreach (var item in Config.Current.System.DestinationFolderCollection
                .Select((folder, index) => new DestinationFolderPanelItem(index + 1, folder)))
            {
                ManagedItems.Add(item);
            }

            OnPropertyChanged(nameof(HasManagedItems));
            UpdateCommandStates();
        }

        /// <summary>
        /// React to book and visible-page changes, refreshing child folders only after a directory change.
        /// </summary>
        private void OnCurrentPageChanged()
        {
            UpdateCurrentDirectory();
            UpdateCommandStates();
        }

        /// <summary>
        /// Handle the auto-refresh setting, discarding stale scans or reading the current directory once.
        /// </summary>
        private void OnAutoRefreshChanged()
        {
            ++_childFolderRefreshVersion;
            if (Config.Current.Panels.IsDestinationFolderAutoRefreshEnabled
                && _currentImageDirectory is { } currentDirectory)
            {
                _ = RefreshChildFoldersAsync(currentDirectory);
            }
        }

        /// <summary>
        /// Track the main image directory and refresh immediate children only after a real change.
        /// </summary>
        private void UpdateCurrentDirectory()
        {
            var directory = TryGetCurrentImageDirectory(out var currentDirectory) ? currentDirectory : null;
            if (string.Equals(_currentImageDirectory, directory, StringComparison.OrdinalIgnoreCase)) return;

            _currentImageDirectory = directory;
            ++_childFolderRefreshVersion;
            CurrentFolderItems.Clear();
            OnPropertyChanged(nameof(HasCurrentFolderItems));

            // Same-directory page changes do not scan, and no timer polls the file system.
            if (directory is not null && Config.Current.Panels.IsDestinationFolderAutoRefreshEnabled)
            {
                _ = RefreshChildFoldersAsync(directory);
            }
        }

        /// <summary>
        /// Determine whether the destination can receive the main image in the selected mode.
        /// </summary>
        /// <param name="item">Destination panel item.</param>
        /// <returns>True when the main image can be classified and the move service is idle.</returns>
        private bool CanClassify(DestinationFolderPanelItem? item)
        {
            return item is not null
                && !_moveService.IsBusy
                && BookOperation.Current.Control.CanMoveToFolder(item.Folder, MultiPagePolicy.Once)
                && (!Config.Current.Panels.IsDestinationFolderCopyMode
                    || BookOperation.Current.Control.CanCopyToFolder(item.Folder, MultiPagePolicy.Once));
        }

        /// <summary>
        /// Move or copy the current main image to the selected destination.
        /// </summary>
        /// <param name="item">Destination panel item.</param>
        [RelayCommand(CanExecute = nameof(CanClassify))]
        private void Classify(DestinationFolderPanelItem? item)
        {
            if (item is null) return;

            // Copy retains the source file and does not enter move undo history.
            if (Config.Current.Panels.IsDestinationFolderCopyMode)
            {
                BookOperation.Current.Control.CopyToFolder(item.Folder, MultiPagePolicy.Once);
            }
            else
            {
                BookOperation.Current.Control.MoveToFolder(item.Folder, MultiPagePolicy.Once);
            }
        }

        /// <summary>
        /// Determine whether a successful move can be undone.
        /// </summary>
        /// <returns>True when undo is available.</returns>
        private bool CanUndo()
        {
            return _moveService.CanUndo;
        }

        /// <summary>
        /// Undo the most recent successful move.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanUndo))]
        private async Task Undo()
        {
            await _moveService.UndoAsync(CancellationToken.None);
        }

        /// <summary>
        /// Determine whether an undone move can be redone.
        /// </summary>
        /// <returns>True when redo is available.</returns>
        private bool CanRedo()
        {
            return _moveService.CanRedo;
        }

        /// <summary>
        /// Redo the most recently undone move.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRedo))]
        private async Task Redo()
        {
            await _moveService.RedoAsync(CancellationToken.None);
        }

        /// <summary>
        /// Open the existing destination-folder management dialog.
        /// </summary>
        [RelayCommand]
        private void Manage()
        {
            DestinationFolderDialog.ShowDialog(MainViewComponent.Current.GetWindow());
        }

        /// <summary>
        /// Determine whether destinations can be refreshed from the current image folder.
        /// </summary>
        /// <returns>True when a regular file-system image is current and no folder operation is running.</returns>
        private bool CanRefreshFromCurrentFolder()
        {
            return !_isFolderOperationBusy && TryGetCurrentImageDirectory(out _);
        }

        /// <summary>
        /// Refresh immediate child folders without modifying managed destinations.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRefreshFromCurrentFolder))]
        private async Task RefreshFromCurrentFolder()
        {
            if (!TryGetCurrentImageDirectory(out var currentDirectory)) return;

            SetFolderOperationBusy(true);
            try
            {
                await RefreshChildFoldersAsync(currentDirectory);
            }
            finally
            {
                SetFolderOperationBusy(false);
            }
        }

        /// <summary>
        /// Determine whether a direct child folder can be created from the current input.
        /// </summary>
        /// <returns>True when the name and current image folder are valid and no folder operation is running.</returns>
        private bool CanCreateFolder()
        {
            return !_isFolderOperationBusy
                && !string.IsNullOrWhiteSpace(NewFolderName)
                && TryGetCurrentImageDirectory(out _);
        }

        /// <summary>
        /// Create a direct child folder, refresh destinations, and classify the current image.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCreateFolder))]
        private async Task CreateFolder()
        {
            if (!TryGetCurrentImageDirectory(out var currentDirectory)) return;
            var currentImagePath = BookOperation.Current.Book?.CurrentPage?.TargetPath;
            if (!TryCreateChildDirectoryPath(currentDirectory, NewFolderName, out var destinationPath))
            {
                ToastService.Current.Show(new Toast(
                    TextResources.GetString("DestinationFolderPanel.InvalidFolderName"),
                    TextResources.GetString("DestinationFolderPanel.CreateFailed"),
                    ToastIcon.Warning));
                return;
            }

            SetFolderOperationBusy(true);
            try
            {
                // Run file-system work in the background so mapped network folders do not block the panel.
                await Task.Run(() => Directory.CreateDirectory(destinationPath));
                NewFolderName = "";
                await RefreshChildFoldersAsync(currentDirectory);

                // Do not classify a different image if navigation occurred while creating the folder.
                if (!TryGetCurrentImageDirectory(out var activeDirectory)
                    || !string.Equals(activeDirectory, currentDirectory, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(BookOperation.Current.Book?.CurrentPage?.TargetPath, currentImagePath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Follow the selected mode; moving retains next-page behavior and undo history.
                var destinationFolder = new DestinationFolder(Path.GetFileName(destinationPath), destinationPath);
                if (BookOperation.Current.Control.CanMoveToFolder(destinationFolder, MultiPagePolicy.Once))
                {
                    if (Config.Current.Panels.IsDestinationFolderCopyMode)
                    {
                        if (BookOperation.Current.Control.CanCopyToFolder(destinationFolder, MultiPagePolicy.Once))
                        {
                            BookOperation.Current.Control.CopyToFolder(destinationFolder, MultiPagePolicy.Once);
                        }
                    }
                    else
                    {
                        BookOperation.Current.Control.MoveToFolder(destinationFolder, MultiPagePolicy.Once);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFolderOperationError(ex, "DestinationFolderPanel.CreateFailed");
            }
            finally
            {
                SetFolderOperationBusy(false);
            }
        }

        /// <summary>
        /// Get the real file-system directory containing the current main image.
        /// </summary>
        /// <param name="directory">Receives the containing directory on success.</param>
        /// <returns>True when the current page is a regular file-system image in an existing directory.</returns>
        private static bool TryGetCurrentImageDirectory(out string directory)
        {
            var page = BookOperation.Current.Book?.CurrentPage;
            if (page?.ArchiveEntry.Archive is not FolderArchive
                || !page.ArchiveEntry.IsFileSystem
                || page.ArchiveEntry.IsShortcut)
            {
                directory = "";
                return false;
            }

            directory = Path.GetDirectoryName(page.TargetPath) ?? "";
            return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory);
        }

        /// <summary>
        /// Enumerate immediate child folders asynchronously and update only the transient panel list.
        /// </summary>
        /// <param name="currentDirectory">Directory containing the current main image.</param>
        private async Task RefreshChildFoldersAsync(string currentDirectory)
        {
            var refreshVersion = ++_childFolderRefreshVersion;
            try
            {
                var folders = await Task.Run(() => EnumerateDestinationFolders(currentDirectory));

                // Network scans may be slow; accept only the newest result for the active directory.
                if (refreshVersion != _childFolderRefreshVersion
                    || !string.Equals(_currentImageDirectory, currentDirectory, StringComparison.OrdinalIgnoreCase)) return;

                CurrentFolderItems.Clear();
                foreach (var folder in folders)
                {
                    // Child folders never consume numeric shortcuts, and neither section has an item cap.
                    CurrentFolderItems.Add(new DestinationFolderPanelItem(null, folder));
                }

                OnPropertyChanged(nameof(HasCurrentFolderItems));
                ClassifyCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                if (refreshVersion == _childFolderRefreshVersion)
                {
                    ShowFolderOperationError(ex, "DestinationFolderPanel.RefreshFailed");
                }
            }
        }

        /// <summary>
        /// Enumerate a directory non-recursively and produce stably sorted destination folders.
        /// </summary>
        /// <param name="currentDirectory">Parent directory to read.</param>
        /// <returns>Immediate child folders sorted by name.</returns>
        private static List<DestinationFolder> EnumerateDestinationFolders(string currentDirectory)
        {
            return Directory.EnumerateDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly)
                .OrderBy(path => Path.GetFileName(path), StringComparer.CurrentCultureIgnoreCase)
                .Select(path => new DestinationFolder(Path.GetFileName(path), path))
                .ToList();
        }

        /// <summary>
        /// Validate the input and create a path constrained to a direct child of the image directory.
        /// </summary>
        /// <param name="parentDirectory">Directory containing the current image.</param>
        /// <param name="input">Folder name entered by the user.</param>
        /// <param name="destinationPath">Receives the full path when validation succeeds.</param>
        /// <returns>True when the input is safe to use as a direct child folder name.</returns>
        private static bool TryCreateChildDirectoryPath(string parentDirectory, string input, out string destinationPath)
        {
            var name = input.Trim();
            destinationPath = "";
            if (string.IsNullOrEmpty(name)
                || name is "." or ".."
                || name.EndsWith(".", StringComparison.Ordinal)
                || FileIO.ContainsInvalidFileNameChars(name)
                || IsReservedWindowsName(name))
            {
                return false;
            }

            try
            {
                var fullParentPath = Path.GetFullPath(parentDirectory);
                var fullDestinationPath = Path.GetFullPath(Path.Combine(fullParentPath, name));
                var destinationParent = Path.GetDirectoryName(fullDestinationPath);

                // Recheck the normalized parent so the input cannot escape the current image directory.
                if (destinationParent is null
                    || !string.Equals(
                        destinationParent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                        fullParentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                destinationPath = fullDestinationPath;
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determine whether the name is a reserved Windows device name.
        /// </summary>
        /// <param name="name">Folder name to inspect.</param>
        /// <returns>True when the name cannot represent a normal directory.</returns>
        private static bool IsReservedWindowsName(string name)
        {
            var stem = Path.GetFileNameWithoutExtension(name).ToUpperInvariant();
            return stem is "CON" or "PRN" or "AUX" or "NUL"
                || (stem.Length == 4
                    && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal))
                    && stem[3] is >= '1' and <= '9');
        }

        /// <summary>
        /// Update folder-operation busy state and reevaluate related commands.
        /// </summary>
        /// <param name="isBusy">Whether a folder operation is running.</param>
        private void SetFolderOperationBusy(bool isBusy)
        {
            _isFolderOperationBusy = isBusy;
            RefreshFromCurrentFolderCommand.NotifyCanExecuteChanged();
            CreateFolderCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Show a folder-operation error with a localized caption.
        /// </summary>
        /// <param name="exception">File-system exception.</param>
        /// <param name="captionResourceKey">Resource key for the error caption.</param>
        private static void ShowFolderOperationError(Exception exception, string captionResourceKey)
        {
            ToastService.Current.Show(new Toast(
                exception.Message,
                TextResources.GetString(captionResourceKey),
                ToastIcon.Error));
        }

        /// <summary>
        /// Notify panel properties and generated commands to recalculate state.
        /// </summary>
        private void UpdateCommandStates()
        {
            OnPropertyChanged(nameof(IsBusy));
            ClassifyCommand.NotifyCanExecuteChanged();
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
            RefreshFromCurrentFolderCommand.NotifyCanExecuteChanged();
            CreateFolderCommand.NotifyCanExecuteChanged();
        }
    }
}
