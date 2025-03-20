# Changelog

## 43.0
(2025-03-20)

> [!WARNING] This is still a beta version.

#### Welcome Dialogue

- Minimal settings, such as the selection of command presets, are made at initial startup.
- This setting can be changed later.

####  Extension associations

- You can now manage extension associations on the “Explorer” page of the settings.
- The Explorer context menu registration in the installer version has also been changed to be done here.

> [!NOTE] This feature is not available in the Store App version.

#### Added

- Language: Korean added.
- System: Display a slideshow playing icon in the title.
- System: Symbolic link supported.
- System: File export from compressed files now also copies Zone.Identifier.
- System: Supports multiple file drops from compressed folders.
- System: Added command line option "--language" to set temporary language.
- System: Added command line option "--clear-registry" to clear registry settings.
- Command: “Cut (Ctrl+X)” command added.
- Command: “Toggle trimming” command added.
- Command: “Overwrite mode” setting added to “Save” command.
- Command: "Scroll + Next/Prev" and "N-type scroll" commands added "Horizontal/Vertical scroll" to the scroll type parameter.
- Theme: Added missing mouse-over theme color setting.
- Book: Page reset setting added when moving to the next book in a page move. (Settings > Move > Reset next book page)
- View: Mouse wheel scrolling implemented. (Settings > Mouse operation > Mouse wheel scroll)
- View: Auto scroll sensitivity setting added. (Settings > Mouse operation > Auto scroll sensitivity)
- View: Added setting of conditions for automatic rotation. (Settings > View operation > Auto rotate policy)
- View: Added "Auto" to "Center of scaling". The center point will be set to fit within the screen as much as possible. (Settings > View operation > Center of scaling)
- View: Added color setting for the page being loaded. (Settings > Book > Loading page color)
- Panel: Added the ability to select list items by input text. (Settings > Panels > Select item with input text)
- History: Added the ability to display date/time groups in the history panel.
- PageList: "Page number" added to page list name format
- Video: Added video playback speed setting.
- Video: "Default Subtitle" setting added. (Settings > Video > Default Subtitle)
- Video: Noted that libVLC support is for 3.x only.

#### Changed

- System: Changed to .NET 9 based.
- System: Delayed panel generation to speed up startup a bit.
- System: When multiple startups are used, the window state is not changed even if the configuration file is updated in one of them.
- System: Date and time are now inherited in file copying.
- System: Export data selection dialog now inherits the location to be opened.
- System: renaming cursor movement made the same as in Explorer.
- System: added configuration importer is invoked when .nvzip file is specified as a command line argument.
- Command: Apply copy policy setting for compressed files when "Copy to folder"
- Command: "Save" is not available for directories as is.
- Command: "Focus on main view” command is now switchable.
- Settings: Command names for scripts can be found in the Command Parameters dialog.
- Book: Tried to avoid reloading when deleting pages.
- Book: The current page is not restored to the top page when it is deleted.
- Book: History saved when page settings are changed.
- View: Adjusting focus when a book is opened with file drop. Try the bookshelf first, then the focus to the main view.
- View: Support for auto-hiding the autoscroll cursor.
- Filmstrip: Filmstrip allows multiple selections.
- Panels: Compressed file icons and media file icons added to folder icons in PageList and Playlist.
- PageList: File drop to PageList now opens book.

#### Fixed

- System: Flash reduction at start of window display.
- System: Pages can now be loaded even with invalid file dates and times.
- System: Fixed a bug that if the application is terminated while deleting a ZIP entry, it will not be deleted correctly.
- System: Fixed a bug that sometimes caused file names to change when exporting files from compressed files.
- System: Fixed a bug that could cause an error if the destination folder for images did not exist.
- System: Fixed a bug when registering a context menu for a command whose menu hierarchy changes depending on the command parameters.
- System: Reduces file path case differences.
- Command: Fixed a bug in which the "External app" command did not work when a number was specified.
- View: Fixed a bug that sometimes left the page title displayed.
- View: Fixed a bug that caused the vertical and horizontal wheel counters to reset each other.
- View: Fixed a bug in image inversion by mouse dragging.
- Panels: Fixed a bug that panels were sometimes not displayed in the window when trying to display them.
- Navigator: Fixed an issue where the drag position of the navigator thumbnail could shift.
- Script: Fixed a bug that video operations in scripts were not reflected when the video page was refreshed.


## 42.6
(2025-01-22)

#### Changed

- 7z.dll 24.09
- Language: Updated zh-Hans


## 42.5
(2025-01-12)

#### Fixed

- Bookshelf: Fixed a bug that compressed files may disappear from the list when they are edited.


## 42.4
(2024-12-23)

#### Fixed

- System: Fixed a bug that caused multi-boot to fail under certain conditions.


## 42.3
(2024-12-22)

#### Fixed

- System: Fixed a bug that caused startup to fail with old language settings.

#### Changed

- Language: Updated zh-Hans.


## 42.2
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


## 42.1
(2024-12-01)

#### Fixed

- System: Fixed a bug in version check.
- Script: Fixed a bug about CreationTime. 


## 42.0
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




## 41.3
(2024-07-14)

#### Changed

- .NET 8.0.7
- 7z.dll ver 24.07

#### Fixed

