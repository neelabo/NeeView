# Language resource 

     Culture.restext

.restext is a language resource. **Culture** is the language code.   
To add a new language resource, simply create and deploy this file.

### How to test

Use the ZIP version of NeeView.
Place the .restext you created in the Languages folder and restart NeeView.
You will be able to select it from “Language” in the settings.
 
 ### Pull Request

Once you have added or edited the .restext to this folder, please pull request it.


# .restext format

The character encoding is UTF-8.  

 * Key=Text
 * Key:Pattern=Text

## Key
**Key** is a resource key.  
e.g., "BookAddressInfo.Bookmark=This is bookmark address."

If not defined, fall back to en.restext.

## Text
**Text** is the corresponding text.

If empty, fall back to en.restext.

 **{0}** is replaced by the argument programmatically.  
e.g., "BookAddressInfo.Page={0} pages"

**@Key** specifies a key to be replaced by another resource.  
e.g., "BookConfig.ContentsSpace=Distance between pages in "@PageMode.WidePage" (pixels)"

## Pattern (optional)
**Pattern** is a regular expression to select a variation of the expression depending on the argument. Define it only if necessary.
For example, it is used when the expression changes in the plural.  
e.g., "BookAddressInfo.Page:1={0} page"

## Input key name (optional)

Define only as much as you need.

### Key

General key. Used to display shortcut.

Prefix is "Key." .  
See [.NET Key Enum](https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.key) for available names.

e.g., "Key.Enter=EINGABE"

Set "_Uppercase" to "true" to make all uppercase.

e.g., "Key._Uppercase=true"

### Modifier key

Modifier key. Used to display shortcuts, etc.

Prefix is "ModifierKeys." .  
The available names are as follows

- Alt	
- Control (default: "Ctrl")
- Shift

e.g., "ModifierKeys.Control=STRG"

Set "_Uppercase" to "true" to make all uppercase.

### Mouse button

Mouse button.

Prefix is "MouseButton." .  
The available names are as follows

- Left (default: 'LeftButton')
- Middle (default: 'MiddleButton')
- Right (default: 'RightButton')	
- XButton1
- XButton2

e.g., "MouseButton.Left=LinkeTaste"

### Mouse action

Mouse action.

Prefix is "MouseAction." .  
The available names are as follows

- LeftClick
- RightClick
- MiddleClick
- LeftDoubleClick
- RightDoubleClick
- MiddleDoubleClick
- XButton1Click
- XButton1DoubleClick
- XButton2Click
- XButton2DoubleClick
- WheelUp
- WheelDown
- WheelLeft
- WheelRight

e.g., "MouseAction.LeftClick=Linksklick"

### Mouse direction

Used to display mouse gestures.

Prefix is "MouseDirection." .  
The available names are as follows

- Up (default: "↑")
- Down (default: "↓")
- Left (default: "←")
- Right (default: "→")
- Click

e.g., "MouseDirection.Click=Klick"

### Touch area

Touch area.

Prefix is "TouchArea." .  
The available names are as follows

- TouchL1
- TouchL2
- TouchR1
- TouchR2
- TouchCenter

e.g., "TouchArea.TouchL1=BerührenL1"

# ConvertRestext.ps1

Converts between language files (*.restext) and JSON. This is a utility tool, not a required feature.

See Get-Help for details.

    > Get-Help .\ConvertRestext.ps1