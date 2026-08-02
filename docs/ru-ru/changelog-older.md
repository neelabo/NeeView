# Changelog (Before 39.0)

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
- Moved the data storage folder for the store app version from "NeeLaboratory\NeeView.a" to "NeeLaboratory-NeeView". To solve the problem that the data may not be deleted even if it is uninstalled.
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
- Effects: Added application magnification setting of "Scale threshold for Keep dot". (Options> Effect panel)
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
