# Theme File Specification v1.0.0

Defines the control color scheme for the theme, in JSON format.

For the ZIP version, the default theme definition files are located in “Libraries/Themes”. Please use it as a reference when creating a custom theme.

### Element: Format

A fixed value indicating that this is a theme file.

    "Format": "NeeView.Theme/1.0.0",

### Element: FormatManual

URL for this page. The application ignores this item.

    "FormatManual": "https://neelabo.github.io/NeeView/en-us/theme.html",

### Element: BasedOn

Inherits the definition. This setting is optional.

If "themes://" is specified, it refers to the default theme file. If the file name is left as it is, it refers to a file in the same folder.

This inheritance can be used to define only the portions of the code that need to be changed.

e.g., Inherit default dark theme

    "BasedOn": "themes://DarkTheme.json",

### Element: Colors

An associative array of “Control Color Definition".

    "Colors": {
        :
    }


## Control Color Definition

### Key

    e.g., Button.MouseOver.Background

Keys are defined by letters and periods.
It is expressed in terms of control name, status, attribute, etc., and indicates where it is to be applied.
The last token is an “attribute” and is important for fallback.

### Value

Format:

    [Color|ReferenceKey](/Opacity)

#### Color

Colors are specified in .NET color specification format: "#AARRGGBB", "#RRGGBB", "#ARGB", "#RGB", color string (e.g., "Red").

e.g., Window.Background is black

    "Window.Background": "#FF000000",

#### ReferenceKey

Specify another key instead of a color and use that color.

e.g., IconButton.Pressed.Border color is the same as Control.Accent

    "IconButton.Pressed.Border": "Control.Accent",

#### Opacity

Opacity is specified by "/[0.0-1.0]".
If omitted, it will be opaque, the same as specifying "/1.0".

e.g., Window.InactiveTitle is 50% translucent color of Window.

    "Window.InactiveTitle": "Window.ActiveTitle/0.5",

## Fallback

Automatically determines a substitute color if the key or value is undefined.

- If the attribute “Foreground” is not defined for AAA, it refers to “Window.Foreground”.
- If the attribute “Background” is not defined for AAA, it refers to “Window.Background”.
- If no other attributes of AAA are defined, refer to “AAA.Background”.

e.g., SideBar.Background is referenced because the value of SideBar.Border is undefined. The same applies if this definition itself does not exist. 

    "SideBar.Border": "",


## List of keys used in the theme

Key|Description
--|--
Window.Background | Window background color
Window.Foreground | Window text color
Window.Border | Window border color
Window.ActiveTitle | Title text color when window is active
Window.InactiveTitle | Title text color when window is inactive
Window.Dialog.Border | Dialog window border color
Control.Background | Generic: Background color of the control
Control.Foreground | Generic: Text color of control
Control.Border | Generic: Border color of control
Control.GrayText | Generic: Gray text color of the control
Control.Accent | Generic: Accent color for control
Control.AccentText | Generic: Text color on control accent color
Control.Focus | Generic: Focus color of control
Control.MouseOver.Background | Generic: Background color when the mouse cursor is over the control
Item.Separator | List item separator color
Item.MouseOver.Background | Background color when the mouse cursor is over a list item
Item.MouseOver.Border | Border color when the mouse cursor is over a list item
Item.Selected.Background | Background color of selected list item
Item.Selected.Border | Border color of selected list item
Item.Inactive.Background | Background color when selected list item is inactive
Item.Inactive.Border | Border color when selected list item is inactive
Button.Background | Background color of button
Button.Foreground | Text color of button
Button.Border | Border color of button
Button.MouseOver.Background | Background color when the mouse cursor is on the button
Button.MouseOver.Border | Border color when mouse cursor is on a button
Button.Checked.Background | Background color when toggle button is checked
Button.Checked.Border |Border color when toggle button is checked
Button.Pressed.Background | Background color when button is pressed
Button.Pressed.Border | Border color when button is pressed
IconButton.Background | Background color of icon button
IconButton.Foreground | Text color of icon button
IconButton.Border | Border color of icon button
IconButton.MouseOver.Background | Background color when the mouse cursor is on the icon button
IconButton.MouseOver.Border | Border color when mouse cursor is on icon button
IconButton.Checked.Background | Background color when toggle icon button is checked
IconButton.Checked.Border | Border color when toggle icon button is checked
IconButton.Pressed.Background | Background color when icon button is pressed
IconButton.Pressed.Border | Border color when icon button is pressed
Slider.Background | Background color of slider control
Slider.Foreground | Text color of slider control
Slider.Border | Border color of slider control
Slider.Thumb | Thumb color of slider control
Slider.Thumb.MouseOver | Thumb color of slider control when the mouse cursor is over the thumb
Slider.Track | Track color of slider control
ScrollBar.Background | Background color of scrollbar
ScrollBar.Foreground | Text color of scrollbar
ScrollBar.Border | Border color of scrollbar
ScrollBar.MouseOver | Color when mouse cursor is on scrollbar
ScrollBar.Pressed | Color when scrollbar is pressed
TextBox.Background | Background color of text box
TextBox.Foreground | Text color of text box
TextBox.Border | Border color of text box
TextBox.MouseOver.Background | Background color when the mouse cursor is over the text box
Menu.Background | Background color of menu
Menu.Foreground | Text color of menu
Menu.Border | Border color of menu
Menu.Separator | Menu separator color
SideBar.Background | Background color of sidebar
SideBar.Foreground | Text color of sidebar
SideBar.Border | Border color of sidebar
Panel.Background | Background color of panel
Panel.Foreground | Text color of panel
Panel.Border | Border color of panel
Panel.Header | Text color of the subtitle of the panel
Panel.Note | Text color of the panel's supplementary description
Panel.Separator | Color of separator in panel
Panel.Splitter | Panel-to-panel separator color
MenuBar.Background | Background color of menu bar
MenuBar.Foreground | Text color of menu bar
MenuBar.Border | Border color of menu bar
MenuBar.Address.Background | Background color of address area of menu bar
MenuBar.Address.Border | Border color of address area of menu bar
BottomBar.Background | Background color of the page slider section
BottomBar.Foreground | Text color of the page slider section
BottomBar.Border | Border color of the page slider section
BottomBar.Slider.Background | Background color of page slider
BottomBar.Slider.Foreground | Text color of page slider
BottomBar.Slider.Border | Border color of page slider
BottomBar.Slider.Thumb | Thumb color of page slider
BottomBar.Slider.Thumb.MouseOver | Thumb color of page slider when the mouse cursor is over the thumb (v43.0)
BottomBar.Slider.Track | Track color of page slider
Toast.Background | Background color of toast notification
Toast.Foreground | Text color of toast notification
Toast.Border | Border color of toast notification
Notification.Background | Background color of main view notification
Notification.Foreground | Text color of main view notification
Thumbnail.Background | Background color of thumbnail
Thumbnail.Foreground | Text color of thumbnail
SelectedMark.Foreground | Color of icon indicating current item
CheckIcon.Foreground | Color of history registered icon
BookmarkIcon.Foreground | Bookmark icon color
PlaylistItemIcon.Foreground | Playlist item icon color