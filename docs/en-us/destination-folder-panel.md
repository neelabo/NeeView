# Destination folder panel

This fork adds a thin, dockable `DestinationFolderPanel` without changing image rendering, decoding, caching, or archive readers.

## Quick classification

The fork registers `MoveToDestinationFolder1` through `MoveToDestinationFolder9` as native commands with default shortcuts `1` through `9`. Their shortcut, destination index, and multi-page policy are editable in the normal command settings; enabling the Scripts folder is not required.

For the original NeeView 46.3 release, copy `SampleScripts/MoveToDestination1.nvjs` through `MoveToDestination9.nvjs` into the Scripts folder shown by **Options > Scripts > Open scripts folder**. The fork ZIP does not bundle these scripts, which prevents duplicate shortcuts if scripting is enabled later.

Keys `1` through `9` follow the persisted, manually managed Destination Folders order. Renaming a folder or changing its path does not require editing a script; reordering those folders changes the numeric mapping. Current-directory subfolders never consume number keys.

If those number keys are already assigned to another command or script, resolve the shortcut conflict in NeeView's command/script settings before using the classification scripts.

The original 46.3-compatible scripts always move images. The fork also has built-in number commands that follow the panel's Move / Copy mode, plus mode-aware scripts under `SampleScripts/Fork/`. Only real images opened from a folder can be classified. Archive, PDF, playlist, media, and link pages are rejected. `MultiPagePolicy` is `Once`, so only the main current image is handled.

## Panel and history

The panel has two sections. The upper section shows all persisted folders added through **Manage destination folders**; the lower section shows only the immediate subfolders of the current image directory. Neither section limits its item count, and each has its own scrollbar. Drag the divider between them to resize their visible areas; the ratio is saved and restored when the panel or app reopens. The Move / Copy and auto-refresh switches share one row.

Manual refresh and new-folder creation update only the lower section. The **Auto-refresh subfolders** switch is on by default and is persisted; it scans once when the current image directory changes, not periodically or on every page of the same directory. Turning it off leaves manual refresh available. A directory change clears the previous directory's child-folder list even when automatic refresh is off.

The separate persisted **Move / Copy** switch controls clicks in either section, new-folder classification, and built-in number commands. Move continues to the next image and enters session move history; Copy keeps the source and current page and does not enter move history. The original `MoveToFolderAs` command remains move-only for compatibility. The global `UndoDestinationMove` and `RedoDestinationMove` commands default to `Ctrl+Z` and `Ctrl+Y`; both shortcuts remain editable.

Previous releases of this fork saved manually refreshed child folders into the persistent destination collection. These old entries cannot safely be distinguished from manually added entries, so an upgrade keeps them. Remove any duplicates through **Manage destination folders** if desired.

Move history is kept in memory for the current session and is limited to 300 successful file moves by default. Its capacity is configurable from 0 to 1000 in the same settings section; zero disables move history. Cancelled or failed operations do not change the history. Undo and redo keep their top record when a file is missing or an overwrite is cancelled.

## Isolated profile

Use the ZIP package to keep the fork's `Profile` directory beside the executable, or set `NEEVIEW_PROFILE` to an existing absolute directory before starting NeeView. Do not replace files under `C:\Program Files\WindowsApps`.

To migrate settings once, export all settings from the Store version as an `.nvzip`, then import it into the fork. The profiles remain independent after the import.
