# Q&A

<style>
  h3 { border-bottom: 1px dashed #CCC; }
</style>

> [!NOTE]  
> Currently identified problems can be found at [Issues](https://github.com/neelabo/NeeView/issues).
> Please post your requests and bugs here as issues.

> [!NOTE]  
> Questions about usage should be posted in the Q&A section of [Discussions](https://github.com/neelabo/NeeView/discussions).
 
### Q. Slow startup

A. .NET, so the initial startup tends to be slow.  
 
### Q. I can't get it to start.

A1. Please restart your PC and try again. 

This is because if the previous process remains for some reason, it may not be able to start due to multiple startup restrictions. This can be resolved by terminating the process in Task Manager or restarting the PC.

A2. Try deleting the user settings file and then launching the application. 

If you delete the user settings file “UserSetting.json” before starting the program, it will start with the default settings.
In the ZIP version, the user settings file is located in the “Profile” folder.

### Q. I want to start multiple applications at the same time.

A1. Please turn ON “Allow multiple activations in the “Launch” page of the settings.

A2. If you want to temporarily start more than one NeeView, hold down the SHIFT key to start a new NeeView.

### Q. I want to use less memory.

A. Since the most memory-intensive data is the image data that is extracted into memory, the settings should be set to reduce the amount of memory used. The settings are summarized in the “Memory & Performance” page.


### Q. The menu items “Delete” and “Rename” are grayed out and cannot be executed.

A. Turn on the menu “Option” > “File management”.

Since its main purpose is to be a viewer, file manipulation is initially disabled to prevent accidental file modification.

### Q. I want to change the criteria for selecting Next Book.

A. Book movement depends on the order of the bookshelf.

Please try changing the order in the bookshelf.


### Q. When I try to display a very large image, “There was not enough memory to continue program execution.” is displayed and the image cannot be displayed.

A. Set the “Maximum image size.”

.NET may have exceeded the maximum image size allowed by .

It may be possible to set the “Maximum image size” in the “Memory & Performance” page of the Preferences to an appropriate value, such as 4096 x 4096, and turn on the “Load image size limit” setting to limit the size of the image that is expanded in memory.


### Q. I want to open all subfolders together as a book.

A. There are multiple ways to load subfolders and settings.

* Set “Load subfolders” in the settings “Page settings".
  Set the behavior when the book is opened.

* Select menu “Page” > “Load subfolders”.
  Turn on/off loading subfolders in that book.

* Select “Load subfolders” from the right-click menu on the bookshelf.
  Turn on/off subfolder loading in that book.

* In the bookshelf's “...” menu, “Load subfolders at this location.”  
  Folders in locations with this setting will load subfolders regardless of the default page settings.

* The setting of whether subfolders are automatically loaded when there is only one subfolder is set in “Subfolder” under “Page settings" page in the settings.


### Q. I can set back to previous page and forward to next page by touch position, but can't I set the same by click position?

A. There is a command called “Touch emulate.”

Since this command executes the touch operation at the cursor position, it can be realized by setting the click operation to this command.


### Q. I want to exit at once from full screen with a shortcut key such as “Esc”.

A. There is a command called “Quit application.” This can be accomplished by assigning the Esc key to the shortcut for this command.


### Q. I want to switch between original size display and the other display setting with a single operation.

A. In the parameter setting of the “Fit to window size” command in the “Command Settings” section, there is a switch called “Switch to original size.” This setting is common to all size switching commands, and it is used to switch with the original size.  


### Q. Simultaneous left and right mouse presses are set for commands, but the response is subtle.

A. Simultaneous left and right presses can be either of the following two inputs

* LeftButton+RightClick "Left button down, right click.”
* RightButton+LeftClick "Right button down, left click.”

It depends on which button is released first after simultaneous presses. Because of the variation in this area, it is believed that simultaneous presses are not always responsive.  
As a countermeasure, please try assigning the above two inputs to the same command, which will improve the response.


### Q. I want to assign mouse wheel operation to a command.

A. Similar to setting up shortcuts for mouse buttons, wheel operations can also be registered as shortcuts.

> [!TIP]
> Tilt wheel is also supported.

### Q. Unable to open certain ZIP files

A. Set up to extract a ZIP with 7z.dll.

The standard ZIP decompression process may not be supported.

Turning off “Use ZIP compressed file expansion with standard function” on the “ZIP” page of the settings will use 7z.dll to perform the ZIP decompression, which may allow you to open the file.


### Q. Some videos cannot be played.

A. Try VLC Media Player.

The standard relies on the functionality of .NET to play it back.
As a rule of thumb, videos that can be played on Windows Media Player are roughly compatible.

If you have installed VLC Media Player, you can set its installation folder in the “libVLC directory” on the “Videos” page to play the same files using VLC's functions and supported formats accordingly.

### Q. I want to display HEIF images.

A. Install [HEIF Image Extensions](https://www.microsoft.com/store/apps/9pmmsr1cgpwg) from the Microsoft Store and try it.

### Q. I want to display AVIF images.

A. Install [AV1 Video Extension](https://www.microsoft.com/store/apps/9mvzqvxjbq9v) from the Microsoft Store and try it.

### Q. App crashes when using Susie plugin.

A. There are compatibility issues with Susie plug-ins. We recommend that you limit the plug-ins you use.

* Some plug-ins don't work.
* There is a plugin that crashes when trying to open an image in a compressed file.
* Plug-in combinations may cause problems.

### Q. Cannot open files with UNICODE characters with Susie plugin

A. The file system must have the short name in 8.3 format stored.  

To check if it has been saved, run “dir /x” on the command line.
Administrative privileges are required to change settings. For more information, please search for the keywords “fsutil” and “8dot3name”.
It is safer not to set this up if you are not familiar with it, as it will change the registry and other aspects of the OS system.

