# User's Guide

## Quick Start

Drag and drop image files or folders containing images to load images.  
Use the left and right keys or the slider at the bottom of the screen to switch between images.

## Concept

The sense of operation is such that folders and archive files are considered **book**, and images are considered **page** each.
Folders and archive files are treated at the same level, and this unit is considered a **book**.  

## Settings Window

The menu “Options” > “Settings” opens the settings window.

NeeView can be configured in a variety of ways, most of which are done here.

## Supported File Formats

### Image File

It supports standard formats, SVG, and WIC installed in Windows. Supported extensions can be found in the settings.
For example, Windows 10 supports digital camera RAW images (.nef .cr2, etc.) by default.
If the icon in the Explorer is a thumbnail image, it is approximately WIC-compatible.

> [!TIP]  
> Several WIC codecs can be installed from the Microsoft Store.
> Try [HEIF Image Extensions](https://www.microsoft.com/store/apps/9pmmsr1cgpwg) for .heic, .heif images or [AV1 Video Extension](https://www.microsoft.com/store/apps/9mvzqvxjbq9v) for .avif images. 

### Archive File

Various compressed files and PDF files are supported. Supported file extensions can be checked in the settings.
Recursive compressed files are also supported. Password-protected files are not supported.

## Video File

Videos that can be played on Windows Media Player are approximately supported. Supported extensions can be checked in the settings.
If you have VLC Media Player installed, you can also use that function to play them.

The video opens as a book, but can be set to open as a page like a GIF animation.

## Theme

You can change the appearance of the window on the “Window” page of the settings.

Custom themes can also be created. See [Theme File Specification](theme.md) for the format.

## Panels

Various information is displayed on the left and right sides of the screen.

The sidebar icon toggles the panel to be displayed.

Panels are dockable. They can be connected by dragging sidebar icons or panel titles to other panels.

Panels can be made into independent windows. Right-click on the sidebar icon or the title of the panel and select “floating”.

### Bookshelf Panel

It is a filer. The folder or archive file selected here will be opened as a book.

External drag & drop is also supported. Files can be moved by right button drag & drop only when “File management” is enabled.

### PageList Panel

Here is a list of pages in the current book.

### History Panel

History of viewed books.

Books in the history are indicated by a green check mark in the book name on the bookshelf.

### Information Panel

Displays information on the current page.

### Navigator Panel

Manipulate the current display.

The following operations are additionally displayed from the panel's “...” menu
- Thumbnail: Displays a thumbnail of the entire page with a rectangular display area above it.
- Control bar: Controls animations such as GIF animations and video pages.

### Effect Panel

Process the current display.

### Bookmark Panel

Register a book as a **bookmark** so that it can be recalled at any time.

When a book is bookmarked, ★ mark will appear in the book name on the bookshelf.

### Playlist Panel

Register the page in the **playlist** so that it can be recalled at any time.
When a page is registered in the playlist, a mark will appear on the slider.

Playlists are saved as files (.nvpls). This file can be opened as a book.
It can also be opened as a book by selecting “Open as book” from the panel's “...” menu.


## Address Bar

It displays the location of the currently viewed book and provides several functions with buttons.

The buttons on the left side are for history back and forth and reloading, similar to a web browser.  
The buttons on the right provide the ability to switch between some of the book's settings.


## Filmstrip

The function is toggled on and off in the menu View > Filmstrip.

The filmstrip is a list of reduced images on each page, displayed above the slider and linked to the slider.


## MainView

This is the main area where the page is displayed.

A separate window can be created by selecting “View” > “MainView window” from the menu. 


### Mouse Drag

The displayed image can be moved or rotated by dragging with the left and middle mouse buttons.  

You can change the settings in the “Mouse operation" page of the settings.


### Mouse Gesture

Dragging with the right mouse button is a mouse gesture input.

 Mouse gestures for commands can be configured from the "Edit commands” page of the Preferences.


### Loupe

Press and hold the left mouse button for magnification.

During this mode, the display magnification can be changed with the wheel.
In addition, all other mouse operations are disabled.

Detailed settings can be made in the “Loupe” page of the settings.


### Drag & drop support

Accepts drag & drop from external applications.

The following types have been identified

* Drag and drop folders and files from Explorer
* Drag and drop from web browser

> [!NOTE]  
> The behavior may vary depending on the browser.  

### Slideshow

The images in the book are switched at a fixed time.  
Toggle slideshow playback/stop in the menu View > Slideshow.

Set the behavior in the “Slideshow” page of the settings.

## Commands

Almost all functions are organized into “commands,” and all command descriptions and current shortcuts can be viewed and set in the “Edit commands” page of the settings.

Some commands allow individual parameters to be set.
Commands can be duplicated from the context menu. This allows you to create commands with different parameters.

> [!NOTE]
> You can initialize the commands with presets from the “Reset” button in the lower right corner of the settings page, but please note that all previous command settings will be lost.

### Context Menu

By default, it can only be opened by pressing the application key on the image, but it can be opened by another operation by assigning a key to the command “Open context menu”.
For example, assigning it to right-click becomes a general context menu operation.

Context menu items can be edited in the “Context menu” page of the settings.

### Script

The command can be extended by preparing a script file.
See [Script Manual](script-manual.html) for details.