- Fixed a bug that sometimes caused an error when switching page filters.
- Fixed a bug that caused a huge popup with a dummy icon in the page list.
- Fixed a bug that sometimes caused an error when switching books during animation frame generation.


## 41.2
(2024-05-31)

#### Changed

- Update 7z.dll to ver 24.06

#### Fixed

- Fixed a bug that the focus was not set correctly when switching lists in the thumbnail layout. 
- Fixed a problem updating the search filter in the PageList.


## 41.1
(2024-05-18)

#### Changed

- Update various libraries

#### Fixed

- Fixed a bug that prevented seamless loops from working.
- Fixed an issue where automatic background color setting is not reflected when a book is opened.


## 41.0
(2024-05-10)

#### Added

- Added Italian language.
- Direct editing of language resources. (/Languages/*.restext) 
- Auto scrolling is implemented. Long press also supported. By default, the wheel button toggles between modes.
- Added a new command parameter "In panorama mode, all pages are considered as one page" to the N-type scrolling command.
- Added book move priority setting. (Settings > Move > Book movement priority)
- Add ability to switch display when page is ready. Suppresses temporary display on page switching. (Settings > Move > Ready to page move)
- Added the ability to drop the bookshelf location icon and the information icon in the address bar to other apps

#### Changed

- ZIP version places DLL files in the Libraries folder.
- The current view is maintained as much as possible when switching books.
- Asynchronous pre-decompression of solid compressed archives
- Various library updates

#### Fixed

- Fixed a bug where the mouse button would sometimes enter long-press mode even when it was released.
- Fixed a bug in file manipulation of network folder search results in the bookshelf.
- Fixed a bug where an incorrect page was sometimes created when a playlist was opened as a book.
- Fixed a bug that sometimes caused incorrect behavior in bookshelf range selection. 
- Fixed a bug that prevented the Susie Plug-in all ON/OFF settings from working properly.
- Fixed incorrect panel display status flag in menus. 
- Reduced the problem of book thumbnails not being generated when files are added.
- Fixed a bug that window dragging did not work when a book was closed.
- Fixed a bug that shortcut archives were not recognized as pages.
- Fixed an issue where the app would sometimes crash when creating a bookmark folder in the folder tree.



## 40.8
(2024-05-01)

#### Security

- Updated to .NET 8.0.4. For more information on this vulnerability, please visit [.NET Blog](https://devblogs.microsoft.com/dotnet/april-2024-updates/).
- Change explorer path to absolute path.


## 40.7
(2024-02-10)

#### Fixed

- Copy command parameters are now reflected in copying page lists, etc. only for text settings. Fixed the same when dragging.


## 40.6
(2024-02-09)

#### Changed

- Language files pt-BR, zh-Hans updated.
- "Play/Stop" command now works for video pages and animated images.

#### Fixed

- Corrected timing of address bar button updates.
- Fixed a bug that search history may not be saved.
- Fixed a bug that the "Apply image resolution information" setting did not work.
- Fixed a problem in which the parameters of the copy command were not reflected when copying a page list, etc.
- Fixed thumbnail bug in file renaming.
- Error sometimes occurring when deleting bookmarks from search results fixed.
- Fixed a bug that page history was not functioning properly.
- Fixed a bug that the playlist registered flag in the context menu was not displayed correctly.
- Fixed a bug that caused an error when enabling loupe when no book is open.
- Fixed a bug that caused the loupe to stop functioning when another book was opened with the loupe open.


## 40.5
(2024-01-12)

#### New

- Added "Stretch Tracking" toggle button to the Scale section of the Navigator panel. This function corrects the scale according to the stretch mode for images that are rotated, etc.
- Added "Stretch" command to apply stretching to scale.

#### Changed

- Changed the display start position setting to selective. Added "Direction dependent, top". (Settings > View operation > Display start position)
- Script: nv.Config.View.IsViewStartPositionCenter is obsolete. Use nv.Config.View.ViewOrigin instead.
- Set the default for the "Toggle page mode" command to loop. The default for the mouse gesture to set the page mode is now a dedicated command and behaves the same as before.
- Switching is now performed when the same stretch mode is specified in the stretch mode specification command, so that the behavior is the same as before.
- Changed scale calculation method for "Auto stretch window" to be more natural.

#### Fixed

- Fixed a bug that the stretch mode was not applied to the stretch apply button in the navigator.
- Fixed a bug that sometimes prevented alphabetic word searches.
- Reduced frame dropping in VLC video.
- Fixed a bug that sometimes caused application errors with VLC videos.
- Fixed a bug that rotation information was not reflected correctly in VLC videos.
- Fixed a bug that disabled track designation when switching repeat settings in VLC video.
- Fixed a bug in VLC video where media with audio information only was sometimes not determined to be audio.


## 40.4
(2023-12-22)

#### New

- Added encoding setting for ZIP files when the UTF-8 flag is not set.

#### Fixed

- ZIP files now load in UTF-8 when the UTF-8 flag is set.
- Fixed a bug in which the file exclusion attribute differs between bookshelf and bookshelf search.
- Fixed a bug where changes were not applied even if the book was reopened after changing archive settings.
- Fixed a bug where QuickAccess property changes were sometimes not saved.
- Fixed a bug where the keyboard focus was not following the change of selected items on the bookshelf.
- Reduces flash when switching books


## 40.3
(2023-12-16)

#### New

- Add "Added dummy page to the first/last page" settings.

#### Fixed

- Fixed a bug that caused an error in copying.
- Fixed a bug that "Start loupe at standard magnification" of loupe did not work.
- Fixed an issue with page slider movement in two-page display that caused misalignment when moving from a single page.
- Corrected the number of pages displayed by the "Last Page" command in the two-page display.
- Fixed a bug in seamless loop in two-page display where "First/Last page alone" did not work.
- Adjust button widths on the settings page.
- Fixed an issue with the number of pages displayed when returning to the previous book in an end-of-page book move.
- Fixed a bug that sometimes moved two pages when moving one page.


## 40.2
(2023-12-12)

#### New

- Added Portuguese (pt-BR)

#### Fixed

- Fixed a bug that sometimes only one page is displayed even though the display mode is 2-page display mode.
- Fixed a bug that film centering did not work immediately after the start of filmstrip display.
- Fixed a bug that history may not be saved.
- Fixed a bug that the loupe did not work immediately after switching to panorama mode.
- Fixed a bug that loupe release on page move did not work.
- Fixed a bug that prevented searching by network path in the bookshelf.
- Fixed a bug in which the sub archive loading failure skip process did not work.
- Susie plugin Improved access timeout handling.


## 40.1
(2023-12-06)

#### Fixed

- Fixed a bug that data could not be read if the drag operation "Move (scale dependent)" parameter was specified in the previous version.
- Fixed a bug that sometimes only one page is displayed even though the display mode is 2-page display mode.
- Fixed a bug in which exclusion patterns were not applied in bookshelf searches.
- Corrected a typo in the help.


## 40.0
(2023-12-05)

#### Important

- Windows 10, Windows 11, and 64-bit operating systems only; packages for Windows 7, Windows 8.1, and 32-bit operating systems are not provided after version 40.0.
- It runs on .NET8. This framework is included as part of the application. There is no need to install it separately.
- For environments that already have .NET8 installed, we have prepared a package "NeeView40-fd.zip" that does not include. .NET8 runtime for x86 must also be installed when using the Susie plugin.
- The ZIP version cannot be overwritten and updated because the file structure has changed significantly. Please use Export/Import to migrate your data. The installer version can be updated as is.

#### User data storage location

- The user data storage location for the ZIP version has been consolidated into the "Profile" folder.
- The default storage location for user data can be specified with the NEEVIEW_PROFILE environment variable.

#### Revamped page display code

The page display program has been reworked to accommodate the implementation of panorama mode. We have tried to match the previous behavior as much as possible, but there is a possibility that the behavior when changing the scale or rotation may have changed.

- Added "Panorama" mode, which connects pages together.
- The page connection direction can be switched between portrait and landscape.
- Scroll time settings for various scroll commands are combined into one. (Settings > View operation > Scroll time)
- Enabled to switch pages with scrolling animation. (Settings > View operation > Page move time)

#### MainView window

The MainView window now functions as a new viewing mode, not just a windowed display.

- Assigned the F12 shortcut to the MainView window switching command.
- Added setting to display page list in main area when MainView window is displayed. (Settings > Main view)
- Added auto-hide setting for MainView window. (Settings > Main view)
- Changed behavior so that the MainView window is minimized with the close button.
- Minimize when ESC key is pressed in the MainView window.
- Added "Auto stretch window" to the main view window. (MainView window > Title bar context menu)
- Added a setting to the page list to move the focus to the main view when a page is selected. (PageList panel > Detail Menu)

#### Enhanced search 

- Search box added to various panels.
- Added search option. You can now search by tags contained in bookmarks and images. See "Search options help" for details.
- Individual deletion button of search history added.
- Added search history size setting. (Settings > General)

#### Enhanced video play

- Added setting to use libVLC (VLC media player) for video play. (Settings > Video)
- Added setting to display videos as pages. (Settings > Video)
- Added a control bar to the Navigator panel for control of videos displayed as pages. (Navigator panel > Detail Menu)
- Use AnimatedImage for GIF animation.
- Supports PNG animation.

#### Base scale

- Added base scale change command and mouse drag operation.
- Base scale values are now stored in book units.

#### Auto rotate

- Added forced left rotate and forced right rotate to Auto rotate.
- Auto rotate settings are now saved per book.

#### Other

- Many bug fixes.
- App icon change.
- Changed window maximization correction process. The window frame width setting when maximized has been abolished.
- Add "$FullPath" to the window title keyword.
- Added a setting to swap left and right commands by slider direction using the tilt wheel operation.
- The SusiePlugin folder can now be specified by relative path.
- Faster operation in panel thumbnail view.
- Added setting to limit panel width to within the window. (Settings > Panels)
- Panels can now be connected horizontally; the second and subsequent panels can only be connected in the current orientation.
- The date/time format of the panel is culture-dependent. This format is configurable. (Settings > General)
- panels, etc., to accept mouse commands when possible.
- The natural order was made closer to the Explorer's order.
- Additional text embedded in PNGs can now be displayed in the "Extras" group of the Information panel.
- History is now saved automatically.
- History update date and time are now displayed in the contents view of the history list.
- Implemented file renaming in the PageList.
- Supports deleting files in ZIP. To make it work, enable ZIP file editability in the settings.
- Added original size and keep dot settings to image output.
- Added "Seamless loop" for end-of-page behavior.
- Improved accuracy of slideshow timer.
- Added timer display to slideshow. (Settings > Slideshow)
- The target of the "Grid" can now be selected between the image and the screen.
- Redesigned book page. Images are now displayed as they are instead of thumbnails.
- The text element when copying a file is now basically absent.
- Playlist items can also be used to copy files.
- Script: Levels were introduced for compatibility. Changes that do not affect the operation, such as parameter name changes, are now only notified in the console.
- Script: nv.Playlist.Name Add. The name of the current playlist can be changed by assigning.
- Scrpit: Added GetMetaValue method to PageAccessor. You can get the meta information of an image.




## 39.5
(2022-08-11)

#### Fixed

- Restored the SQLite library to the previous one to reduce the error phenomenon when closing the application.
- Fixed a bug that read-only shortcuts could not be processed.
- Fixed a bug where loading a book with subfolders could not be canceled.
- Language file update (zh-TW).


## 39.4
(2022-07-04)

#### New
- Supports Windows11 snap layouts

#### Fixed
- Fixed a bug that the bookshelf exclusion filter does not work when adding files
- Fixed a bug that caused the thumbnail operation to become very slow due to certain operations.
- Fixed a bug that the coordinates of the image shift when returning from minimization to full screen.
- Fixed a bug that folder thumbnails are not updated.
- Fixed a bug that the "Move focus to search box" command may not be focused.
- Fixed a bug that the "Load subfolders at this location" setting does not work when opening previous or next workbooks.
- Fixed a bug when cruising from a book loading a subfolder.
- Fixed a bug when an invalid path is passed to the path specification dialog.
- Fixed a bug that shortcut files are not recognized when opening playlists as books.
- Fixed a bug when dragging and dropping shortcuts for multiple archive files.
- Fixed a bug that the shortcut of UNICODE name could not be recognized.
- Fixed a bug that may not be reflected even if deleted from the history list.
- Fixed a bug that an error occurs in the "Prev(Next) playlist item" command when the playlist is "Current book only".
- Fixed the problem that the brightness may change when applying the resize filter.
- Script: Fixed a bug where the effects of Patch() would continue to remain.
- Script: Fixed an issue with large arrays.
- Correcting typographical errors.

#### Changed
- Libraries update.
- Language file update (zh-TW).


## 39.3
(2021-07-17)

####  New

- Language: Supports 中文(中国)

#### Fixed

- Fixed a bug that the taskbar is displayed when returning from minimization to full screen
- Improved the problem that the taskbar is not displayed when the window is maximized when the taskbar is set to be hidden automatically.
- Fixed a bug that an error occurs in the "Prev/Next History" command.
- Fixed a bug where you couldn't rename in a floating panel
- Fixed initial selection bug when renaming Quick Access
- Fixed a bug that shortcut keys are not displayed in the context menu of the folder tree
- Fixed a bug that theme loading fails when the app is placed in the path containing "#"

#### Changed

- Added "Text copy" setting to "Copy file" command parameter. Select the type of text that will be copied to the clipboard.


## 39.2
(2021-06-26)

#### Fixed

- Fixed the main menu not to take focus.
- Fixed a bug where the layout of the startup help dialog was broken.
- Fixed tilt wheel operation to be one command. (Settings > Command > Limit tilt wheel operation to one time)


## 39.1
(2021-06-20)

#### Fixed

- Fixed a bug that the scroll type changes to "Diagonal scroll" when the parameter of "Scroll + Prev" command is set.
- Fixed a bug that could cause blurring when applying the resize filter.
- Fixed a bug where the README file could not be opened when the application was placed in a path containing multibyte characters or spaces.


## 39.0
(2021-06-18)

#### Important: Integrate Pagemark into Playlist

- Pagemark have been abolished. The previous pagemarks will be carried over as a playlist named "Pagemark".
- A new playlist panel has been added.
- You can create multiple playlists and switch between them. You can treat the selected playlist like a Pagemark.
- The playlists managed in the Playlist panel are limited to those placed in a dedicated folder, but existing playlist files can still be used.
- In the page mark, it was grouped by book, but in the playlist, it is grouped by folder or compressed file.

#### Important: Renewal of appearance

- Almost all UI controls have been tuned.
- We increased the theme. The theme color setting in the menu section has been abolished. (Settings > Window > Theme)
- It is now possible to freely color by creating a custom theme. See [here](https://neelabo.github.io/NeeView/en-us/theme.html) for the theme file format.
- Themes are now applied to the settings window as well.
- The font settings have been totally revised. (Settings > Fonts)

#### Important: Information panel renewal

- Changed to display a lot of EXIF information.
- Enabled to switch the display information when displaying 2 pages.

#### New

- Language: Compatible with Chinese(Taiwan). (Thanks to the provider!)
- Setting: Added settings for the web browser and text editor to be used. (Settings > General)
- Setting: Add scripts and custom themes to your export data.
- Command: The command can be cloned. Right-click the command in the command list of settings and select "Clone" to create it. Only commands with parameters can be cloned.
- Command: Added "Delete invalid history items".
- Command: Tilt wheel compatible.
- MainView: Hover scroll. (Menu > Image > Hover scroll)
- MainView: Added view margin settings. (Settings > Window > Main view margin)
- MainView: Corresponds to the loupe by pressing and holding the touch.
- QuickAccess: Enabled to change the name. You can also change the reference path from the quick access properties.
- Navigator: Added display area thumbnails. (Detailed menu in the navigator panel)
- Navigator: Added settings to maintain rotation expansion and contraction even when the book is changed. Change from the context menu of the pushpin button in the navigator panel.
- PageSlider: Added slider display ON / OFF command. (Menu > View > Slider)
- PageSlider: Added playlist registration mark display ON / OFF setting for slider. (Setting > Slider)
- Filmstrip: Display the playlist registration mark. (Setting > Filmstrip)
- Filmstrip: Implemented context menu on filmstrip.
- Script: Added error level setting. (Setting > Script > Obsolete member access error level)
- Script: Changed to monitor changes in the script folder.
- Script: Added script command argument nv.Args[]. Specify in the command parameter of the script command.
- Script: Added page switching event OnPageChanged.
- Script: Added instruction nv.Book.Wait() to wait for page loading to complete.
- Script: Added nv.Environment
- Develop: We have prepared a multilingual development environment. See [here](https://github.com/neelabo/NeeView/tree/master/NeeView/Languages) for more information.

#### Fixed

- Setting: Fixed a bug that data is incorrect when using a semicolon in the extension setting.
- Setting: Fixed a bug that the initialization button of the extension setting does not work.
- Setting: Fixed a bug that the list box disappears after searching for settings.
- Other: Fixed a bug that page recording is not working.
- Window: Fixed a bug that thumbnail images pop up in rare cases.
- Window: Fixed a bug that the panel may also be hidden when the context menu is closed.
- Window: Fixed a bug that the display size of certain pop-up thumbnails is incorrect.
- Window: Fixed multiple selection behavior of list.
- MainView: Fixed a bug that the aspect ratio may be incorrect when rotating the RAW camera image.
- Bookshelf: Fixed a bug that the mark indicating the current book may not be displayed.
- ScriptConsole: Fixed a bug that the application terminates abnormally with "exit".
- Script: Fixed a bug that the image size was the value after the limit.
- Script: Fixed a bug that the Enter key input of ShowInputDialog affects the main window.
- Script: Enabled to get the path with the default path setting.

#### Changed

- Setting: The file operation permission in the initial state has been turned off. (Menu > Option > File operation)
- Network: When the network access permission setting is OFF, when connecting to the Internet with a Web browser, a confirmation dialog is displayed instead of being invalid.
- Command: Added command parameters to change N-type scroll to Z-type scroll.
- Command: Added a stop parameter for line breaks to the N-type scroll command.
- Command: Added working directory settings for external apps.
- Command: Added a mode to open from the left page when opening multiple pages with an external application.
- Command: Added command parameters to import and export commands.
- Book: Added registration order in page order. Only works for playlists. Otherwise it works as a name order.
- Window: Added automatic display judgment setting for the overlapping part of the side panel and menus and sliders. (Settings > Panels)
- Window: The area width of the automatic display judgment is divided into the vertical direction and the horizontal direction. (Settings > Panels)
- Window: The tab movement of the entire main window has been adjusted from the upper left to the lower right.
- MainView: Changed to process non-animated GIF as an image.
- MainView: Added parameters to mouse drag operation. (Settings > Mouse operation)
- Bookshelf: A search path is also valid for "Home Location".
- PageList: Changed to open the current book as a selection page by moving the parent.
- Effect: Expanded custom size function.
- PageSlider: Added thickness setting. (Settings > Slider)
- PageSlider: Changed the playlist registration mark display design.
- Script: Changed to create folders and samples when first opening the script folder.

#### Removed

- Command: Removed "Toggle title bar" command.
- Panels: Supplemental text opacity setting is abolished. Can be set with a custom theme.
- Bookshelf: Removed "Save playlist" from the details menu.
- Filmstrip: Abolished the "Display background" setting. Linked to the opacity of the page slider.
- Script: Some members have been deleted. See "Obsolete members" in Script Help for more information.




## 38.3
(2021-01-29)

#### Fixed

- Fixed a bug in PDF rendering resolution.
- Fixed a bug related to window coordinate restoration when the taskbar is on the top or left.
- Fixed a bug that scrolling may be unnatural when multiple items are selected on a bookshelf, etc.
- Fixed a bug that "Stretch window" may not work properly.
- Script: Fixed a bug that the focus when selecting a panel list item may not match the keyboard focus.
- Script: Fixed a bug that the Enter key input of the ShowInputDialog command affects the main window.
- Script console: Fixed a bug that the application terminates illegally with the exit command.
- Script console: Fixed a bug that the application may terminate abnormally in the object display.

#### Changed

- "Stretch window" changed to work only when the window state is normal.
- Script console: Changed to omit nested properties in object display.


## 38.2
(2021-01-18)

#### Fixed

- Fixed a bug that the DPI of the display may not be applied.
- Fixed a bug that dots may be enlarged as they are when the scale is changed in the navigator.
- Fixed a bug that videos could not be played when switching the main view window.
- Fixed a bug that the taskbar is displayed in full screen mode when in tablet mode.
- Fixed a bug that the placement save setting of AeroSnap is not working.
- Fixed a memory leak in a subwindow.
- Corrected the text of the command initialization dialog.


## 38.1
(2021-01-08)

#### Fixed

- Fixed a bug related to the state of the window at startup.


## 38.0
(2021-01-01)

#### New

- Docking side panel support. You can drag the panels to connect them.
- Floating side panel support. Right-click the panel icon or panel title and execute "Floating" to make the panel a subwindow.
- Main view window implementation. Makes the main view a separate window. (View > MainView window)
- Added "Window size adjustment" command to match the window size with the display content size.
- Added auto-hide mode setting. You can enable automatic hiding even in window states other than full screen. (Options > Panels > Auto-hide mode)
- Added AeroSnap coordinate restore settings. (Options > Launch > Restore AeroSnap window placement)
- Added slider movement setting according to the number of displayed pages. (Options > Page slider > Synchronize the...)
- Added ON / OFF setting for WIC information acquisition. (Options > File types > Use WIC information)

#### Fixed

- Fixed an issue that caused an error when trying to open the print dialog on some printers.
- Fixed a bug that may not start depending on the state of WIC.
- Fixed a bug that you cannot start if you delete all excluded folder settings.
- Fixed a bug that thumbnails are not updated when changing the history style.
- Fixed a bug that the display may not match the film strip.
- Fixed a bug that the shortcut of the main menu may not be displayed.
- Fixed a bug that folders in the archive could not be opened with the network path.
- Fixed a bug that bookmark update may not be completed when importing settings.
- Fixed a bug related to page spacing setting and stretching application by rotation.
- Fixed a bug related to scale value continuation when page is moved after rotation.
- Fixed a bug that playback cannot be performed if "#" is included in the video path.
- Fixed a page movement bug when splitting horizontally long pages.
- Suppresses the phenomenon that the page advances when the book page is opened by double-clicking.
- Improved the problem that media without video such as MP3 may not be played.
- Fixed shortcut key name.

#### Changed

- Transparent side panel grip.
- Disable IME except for text boxes.
- Backup file generation is limited to once per startup.
- Moved the data storage folder for the store app version from "NeeLaboratory\NeeView.a" to "NeeLavoratory-NeeView". To solve the problem that the data may not be deleted even if it is uninstalled.
- To solve the problem that the upper folder of the opened file cannot be changed, the current directory is always in the same location as the exe.
- Changed the order of kanji in natural sort to read aloud.
- Changed to generate a default script folder only when scripts are enabled. If a non-default folder is specified, it will not be generated.
- Added a detailed message to the setting loading error dialog and added an application exit button.
- Changed the NeeView switching order to the startup order.
- Added the option to initialize the last page in "Page Position" of the page settings.
- Adjust the order of the "View" menu.
- Changed "File Information" to "Information".
- Various library updates.

#### Removed

- Abolished the setting "Do not cover the taskbar area at full screen". Substitute in auto-hide mode.
- "Place page list on bookshelf" setting abolished. Substitute with a docking panel.

#### Script

- Fixed: Fixed a bug that command parameter changes were not saved.
- Fixed: Fixed a bug that the focus did not move with "nv.Command.ToggleVisible*.Execute(true)".
- Fixed: Fixed a bug that the focus did not move to the bookshelf in the startup script.
- New: The default shortcut can be specified in the doc comments of the script file.
- New: Added nv.ShowInputDialog() instruction. This is a character string input dialog.
- New: Added sleep() instruction. Stops script processing for the specified time.
- New: Added "Cancel script" command. Stops the operation of scripts that use sleep.
- New: Addition of each panel accessor such as nv.Bookshelf. Added accessors for each panel such as bookshelves. You can get and set selection items.
- Changed: Changed to output the contents of the object in the script console output.
- Changed: Changed nv.Book page accessor acquisition from method to property.
    - nv.Book.Page(int) -> nv.Book.Pages\[int\] (The index will start at 0)
    - nv.Book.ViewPage(int) -> nv.Book.ViewPages\[int\]
    - Pages[] cannot get the page size(Width,Height). You can get it in ViewPages[].
- nv.Config
    - New: nv.Config.Image.Standard.UseWicInformation
    - New: nv.Config.MainView.IsFloating
    - New: nv.Config.MainView.IsHideTitleBar
    - New: nv.Config.MainView.IsTopmost
    - New: nv.Config.MenuBar.IsHideMenuInAutoHideMode
    - New: nv.Config.Slider.IsSyncPageMode
    - New: nv.Config.System.IsInputMethodEnabled
    - New: nv.Config.Window.IsAutoHideInFullScreen
    - New: nv.Config.Window.IsAutoHideInNormal
    - New: nv.Config.Window.IsAutoHidInMaximized
    - New: nv.Config.Window.IsRestoreAeroSnapPlacement
    - Changed: nv.Config.Bookmark.IsSelected → nv.Bookmark.IsSelected
    - Changed: nv.Config.Bookmark.IsVisible → nv.Bookmark.IsVisible
    - Changed: nv.Config.Bookshelf.IsSelected → nv.Bookshelf.IsSelected
    - Changed: nv.Config.Bookshelf.IsVisible → nv.Bookshelf.IsVisible
    - Changed: nv.Config.Effect.IsSelected → nv.Effect.IsSelected
    - Changed: nv.Config.Effect.IsVisible → nv.Effect.IsVisible
    - Changed: nv.Config.History.IsSelected → nv.History.IsSelected
    - Changed: nv.Config.History.IsVisible → nv.History.IsVisible
    - Changed: nv.Config.Information.IsSelected → nv.Information.IsSelected
    - Changed: nv.Config.Information.IsVisible → nv.Information.IsVisible
    - Changed: nv.Config.PageList.IsSelected → nv.PageList.IsSelected
    - Changed: nv.Config.PageList.IsVisible → nv.PageList.IsVisible
    - Changed: nv.Config.Pagemark.IsSelected → nv.Pagemark.IsSelected
    - Changed: nv.Config.Pagemark.IsVisible → nv.Pagemark.Visible
    - Changed: nv.Config.Panels.IsHidePanelInFullscreen → nv.Config.Panels.IsHidePanelInAutoHideMode
    - Changed: nv.Config.Slider.IsHidePageSliderInFullscreen → nv.Config.Slider.IsHidePageSliderInAutoHideMode
    - Removed: nv.Config.Bookshelf.IsPageListDocked → x
    - Removed: nv.Config.Bookshelf.IsPageListVisible → x
    - Removed: nv.Config.Window.IsFullScreenWithTaskBar → x
- nv.Command
    - New: ToggleMainViewFloating
    - New: StretchWindow
    - New: CancelScript
    - Changed: FocusPrevAppCommand → FocusPrevApp
    - Changed: FocusNextAppCommand → FocusNextApp
    - Changed: TogglePermitFileCommand → TogglePermitFile
    - Removed: TogglePageListPlacement → x


## 37.1
(2020-06-08)

#### Changed

- When changing the stretch, the stretch is applied without changing the current angle.

#### Fixed

- Fixed a bug that an incorrect setting file may be output depending on the combination of system region and language.
- Fixed a bug that the file deletion confirmation setting did not work.
- Fixed a bug that the same stretch is not applied when the same stretch is selected from the menu or command.
- Fixed a bug that could not read compressed files that contained folders with names like "x.zip".


## 37.0 
(2020-05-29) 

#### Important

- Separated the packages into x86 and x64 versions
    - Usually use the x64 version. Use the x86 version only if your OS is 32-bit.
    - We strongly recommend that you install the installer version after uninstalling the previous version.
        - The x86 version and the x64 version are treated as separate apps, and although it makes no sense, they can be installed at the same time. The x86 version overwrites the previous version.
    - Both versions support only the 32-bit Susie plugin (.spi).

- .NET framework 4.8
    - Changed the supported framework to .NET framework 4.8 . [If it doesn't start, please install ".NET Framework 4.8 Runtime" from here.](https://dotnet.microsoft.com/download/dotnet-framework/net48)

- Change configuration file format
    - Changed the structure of settings and changed the format to JSON. The existing XML format setting file can also be read, and automatically converted to JSON format.
    - Backward compatibility of configuration files will be maintained for about a year. In other words, the version around the summer of 2021 will not be able to read the old XML format. The same applies to the exported setting data.

#### New

- Faster booting: Booting will be faster than previous versions, including the ZIP version.
- Navigator: Newly added navigator panel for image manipulation such as rotation and scale change.
- Navigator: Added "Base Scale" setting. The stretch applied size is further corrected.
- Navigator: Moved settings such as "Keep rotation even if you change the page" to Navigator panel.
- Navigator: When the automatic rotation setting and the keep angle are turned on, the rotation direction is forced when the book is opened.
- Script: You can now extend commands with JavaScript. See the script manual for details. (Help> Script Help)
- Script: It is disabled by default and must be enabled in settings to use it. (Options> Script)
- Command: Added "Save settings".
- Command: Added "Go back view page" and "Go next view page". Follows the internal history of each page.
- Command: Add the keyword "$NeeView" to start NeeView itself in the program path of "External app" command parameter.
- Command: Add "Random page".
- Command: Add "Random book".
- Command: "Switch prev NeeView" and "Switch next NeeView" added. Switch NeeView at multiple startup.
- Command: "Save as" The folder registered in the image save dialog can be selected.
- Command: Added the command "N-type scroll ↑" and "N-type scroll ↓" for display operation only for N-type scroll.
- Command: Add scroll time setting to command parameter of scroll type.
- System: Added setting to apply natural order to sort by name. (Options> General> Natural sort)
- System: Added a setting to disable the mouse operation when the window is activated by clicking the mouse. (Options> Window> Disable mouse data when..)
- Panel item: Added setting to open book by double click. (Options> Panels> Double click to open book)
- Panel item: Enabled to select multiple items.
- Panel item: Added popup setting for thumbnail display or banner display of list. (Options> Panel list item> *> Icon popup)
- Panel item: Added the wheel scroll speed magnification setting in the thumbnail display of the list. (Options> Panel list item> > Mouse wheel scroll speed rate in thumbnail view)
- Thumbnail: Added image resolution setting. (Options> Thumbnail> Thumbnail image resolution)
- Bookshelf: Added an orange mark indicating the currently open book.
- Bookshelf: Added setting to display the current number of items. (Options> Bookshelf> Show number of items)
- Bookshelf: Delete shortcut to move to upper folder with Back key.
- Bookshelf: Added setting to sort without distinguishing item types. (Options> Bookshelf> Sort without file type)
- Bookshelf: Default order setting "Standard default order", "Default order of playlists", "Default order of bookmarks" added. (Options> Bookshelf)
- Bookshelf, PageList: "Open in external app" is added to the context menu.
- Bookshelf, PageList: "Copy to folder" is added to the context menu.
- Bookshelf, PageList: Added "Move to folder" to context menu. Enabled to move files. Effective only when file operation is permitted.
- Bookshelf, PageList: Add move attribute to right button drag. You can move files by dropping them in Explorer. Effective only when file operation is permitted.
- PageList: Add move button.
- PageList, PagemarkList: Image files can be dragged to the outside.
- History: Added a setting to display only the history of the current book location. (HistoryPanel menu> Current folder only)
- History: Added a setting to automatically delete invalid history at startup. (Options> History> Delete invalid history automatically)
- Effects: Trimming settings added to the effects panel.
- Effects: Added application magnification setting of "Scake threshold for Keep dot". (Options> Effect panel)
- Loupe: Added setting to center the start cursor on the screen. (Options> Loupe> At the start, move the cursor position to the screen center)
- Book: Pre-reading at the end of the book is also performed in the reverse direction of page feed.
- Book: Added "Select in dialog" to end page behavior. (Options> Move> Behavior when trying to move past the end of the page)
- Book: Added setting to display dummy page at the end of page when displaying 2 pages. (Options> Book> Insert a dmmy page)
- Book: Added a notification setting when the book cannot be opened. (Options> Notification> Show message when there are no pages in the book)
- Book: Added setting to reset page when shuffled. (Options> Book> Reset page when shuffle)
- Image drag operation: "Select area to enlarge" is added. (Options> Mouse operation> Drag action settings)
- Image drag operation: A mouse drag operation "Scaling (horizontal slide, centered)" that moves to the center of the screen at the same time as enlargement is added. (Options> Mouse operation> Drag action settings)
- Startup option: Added option "\-\-script" to execute script at startup.
- Startup option: Added option "\-\-window" to specify window status.
- Options: Add search box.
- Options: Search box added to the list of command settings.
- Options: added the SVG extension. (Options> File types> SVG file extensions)
- Options: "All enable / disable" button added to Susie plugin settings.

#### Changed

- Command: Change shortcut "Back", "Shift+Back" to page history operation command.
- Command: Improve the behavior of N-type scroll of "Scroll + Prev" and "Scroll + Next" command. Equalized transfer rate.
- Command: "Scroll + Prev" and "Scroll + Next" command parameter "Page move margin (sec)" is added. In addition, the "scroll stop" flag is abolished.
- System: Change delete confirmation dialog behavior of Explorer. Show only the dialog when you don't put it in the trash.
- System: Change the upper limit of internal history of bookshelves etc. to 100.
- Display: The display image position is not adjusted when the window size changes.
- Display: Don't hide the image when the caption is permanently shown in the main view.
- Panels: The left and right key operations in the panel list are disabled by default. (Options> Panels> The left and right keys input of the side panel is valid)
- Bookshelf: Change layout of search box. Search settings moved to the menu on the bookshelf panel.
- Thumbnail: Change the cache format. The previous cache is discarded.
- Thumbnail: Change cache save location from folder path to file path.
- Thumbnail: The expiration date of the cache can be set. (Options> Thumbnail> Thumbnail cache retention period)
- Thumbnail: When settings such as thumbnail image resolution are changed, cache with different settings is automatically regenerated.
- Book: added a check pattern to the background of transparent images. (Options> File types> Check background of transparent image)
- Book: The extension of standard image files can be edited. (Options> File types> Image file extensions)
- Book page: Change the operation feeling. Gestures are enabled on the book page image, and the book can be opened by double touch.
- Book page: Layout change. Removed the folder icon display.
- Book page: Added image size setting. (Options> Book> Book page image size)
- Susie: Enabled to access "C:\\Windows\\System32" by Susie plug-in in 64bitOS environment.
- Startup option: Removed the full screen option "-f".
- Options: Merged page display settings with book settings, moved dip-related settings to image format settings.
- Options: Reorganization of settings page. "Visual" items have been reorganized into groups such as "Window" and "Panels".
- Options: Abolished the external application setting, changed to specify with the command parameter of the "External app" command. Deleted protocol setting and delimiter setting.
- Options: Removed clipboard setting and changed to specify with command parameter of "Copy file" command. Removed delimiter setting.
- Others: "Settings" is changed to "Options".
- Others: The Esc key in the search box, address bar, etc. is accepted as a command shortcut.
- Others: Various library updates.
- Others: Minor layout correction.

#### Fixed

- Fixed a bug that may crash when thumbnail image creation fails.
- Fixed a bug that crashes when searching playlists. The playlist does not support search, so it was disabled.
- Fixed a bug that the book itself cannot be opened if there is a compressed file that cannot be opened when opening the book including subfolders. Only the file is skipped.
- Fixed a bug that the rename of compressed files may fail.
- Fixed a bug that the image position is changed by returning from window minimization.
- Other minor bug fixes.

> [!NOTE]
> There is no English version of Changelog prior to 37.0.
