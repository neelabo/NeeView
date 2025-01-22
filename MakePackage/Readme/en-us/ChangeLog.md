## ChangeLog

----

### 42.6
(2025-01-22)

#### Changed

- 7z.dll 24.09
- Language: Updated zh-Hans

----

### 42.5
(2025-01-12)

#### Fixed

- Bookshelf: Fixed a bug that compressed files may disappear from the list when they are edited.

----

### 42.4
(2024-12-23)

#### Fixed

- System: Fixed a bug that caused multi-boot to fail under certain conditions.

----

### 42.3
(2024-12-22)

#### Fixed

- System: Fixed a bug that caused startup to fail with old language settings.

#### Changed

- Language: Updated zh-Hans.

----

### 42.2
(2024-12-08)

#### Fixed

- Bookshelf: Corrected a bug in file name case-only renaming.
- PageList: Fixed a bug that the display was not updated when the file name was changed in the page list.
- View: Corrected a bug in the behavior of automatic rotation under certain conditions in the main view window.
- View: Reduced application error on loading corrupted GIFs.
- Pages: Width, Height set to 0 for archive pages and other pages with no size.
- Script: Fixed a bug that ViewPageAccessor's Width and Height could not be obtained in videos.

#### Changed

- Language: Updated zh-Hans.

----

### 42.1
(2024-12-01)

#### Fixed

- System: Fixed a bug in version check.
- Script: Fixed a bug about CreationTime. 

----

### 42.0
(2024-11-24)

#### Added

- System: Dialog and toast notification text can now be copied to the clipboard.
- System: File manager can be configured to replace Explorer.  (Settings > General)
- System: Added the ability to select “Open as book” from the context menu of the video page.
- System: Support for copying per archive folder.
- System: Embedded a link to a wiki page explaining the format in the JSON file of the sample theme.
- Command: Added toast notification flag to “Save” command parameter.
- Command: Added selective “External app” command. It is equivalent to the command in the page menu.
- Command: Added selective “Copy to folder” command. Equivalent to the command in the page menu.
- Command: Added selective “Move to folder” command. Equivalent to the command in the page menu.
- Book: Added split rate setting for page splitting. (Settings > Book > Rete of divide page) 
- Book: Added setting to determine the reference page only by number when “Two pages" is selected.  (Settings > Book)
- Book: Added setting for how to align the size of each page in “Two pages". (Settings > Book)
- Book: Image start position can be set horizontally and vertically respectively.  (Settings > View operation) 
- Address bar: Add button to address bar to prohibit book switching.
- Panel: The same operations as in the PageList context menu, such as rename, can now be performed in the Information panel and Filmstrip page context menus.
- Panel: Selection mark displayed on side panel icon.
- MainView window: Added setting to disable the MainView window mode when the MainView window is closed.  (Settings > Main view) 
- MainView window: Added “MainView window auto show” setting. (Settings >Main view)
- Playlist: Added confirmation dialog to “Sort by path” in playlist.
- Playlist: Added the ability to move multiple specified items in the playlist panel.
- Playlist: Playlist books are supported in “Current book only” in the Playlist panel.
- Playlist: Ctrl-click on the move button of a playlist item to move it to the end of the playlist.
- Playlist: “Open source file” added to context menu of playlist items only when playlist book is open.
- Script: Added setting to call OnBookLoaded.nvjs script when renaming a book. (Settings > Script)
- Script: Apply theme to Script Console.
- Script: Added setting to add SQLite access to scripts. (Settings > Script)
- Script: Added IsChecked flag for menus to command parameters of script commands.
- Script: Added “script:foobar.nvjs” in the command line startup script specification to allow specifying files in the scripts folder.
- Script: Event script for startup OnStartup.nvjs Supported.
- Script: Window state change event OnWindowStateChanged.nvjs Supported.
- Script: Added "@args" to script doc comment.
- Script: Added nv.Bookshelf.FolderTree to manipulate the Bookshelf tree view.
- Script: Added nv.Bookmark.FolderTree to manipulate the tree view of the Bookmark panel.
- Script: Added nv.ShowInputDialog(title, message, text).
- Script: Added nv.Bookshelf.Wait() to wait until bookshelf display is complete.
- Script: Added nv.Book.ViewPages[].Player to control video.
- Script: Added nv.SusiePluginCollection to manipulate Susie plugins.
- Script: Added nv.DestinationFolderCollection to manage move and copy destination folders.
- Script: Added nv.ExternalAppCollection to manage external app settings.
- Script: Added PageAccessor.Index for page index.
- Script:  Added CommandAccessor.Name.
- Script:  Added nv.Book.IsBookmarked.
- Script:  Added MoveToParent(), etc. to BookshelfPanelAccessor.
- Script:  Added file manipulation commands nv.FileCopy(), nv.FileMove(), and nv.FileDelete().
- Script:  Maintain the package information nv.Environment.
- Script:  Size, LastWriteTime, and CreationTime of books and pages are maintained.
- Script:  Added accessor nv.CurrentCommand for the currently executing command.
- Script:  Add nv.ScriptPath, the path of the currently running script file.

#### Changed

- System: Window is now activated by dropping a file.
- System: Activate at the start of the main window display.
- System: Moved some of the copy content settings from file copy command parameters to system settings. (Settings > General > Copy Contents Policy)
- System: Limit multiple launches only if the executable file paths are the same.
- System: Optimization of startup process.
- Book: End-of-page judgment is now performed even when moving a set number of pages. 
- Book: Loop notifications even for seamless loops. 
- Address bar: The path text is now selected when the address bar is selected.
- Address bar: When an image file path is entered in the address bar, it can now be opened as a book containing that page.
- MainView window: The page list panel has been restored to its original state as much as possible when returning from the main view window mode.
- Bookshelf: Reloading a book no longer changes the bookshelf selections.
- Bookshelf: Added the ability to delete multiple histories at once from the bookshelf.
- Playlist: When the current playlist is opened as a book, opening a playlist item will now page through the current book.
- Playlist: When registering a playlist book page to the playlist, the entity is now registered.
- Playlist: Enabled “Load subfolders” in playlist book.
- Information panel: If there are no Extras, “None” is displayed.
- Script: Change PageAccessor.Path to the entity path.
- Script: Changed the type of date/time values, such as LastWriteTime, from string to Date.

####  Fixed

- System: Fixed a bug that multiple launch restrictions may not work.
- System: Fixed a bug that history may not be merged when multiple startups are performed.
- System: Fixed a bug that videos are not included when decompressed in units of compressed files.
- System: Fixed garbled command line help.
- System: Fixed a bug that UI animations such as menus do not follow Windows settings.
- System: Countermeasure for a bug that may cause an error when selecting print.
- Book: Fixed page position bug in seamless loop.
- Book: Fixed a bug that could cause an error when moving a folder page during a seamless loop.
- Book: Fixed an issue where the loading display sometimes did not disappear when pages were split.
- Book: Fixed a bug that title text scale did not change after stretch change.
- Book: Fixed a bug in which specifying the start page by archive path sometimes did not work when “Expand for each directory” was selected.
- Panel: Fixed a bug in which single selection from multiple selections did not execute the selection process.
- Playlist: Fixed a behavior bug with the + button in the playlist panel.
- Playlist: Fixed a bug that could cause incorrect playlist item paths on drag-and-drop.
- Playlist: File names are no longer duplicated when registering video books.
- Playlist: Fixed bug when copying compressed file pages in playlist book.

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.
