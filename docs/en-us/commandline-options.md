# NeeView Startup options

    > NeeView.exe [Options...] [File or Folder...]

## Options

Name|Description
--|--
-b, --blank|Start up without opening the image file
-h, --help|This help is displayed
-l, --language=\<string\>|Temporary Language (e.g., 'en-US')
-n, --new-window[=off\|on]|Specify whether to start in a new window
-o, --folderlist=\<string\>|Specify the bookshelf location.
-r, --reset-placement|Reset window coordinates
-s, --slideshow[=off\|on]|Specify whether to start a slideshow
-v, --version|Display version information
-x, --setting=\<string\>|Specify the path of the setting file "UserSetting.json"
--clear-registry|Clear registries for NeeView
--script=\<string\>|Executes the specified script file at startup. You can specify a file in the scripts folder by specifying 'script:\foobar.nvjs'.
--window=\<normal\|min\|max\|full\>|Start with the specified window state
--|Indicates the end of option list. Subsequent arguments are considered file names.

## Examples

`> NeeView.exe -s E:\Pictures`

`> NeeView.exe -o "E:\Pictures?search=foobar"`

`> NeeView.exe --window=full`

`> NeeView.exe --setting="C:\MySetting.json" --new-window=off`

